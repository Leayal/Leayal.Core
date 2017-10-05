using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Leayal.IO
{
    /// <summary>
    /// Provides a class that will help you scan a binary file.
    /// </summary>
    public sealed class BinaryScanner : IDisposable
    {
        public static BinaryScanner FromFile(string filepath) => BinaryScanner.FromFile(filepath, System.IO.FileAccess.ReadWrite);
        public static BinaryScanner FromFile(string fileToRead, System.IO.FileAccess fileToReadAccess)
        {
            if (System.IO.File.Exists(fileToRead))
            {
                System.IO.FileStream fs = new System.IO.FileStream(fileToRead, System.IO.FileMode.Open, fileToReadAccess);
                return new BinaryScanner(fs);
            }
            else
                throw new System.IO.FileNotFoundException();
        }

        public static BinaryScanner FromFile(string fileToRead, string fileToWrite)
        {
            if (System.IO.File.Exists(fileToRead))
            {
                System.IO.FileStream fs = System.IO.File.OpenRead(fileToRead);
                Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(fileToWrite));
                System.IO.FileStream ofs = System.IO.File.Create(fileToWrite);
                return new BinaryScanner(fs, ofs);
            }
            else
                throw new System.IO.FileNotFoundException();
        }

        private bool _leaveOpen;
        private ProgressPercentageEventArgs eventarg;

        public System.IO.Stream BaseStream { get; }
        public System.IO.Stream WriteStream { get; }
        internal bool _isscanning;
        public bool IsScanning => this._isscanning;

        /// <summary>
        /// Initialize new BinaryScanner with one stream that will read and writing.
        /// </summary>
        /// <param name="content">The stream which will be used to scan and write.</param>
        public BinaryScanner(System.IO.Stream content) : this(content, false) { }
        /// <summary>
        /// Initialize new BinaryScanner with one stream that will read and writing.
        /// </summary>
        /// <param name="content">The stream which will be used to scan and write.</param>
        /// <param name="leaveOpen">Determine if the stream will be disposed when <see cref="BinaryScanner"/> is disposed.</param>
        public BinaryScanner(System.IO.Stream content, bool leaveOpen) : this(content, content, leaveOpen) { }
        /// <summary>
        /// Initialize new BinaryScanner with the reading stream and writing stream different from each other.
        /// </summary>
        /// <param name="content">The stream which will be used to scan.</param>
        /// <param name="writeStream">The stream which will be used to write out</param>
        public BinaryScanner(System.IO.Stream content, System.IO.Stream writeStream) : this(content, writeStream, false) { }
        /// <summary>
        /// Initialize new BinaryScanner with the reading stream and writing stream different from each other.
        /// </summary>
        /// <param name="content">The stream which will be used to scan.</param>
        /// <param name="writeStream">The stream which will be used to write out</param>
        /// <param name="leaveOpen">Determine if the stream will be disposed when <see cref="BinaryScanner"/> is disposed.</param>
        public BinaryScanner(System.IO.Stream content, System.IO.Stream writeStream, bool leaveOpen)
        {
            this._leaveOpen = leaveOpen;
            this.BaseStream = content;
            this.WriteStream = writeStream;
            this._isscanning = false;
            this.eventarg = new ProgressPercentageEventArgs(0);
        }

        public IEnumerable<BinaryScanResult> Scan(string hex)
        {
            if (this.IsScanning)
                throw new InvalidOperationException();
            return this.Scan(ByteHelper.FromHexString(hex));
        }

        public IEnumerable<BinaryScanResult> Scan(params byte[] bytes)
        {
            if (this.IsScanning)
                throw new InvalidOperationException();

            if (this.BaseStream == this.WriteStream)
                return new BinaryScanResultsWalker(new BinaryScanResults_SameStream(this, bytes));
            else
                return new BinaryScanResultsWalker(new BinaryScanResults_DifferentStreams(this, bytes));
        }

        public IEnumerable<BinaryScanResult> Scan(string str, Encoding encoding)
        {
            if (this.IsScanning)
                throw new InvalidOperationException();
            return this.Scan(encoding.GetBytes(str));
        }

        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            if (!this._leaveOpen)
            {
                this.BaseStream.Dispose();
                if (this.BaseStream != this.WriteStream)
                    this.WriteStream.Dispose();
            }
        }

        public event EventHandler<ProgressPercentageEventArgs> ProgressPercentage;
        private void OnProgressPercentage(ProgressPercentageEventArgs e)
        {
            this.ProgressPercentage?.Invoke(this, e);
        }

        internal bool IsProgressPercentageEventNull()
        {
            return (this.ProgressPercentage == null);
        }

        internal void RaiseProgressPercentageEvent(byte value)
        {
            if (this.eventarg.Percentage != value)
            {
                this.eventarg.SetPercentage(value);
                this.OnProgressPercentage(this.eventarg);
            }
        }
    }

    public sealed class BinaryScanResult
    {
        private long pos;
        private System.IO.Stream stream;
        public byte[] OriginalResult { get; }
        private byte[] _result;
        public byte[] Result => this._result;
        private string _hexResult;
        public string HexResult
        {
            get
            {
                if (string.IsNullOrEmpty(this._hexResult))
                    this._hexResult = ByteHelper.ToHexString(this.Result);
                return this._hexResult;
            }
        }
        
        internal BinaryScanResult(System.IO.Stream stream, long position, byte[] value)
        {
            this.pos = position;
            this.stream = stream;
            this.OriginalResult = value;
            this._result = value;
            this._hexResult = null;
        }

        public long Offset => this.pos;

        public void Replace(byte[] newValues)
        {
            this.Replace(newValues, 0);
        }

        public void Replace(byte[] newValues, int offset)
        {
            this.Replace(newValues, offset, newValues.Length);
        }

        public void Replace(byte[] newValues, int offset, int count)
        {
            if (!this.stream.CanWrite)
                throw new InvalidOperationException("The stream must be writable.");
            if (!this.stream.CanSeek)
                throw new InvalidOperationException("The stream must be seekable.");

            if (count > this.OriginalResult.Length)
                throw new ArgumentException("The new value should have the same length of the original. Try to fix the length with null byte.");

            long lastknownPos = this.stream.Position;

            this.stream.Seek(this.pos, System.IO.SeekOrigin.Begin);
            this.stream.Write(newValues, offset, count);

            this.stream.Seek(lastknownPos, System.IO.SeekOrigin.Begin);
            
            this._result = newValues.SubArray(offset, count);
            this._hexResult = null;
        }
    }

    class BinaryScanResultsWalker : IEnumerable<BinaryScanResult>
    {
        private IEnumerator<BinaryScanResult> source;
        public BinaryScanResultsWalker(IEnumerator<BinaryScanResult> ienumerator)
        {
            this.source = ienumerator;
        }
        public IEnumerator<BinaryScanResult> GetEnumerator() => this.source;

        IEnumerator IEnumerable.GetEnumerator() => this.source;
    }

    /// <summary>
    /// Shhhh....This class didn't get exposed by standard anyway, so don't blame my naming sense. This class has some problem with performance as it has to write the byte out to the WritingStream.
    /// </summary>
    class BinaryScanResults_DifferentStreams : BinaryScanResults_SameStream
    {
        internal BinaryScanResults_DifferentStreams(BinaryScanner scanner, byte[] bytes) : base(scanner, bytes) { }

        protected override bool ScannerRead()
        {
            int reading = this.scanner.BaseStream.ReadByte();
            byte readingByte;
            while (reading > -1)
            {
                readingByte = (byte)reading;
                this.scanner.WriteStream.WriteByte(readingByte);
                this.buffer.Push(readingByte);
                if (!this.scanner.IsProgressPercentageEventNull())
                    this.scanner.RaiseProgressPercentageEvent((byte)Convert.ToInt32((this.scanner.BaseStream.Position * 100d) / this.scanner.BaseStream.Length));
                if (this.buffer.Count == this.bytesToScan.Length)
                    if (this.IsMatch(this.buffer.GetArray()))
                    {
                        byte[] result = new byte[this.bytesToScan.Length];
                        this.buffer.CopyTo(result);
                        this._current = new BinaryScanResult(this.scanner.WriteStream, this.scanner.WriteStream.Position - this.bytesToScan.Length, result);
                        return true;
                    }
                reading = this.scanner.BaseStream.ReadByte();
            }
            return false;
        }
    }

    /// <summary>
    /// Shhhh....This class didn't get exposed by standard anyway, so don't blame my naming sense.
    /// </summary>
    class BinaryScanResults_SameStream : IEnumerator<BinaryScanResult>
    {
        protected long first_position;
        protected BinaryScanner scanner;
        protected byte[] bytesToScan;
        protected Collections.FixedArray<byte> buffer;
        
        internal BinaryScanResults_SameStream(BinaryScanner scanner, byte[] bytes)
        {
            this.bytesToScan = bytes;
            this.first_position = scanner.BaseStream.Position;
            this.scanner = scanner;
            this.buffer = new Collections.FixedArray<byte>(bytes.Length);
        }

        protected BinaryScanResult _current;

        public BinaryScanResult Current => this._current;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            this.scanner._isscanning = false;
        }

        public bool MoveNext()
        {
            return this.ScannerRead();
        }

        protected virtual bool ScannerRead()
        {
            int reading = this.scanner.BaseStream.ReadByte();
            while (reading > -1)
            {
                this.buffer.Push((byte)reading);
                if (!this.scanner.IsProgressPercentageEventNull())
                    this.scanner.RaiseProgressPercentageEvent((byte)Convert.ToInt32((this.scanner.BaseStream.Position * 100d) / this.scanner.BaseStream.Length));
                if (this.buffer.Count == this.bytesToScan.Length)
                    if (this.IsMatch(this.buffer.GetArray()))
                    {
                        byte[] result = new byte[this.bytesToScan.Length];
                        this.buffer.CopyTo(result);
                        this._current = new BinaryScanResult(this.scanner.BaseStream, this.scanner.BaseStream.Position - this.bytesToScan.Length, result);
                        return true;
                    }
                reading = this.scanner.BaseStream.ReadByte();
            }
            return false;
        }

        protected bool IsMatch(byte[] target)
        {
            for (int i = 0; i < this.bytesToScan.Length; i++)
                if (target[i] != this.bytesToScan[i])
                    return false;
            return true;
        }

        public void Reset()
        {
            this.scanner._isscanning = true;
            this.scanner.BaseStream.Position = this.first_position;
        }
    }
}
