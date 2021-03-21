using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C5600Lab1
{
    public class DMListFile<T> : SortableDataStructure<T> where T : IComparable<T>
    {
        private FileStream _fs;
        private int _metadataLength;
        private Func<byte[], T> _toObject;
        private Func<T, byte[]> _toBuffer;
        private int _n;
        private int _size;
        private int _head;
        private int _tail;
        private int _capacity;

        public override T this[int index]
        {
            get
            {
                if (index < 0 || index >= _n) throw new IndexOutOfRangeException();

                int pointer = _head;
                for(int i = 0; i < index; i++)
                {
                    _fs.Seek(pointer, SeekOrigin.Begin);
                    byte[] pointerBuffer = new byte[sizeof(int)];
                    _fs.Read(pointerBuffer, 0, sizeof(int));
                    pointer = BitConverter.ToInt32(pointerBuffer,0);
                }

                _fs.Seek(pointer + sizeof(int), SeekOrigin.Begin);

                byte[] objBuffer = new byte[_size - sizeof(int)];
                _fs.Read(objBuffer, 0, objBuffer.Length);
                return _toObject(objBuffer);
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (_n > _capacity * 0.5 ) increaseCapacity(2);
                if (index < 0 || index > _n) throw new IndexOutOfRangeException();

                var data = _toBuffer(value);

                int pointer = _head;
                Console.WriteLine($@"{_head} {_tail}");
                if (index == _n) { 
                   pointer = _tail;
                } 
                else{
                    for (int i = 0; i < index; i++)
                    {
                        _fs.Seek(pointer, SeekOrigin.Begin);
                        byte[] pointerBuffer = new byte[sizeof(int)];
                        _fs.Read(pointerBuffer, 0, sizeof(int));
                        pointer = BitConverter.ToInt32(pointerBuffer, 0);
                    }
                }
                //ONLY IF ADD LAST
                if (data.Length <= _size-sizeof(int))
                {
                    Console.WriteLine("ifworks");
                    var nodeBuffer = new byte[_size];
                    var slot = -1;

                    if (index == _n)
                    {
                        slot = getFreeSlot(_tail);
                        updateCount(1);
                        setTail(slot);
                    }
                    else
                    {
                        _fs.Seek(pointer, SeekOrigin.Begin);
                        var pointerBuffer = new byte[sizeof(int)];
                        _fs.Read(pointerBuffer, 0, sizeof(int));
                        slot = BitConverter.ToInt32(pointerBuffer,0);
                    }
                    Array.Copy(BitConverter.GetBytes(slot), 0, nodeBuffer, 0, sizeof(int));
                    Array.Copy(data, 0, nodeBuffer, sizeof(int), data.Length);
                    _fs.Seek(pointer, SeekOrigin.Begin);
                    _fs.Write(nodeBuffer, 0, _size);
                    _fs.Flush();
                }
                else
                    throw new RankException();

            }
        }



        /*  public void Add(T item) => list.Add(item);
          public void Clear() => list.Clear();
          public bool Contains(T item) => list.Contains(item);
          public void RemoveAt(int index) => list.RemoveAt(index);*/
        public DMListFile(FileStream fs, Func<byte[], T> toObject, Func<T, byte[]> toBuffer, int size)
        {
            _fs = fs;
            _toObject = toObject;
            _toBuffer = toBuffer;
            _metadataLength = 4 * sizeof(int);
            _n = 0;
            _size = size + sizeof(int);
            int reservedSlot = _metadataLength;
            _head = reservedSlot;
            _tail = reservedSlot;
            _capacity = 100 > _n * 2 ? 10 : _n * 2;
            Console.WriteLine("EE");
            Clear();
        }
        public DMListFile(FileStream fs, Func<byte[], T> toObject, Func<T, byte[]> toBuffer)
        {
            _fs = fs;
            _toObject = toObject;
            _toBuffer = toBuffer;
            _metadataLength = 4 * sizeof(int);

            var buffer = new byte[4 * sizeof(int)];
            _fs.Seek(0, SeekOrigin.Begin);
            _fs.Read(buffer, 0, buffer.Length);
            _n = BitConverter.ToInt32(buffer, 0);
            _size = BitConverter.ToInt32(buffer, sizeof(int));
            _head = BitConverter.ToInt32(buffer, 2 * sizeof(int));
            _tail = BitConverter.ToInt32(buffer, 3 * sizeof(int));
            _capacity = 100 > _n * 2 ? 10 : _n * 2;

        }
        public void Add(T value)
        {
            this[Length] = value;
        }

        public override int Length { get => _n; }

        public void Clear()
        {
            Console.WriteLine("clearing");
            _fs.SetLength(0);
            _n = 0;
            _fs.Seek(0, SeekOrigin.Begin);
            _fs.Write(BitConverter.GetBytes(0), 0, sizeof(int));
            _fs.Write(BitConverter.GetBytes(_size), 0, sizeof(int));
            _fs.Write(BitConverter.GetBytes(_head), 0, sizeof(int));
            _fs.Write(BitConverter.GetBytes(_tail), 0, sizeof(int));
            _fs.Write(new byte[_capacity * _size], 0, _capacity * _size);
            _fs.Flush();
        }
        
        public bool Contains(T obj)
        {
            int currentPtr = _head;
            byte[] objBytes = _toBuffer(obj);

            for (int i = 0; i < _n; i++)
            {
                _fs.Seek(currentPtr, SeekOrigin.Begin);
                byte[] pointerBuffer = new byte[sizeof(int)];
                byte[] objBuffer = new byte[_size - sizeof(int)];

                _fs.Read(pointerBuffer, 0, sizeof(int));
                _fs.Read(objBuffer, 0, _size - sizeof(int));
                currentPtr = BitConverter.ToInt32(pointerBuffer, 0);
                if (objBytes.SequenceEqual(objBuffer)) return true;
            }
            return false;
        }
        public void Insert(T obj, int index)
        {
            if (index < 0 || index >= _n) throw new IndexOutOfRangeException();
            int objPtr = getFreeSlot();
            int nextPtr = getPtr(index);
            byte[] objBytes = _toBuffer(obj);
            if (index != 0)
            {
                int prevPtr = getPtr(index - 1);
                _fs.Seek(prevPtr, SeekOrigin.Begin);
                _fs.Write(BitConverter.GetBytes(objPtr), 0, sizeof(int));
                _fs.Flush();

            }
            _fs.Seek(objPtr, SeekOrigin.Begin);
            _fs.Write(BitConverter.GetBytes(nextPtr), 0, sizeof(int));
            _fs.Write(objBytes, 0, _size - sizeof(int));
            _fs.Flush();

            updateCount(1);
            if (index == 0)
            {
                setHead(objPtr);
            }
        }

        private int getFreeSlot(params int[] reservedSlots)
        {
            var rnd = new Random();
            int ptr = -1;
            while (true)
            {

                ptr = ((int)( rnd.NextDouble() * _capacity ) ) * _size + _metadataLength;
                Console.WriteLine($@"{_capacity} {_size} {_metadataLength}");
                _fs.Seek(ptr, SeekOrigin.Begin);
                byte[] slotBuffer = new byte[_size];
                _fs.Read(slotBuffer, 0, _size);
                Console.WriteLine(slotBuffer.Length);
                bool found = true;
                Console.WriteLine($@"{(ptr-16)/_size} {_n} {BitConverter.ToString(slotBuffer,0)} {reservedSlots}");
                string aaaa = "";
                foreach (byte b in slotBuffer)
                {
                    aaaa += b;
                    if (b != 0) { found = false; Console.WriteLine($@"shit fuck {aaaa}"); break; }
                }
                foreach(int slot in reservedSlots)
                {
                    if (ptr == slot) { found = false; Console.WriteLine("ooooooooooooo"); break; }
                }
                if (found) {
                    Console.WriteLine("SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                return ptr;

                }
            }
        }
        private void updateCount(int change)
        {
            _n += change;
            _fs.Seek(0, SeekOrigin.Begin);
            _fs.Write(BitConverter.GetBytes(_n), 0, sizeof(int));
            _fs.Flush();
        }
        private void setHead(int ptr)
        {
            _head = ptr;    
            _fs.Seek(2 * sizeof(int), SeekOrigin.Begin);
            _fs.Write(BitConverter.GetBytes(ptr), 0, sizeof(int));
            _fs.Flush();
        }
        private void setTail(int ptr)
        {
            _tail = ptr;
            _fs.Seek(3 * sizeof(int), SeekOrigin.Begin);
            _fs.Write(BitConverter.GetBytes(ptr), 0, sizeof(int));
            _fs.Flush();
        }
        private void increaseCapacity(double modifier)
        {
            var byteCount = (int)(_capacity * modifier - _capacity);
            var buffer = new byte[byteCount];
            _fs.Seek(0, SeekOrigin.End);
            _fs.Write(buffer, 0, buffer.Length);
            _fs.Flush();
            _capacity = (int)(_capacity * modifier);
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= _n) throw new IndexOutOfRangeException();
            int removePtr = getPtr(index); 
            if (index == 0)
            {
                if (_n > 1) setHead(getPtr(1));
                wipeNode(removePtr);
                updateCount(-1);
            }
            else if(index == _n-1)
            {
                int previousPtr = getPtr(index - 1);
                updateCount(-1);
                wipeNode(removePtr);
                _fs.Seek(previousPtr, SeekOrigin.Begin);
                _fs.Write(BitConverter.GetBytes(_tail), 0, sizeof(int));
                _fs.Flush();

            }
            else
            {
                int previousPtr = getPtr(index - 1);
                int nextPtr = getPtr(index + 1);
                wipeNode(removePtr);
                updateCount(-1);
                _fs.Seek(previousPtr, SeekOrigin.Begin);
                _fs.Write(BitConverter.GetBytes(nextPtr),0,sizeof(int));
                _fs.Flush();
            }
        }

        private void wipeNode(int removePtr)
        {
            var zeroBytes = new byte[_size];

            _fs.Seek(removePtr, SeekOrigin.Begin);
            _fs.Write(zeroBytes, 0, _size);
            _fs.Flush();
        }

        private int getPtr(int index)
        {
            if (index < 0 || index > _n) throw new IndexOutOfRangeException();

            int pointer = _head;
            for (int i = 0; i < index; i++)
            {
                _fs.Seek(pointer, SeekOrigin.Begin);
                byte[] pointerBuffer = new byte[sizeof(int)];
                _fs.Read(pointerBuffer, 0, sizeof(int));
                pointer = BitConverter.ToInt32(pointerBuffer, 0);
            }
            return pointer;
        }
        public T[] toArray()
        {
            T[] arr = new T[_n];
            int currentPtr = _head;

            for (int i = 0; i < _n; i++)
            {
                _fs.Seek(currentPtr, SeekOrigin.Begin);
                byte[] pointerBuffer = new byte[sizeof(int)];
                byte[] objBuffer = new byte[_size - sizeof(int)];

                _fs.Read(pointerBuffer, 0, sizeof(int));
                _fs.Read(objBuffer, 0, _size - sizeof(int));
                arr[i] = _toObject(objBuffer);
                currentPtr = BitConverter.ToInt32(pointerBuffer, 0);
            }
            return arr;
        }
    }
}
