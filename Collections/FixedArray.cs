using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Leayal.Collections
{
    /// <summary>
    /// Provide an array that will always stay at fixed length by push the item out whenever the items exceed the fixed length in the order First-out-Last-in (FOLI).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class FixedArray<T>
    {
        private T[] innerArray;
        private int index;

        public int Count => this.index;

        public int Length => this.innerArray.Length;

        public FixedArray(int length)
        {
            this.index = 0;
            this.innerArray = new T[length];
        }

        public T[] GetArray() => this.innerArray;

        public void Push(T item)
        {
            if (this.index < this.Length)
            {
                // Appending
                this.innerArray[this.index] = item;
                this.index++;
            }
            else
            {
                // Begin push out
                this.TakeFirstItemOut();
                this.innerArray[this.Length - 1] = item;
            }
        }

        internal void TakeFirstItemOut()
        {
            for (int i = 1; i < this.innerArray.Length; i++)
                this.innerArray[i - 1] = this.innerArray[i];
        }

        internal void PushItemOut()
        {
            this.TakeFirstItemOut();
            this.innerArray[this.Length - 1] = default(T);
        }

        public void CopyTo(T[] target)
        {
            this.CopyTo(target, 0);
        }

        public void CopyTo(T[] target, int targetIndex)
        {
            this.CopyTo(target, targetIndex, this.Length);
        }

        public void CopyTo(T[] target, int targetIndex, int length)
        {
            this.CopyTo(target, targetIndex, 0, length);
        }

        public void CopyTo(T[] target, int targetIndex, int sourceIndex, int length)
        {
            if (target.Length < length)
                throw new InvalidOperationException("The target's length should be equal or bigger than the buffer.");

            if (targetIndex >= target.Length)
                throw new IndexOutOfRangeException();
            if (sourceIndex >= this.Length)
                throw new IndexOutOfRangeException();

            int howmuchleft = this.Length - sourceIndex;
            howmuchleft = Math.Min(howmuchleft, length);
            for (int i = 0; i < howmuchleft; i++)
                target[targetIndex + i] = this.innerArray[i + sourceIndex];
        }

        public void Clear()
        {
            for (int i = 0; i < this.Length; i++)
                this.innerArray[i] = default(T);
            this.index = 0;
        }
    }
}
