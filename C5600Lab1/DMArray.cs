using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C5600Lab1
{
    public class DMArray<T> : SortableDataStructure<T> where T : IComparable<T>
    {
        private T[] arr;

        public override T this[int i] { get => arr[i]; set => arr[i] = value; }
        public override int Length { get => arr.Length; }

        public DMArray(int n) { arr = new T[n]; }

        public void Clear() { arr = new T[arr.Length]; }
        
    }
}
