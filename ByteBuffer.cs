using System;
using Leayal.IO;

namespace Leayal
{
    /// <summary>
    /// Provide a byte[] buffer from memory pool.
    /// </summary>
    public class ByteBuffer : IDisposable
    {
        private RecyclableMemoryStream memStream;
        private byte[] buffer;
        /// <summary>
        /// Acquire the buffer from the pool. If the pool has no free buffer left, it will create a new one.
        /// </summary>
        /// <param name="capacity">The length of the buffer.</param>
        public ByteBuffer(int capacity)
        {
            this.memStream = new RecyclableMemoryStream(string.Empty, capacity);
            if (this.memStream.Capacity != capacity)
                this.memStream.Capacity = capacity;
        }

        public int Length => this.GetBuffer().Length;
        public long LongLength => this.GetBuffer().LongLength;

        /// <summary>
        /// Return the byte[] buffer.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            if (this.buffer != null)
                return this.buffer;
            this.buffer = this.memStream.GetBuffer();
            return this.buffer;
        }

        /// <summary>
        /// Return the buffer to memory pool.
        /// </summary>
        public void Dispose()
        {
            if (this.memStream != null)
                this.memStream.Dispose();
        }

        static public implicit operator byte[](ByteBuffer byteBuffer)
        {
            return byteBuffer.GetBuffer();
        }
    }
}
