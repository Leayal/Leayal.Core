using System;
using System.IO;

namespace Leayal.IO
{
    public class RecyclableMemoryStream : MemoryStream
    {
        private Microsoft.IO.RecyclableMemoryStream innerMemStream;

        public RecyclableMemoryStream() : this(AppInfo.MemoryStreamManager) { }

        public RecyclableMemoryStream(string tag)
        {
            this.innerMemStream = new Microsoft.IO.RecyclableMemoryStream(AppInfo.MemoryStreamManager, tag);
        }

        public RecyclableMemoryStream(string tag, int requestedSize)
        {
            this.innerMemStream = new Microsoft.IO.RecyclableMemoryStream(AppInfo.MemoryStreamManager, tag, requestedSize);
        }

        public RecyclableMemoryStream(Microsoft.IO.RecyclableMemoryStreamManager manager)
        {
            this.innerMemStream = new Microsoft.IO.RecyclableMemoryStream(manager);
        }

        public RecyclableMemoryStream(Microsoft.IO.RecyclableMemoryStreamManager manager, string tag)
        {
            this.innerMemStream = new Microsoft.IO.RecyclableMemoryStream(manager, tag);
        }

        public RecyclableMemoryStream(Microsoft.IO.RecyclableMemoryStreamManager manager, string tag, int requestedSize)
        {
            this.innerMemStream = new Microsoft.IO.RecyclableMemoryStream(manager, tag, requestedSize);
        }

        public override bool CanRead => this.innerMemStream.CanRead;

        public override bool CanSeek => this.innerMemStream.CanSeek;

        public override bool CanWrite => this.innerMemStream.CanWrite;

        public override long Length => this.innerMemStream.Length;

        public override long Position
        {
            get { return this.innerMemStream.Position; }
            set { this.innerMemStream.Position = value; }
        }

        public override void Flush()
        {
            this.innerMemStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.innerMemStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerMemStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.innerMemStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.innerMemStream.Write(buffer, offset, count);
        }

        public override byte[] GetBuffer()
        {
            return this.innerMemStream.GetBuffer();
        }

        public override byte[] ToArray()
        {
            return this.innerMemStream.ToArray();
        }

        public override void WriteByte(byte value)
        {
            this.innerMemStream.WriteByte(value);
        }

        public override int ReadByte()
        {
            return this.innerMemStream.ReadByte();
        }

        public override void Close()
        {
            this.innerMemStream.Close();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.innerMemStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.innerMemStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.innerMemStream.EndRead(asyncResult);
        }

        public override int ReadTimeout
        {
            get { return this.innerMemStream.ReadTimeout; }
            set { this.innerMemStream.ReadTimeout = value; }
        }

        public override bool CanTimeout => this.innerMemStream.CanTimeout;

        public override int WriteTimeout
        {
            get { return this.innerMemStream.WriteTimeout; }
            set { this.innerMemStream.WriteTimeout = value; }
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.innerMemStream.EndWrite(asyncResult);
        }

        public override string ToString()
        {
            return this.innerMemStream.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.innerMemStream.Dispose();
        }
    }
}
