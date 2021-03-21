using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C5600Lab1
{
    public abstract class SortableDataStructure<T> where T : IComparable<T>
    {
        public abstract T this[int i] { get; set; }
        public abstract int Length { get; }

        public void Swap(int a, int b)
        {
            if ((a < 0) || (b < 0) || (Length <= a) || (Length <= b)) throw new IndexOutOfRangeException();
            T temp = this[b];
            this[b] = this[a];
            this[a] = temp;
        }
        public void BubleSort()
        {
            for (int i = 0; i < Length-1; i++)
            {
                for (int j = i+1; j < Length; j++)
                {
                    if (this[i].CompareTo(this[j]) < 0)
                        Swap(i,j);
                }
            }
        }

    }
}
