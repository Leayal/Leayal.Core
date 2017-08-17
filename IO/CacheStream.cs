using System;
using System.IO.Compression;
using System.IO;

namespace Leayal.IO
{
    public class CacheStream : Stream
    {
        private Stream innerStream;
        internal CacheStream(Stream baseStream)
        {
            this.innerStream = baseStream;
            if (baseStream.Length > 0)
            {
                if (baseStream.Length == 1)
                    this.innerStream.Position = 1;
                else
                {
                    this.innerStream.Position = 0;
                    BinaryReader br = new BinaryReader(this.innerStream);
                    if (br.ReadBoolean())
                        this.innerStream = new DeflateStream(this.innerStream, CompressionMode.Decompress, false);
                }
            }
        }

        public override bool CanRead => true;

        public override bool CanSeek => this.innerStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                DeflateStream ds = this.innerStream as DeflateStream;
                if (ds != null)
                    return (ds.BaseStream.Length - 1);
                else
                    return (this.innerStream.Length - 1);
            }
        }

        public override long Position
        {
            get {
                DeflateStream ds = this.innerStream as DeflateStream;
                if (ds != null)
                    return (ds.BaseStream.Position - 1);
                else
                    return (this.innerStream.Position - 1);
            }
            set {
                DeflateStream ds = this.innerStream as DeflateStream;
                if (ds != null)
                    ds.BaseStream.Position = (value + 1);
                else
                    this.innerStream.Position = (value + 1);
            }
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerStream.Seek(offset + 1, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            this.innerStream.Close();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                this.innerStream.Dispose();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.innerStream.EndRead(asyncResult);
        }

        public override int ReadByte()
        {
            return this.innerStream.ReadByte();
        }

        public override int ReadTimeout
        {
            get
            {
                DeflateStream ds = this.innerStream as DeflateStream;
                if (ds != null)
                    return ds.BaseStream.ReadTimeout;
                else
                    return this.innerStream.ReadTimeout;
            }

            set
            {
                DeflateStream ds = this.innerStream as DeflateStream;
                if (ds != null)
                    ds.BaseStream.ReadTimeout = value;
                else
                    this.innerStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                DeflateStream ds = this.innerStream as DeflateStream;
                if (ds != null)
                    return ds.BaseStream.WriteTimeout;
                else
                    return this.innerStream.WriteTimeout;
            }

            set
            {
                DeflateStream ds = this.innerStream as DeflateStream;
                if (ds != null)
                    ds.BaseStream.WriteTimeout = value;
                else
                    this.innerStream.WriteTimeout = value;
            }
        }

        public override bool CanTimeout => false;

        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
    }
}
