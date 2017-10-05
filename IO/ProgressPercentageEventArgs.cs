using System;

namespace Leayal.IO
{
    public class ProgressPercentageEventArgs : EventArgs
    {
        private byte _percentage;
        public byte Percentage => this._percentage;

        internal void SetPercentage(byte value)
        {
            if (value > 100)
                throw new ArgumentException();
            this._percentage = value;
        }

        public ProgressPercentageEventArgs(byte percent) : base()
        {
            this.SetPercentage(percent);
        }
    }
}
