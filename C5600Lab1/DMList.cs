using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C5600Lab1
{
    public class DMList<T> : SortableDataStructure<T> where T : IComparable<T>
    {
        private List<T> list;

        public override T this[int i] { get => list[i]; set => list[i] = value; }
        public override int Length { get => list.Count; }
        public void Add(T item) => list.Add(item);
        public void Clear() => list.Clear();
        public bool Contains(T item) => list.Contains(item);
        public void RemoveAt(int index) => list.RemoveAt(index);
        public DMList() { list = new List<T>(); }

    }
}
