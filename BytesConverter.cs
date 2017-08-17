using System;
using System.Runtime.InteropServices;

namespace Leayal
{
    internal unsafe class BytesConverter : IDisposable
    {
        private bool _disposed;
        private uint[] _lookup32Unsafe;
        private GCHandle inneralloc;
        private uint* _lookup32UnsafeP;

        public BytesConverter()
        {
            this._lookup32Unsafe = CreateLookup32Unsafe();
            this.inneralloc = GCHandle.Alloc(_lookup32Unsafe, GCHandleType.Pinned);
            this._lookup32UnsafeP = (uint*)this.inneralloc.AddrOfPinnedObject();
        }

        private uint[] CreateLookup32Unsafe()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                if (BitConverter.IsLittleEndian)
                    result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
                else
                    result[i] = ((uint)s[1]) + ((uint)s[0] << 16);
            }
            return result;
        }

        public string ToHexString(byte[] bytes)
        {
            if (_disposed)
                throw new ObjectDisposedException("BytesConverter");
            var result = new string((char)0, bytes.Length * 2);
            fixed (byte* bytesP = bytes)
            fixed (char* resultP = result)
            {
                uint* resultP2 = (uint*)resultP;
                for (int i = 0; i < bytes.Length; i++)
                    resultP2[i] = this._lookup32UnsafeP[bytesP[i]];
            }
            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            this.inneralloc.Free();
        }
    }
}
