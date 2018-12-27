using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Common
{
    internal class ResizableArray<T>
    {
        private T[] _array;
        private int _count;
        public int Count => _count;
        internal T[] InternalArray => _array;


        public ResizableArray(int? initialCapacity = null)
        {
            _array = new T[initialCapacity ?? 4];
        }


        public void Add(T element)
        {
            if (_count == _array.Length)
            {
                Array.Resize(ref _array, _array.Length * 2);
            }

            _array[_count++] = element;
        }

        public ArraySegment<T> GetArraySegment()
        {
            return new ArraySegment<T>(_array, 0, _count);
        }

        public T[] ToArray()
        {
            var destinationArray = new T[_count];
            Array.Copy(_array, destinationArray, _count);
            return destinationArray;
        }
    }
}
