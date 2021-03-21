using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C5600Lab1
{
    public class DMArrayFile<T> : SortableDataStructure<T> where T : IComparable<T>
    {
        private FileStream _fs;
        private int _start_data;
        private Func<byte[], T> _toObject;
        private Func<T, byte[]> _toBuffer;
        private int _n;
        private int _size;

        public override T this[int index]
        {
            get
            {
                if (index < 0 || index < _n) throw new IndexOutOfRangeException();

                _fs.Seek(index * _size + _start_data, SeekOrigin.Begin);
                var buffer = new byte[_size];
                _fs.Read(buffer, 0, buffer.Length);
                return _toObject(buffer);
            }
            set
            {
                if (index < 0 || index < _n) throw new IndexOutOfRangeException();
                var data = _toBuffer(value);
                _fs.Seek(index * _size + _start_data, SeekOrigin.Begin);
                
                if (data.Length <= _size)
                {
                    var dataBuffer = new byte[_size];
                    Array.Copy(data, 0, dataBuffer, 0, data.Length);
                    _fs.Write(dataBuffer, 0, dataBuffer.Length);
                    _fs.Flush();
                }
                else
                    throw new RankException();
                
            }
        }
        public override int Length { get => _n; }

        public DMArrayFile(FileStream fs, Func<byte[], T> toObject, Func<T, byte[]> toBuffer, int n, int size)
        {
            _fs = fs;
            _n = n;
            _size = size;
            _start_data = 2 * sizeof(int);
            _toObject = toObject;
            _toBuffer = toBuffer;
            Clear();
        }
        public DMArrayFile(FileStream fs, Func<byte[], T> toObject, Func<T, byte[]> toBuffer)
        {
            _fs = fs;
            _start_data = 2 * sizeof(int);
            _toObject = toObject;
            _toBuffer = toBuffer;

            var buffer = new byte[2 * sizeof(int)];
            _fs.Seek(0, SeekOrigin.Begin);
            _fs.Read(buffer, 0, buffer.Length);
            _n = BitConverter.ToInt32(buffer, 0);
            _size = BitConverter.ToInt32(buffer, sizeof(int));
        }

        public void Clear()
        {
            _fs.Seek(0, SeekOrigin.Begin);
            _fs.Write(BitConverter.GetBytes(_n), 0, sizeof(int));
            _fs.Write(BitConverter.GetBytes(_size), 0, sizeof(int));
            _fs.Write(new byte[_n * _size], 0, _n*_size);
            _fs.Flush();
        }
        
    }
}
