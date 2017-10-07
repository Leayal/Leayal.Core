using System.Collections;
using System.Collections.Generic;

namespace Leayal.Collections
{
    class IEnumeratorWalker<T> : IEnumerable<T>
    {
        private IEnumerator<T> source;
        public IEnumeratorWalker(IEnumerator<T> ienumerator)
        {
            this.source = ienumerator;
        }
        public IEnumerator<T> GetEnumerator() => this.source;

        IEnumerator IEnumerable.GetEnumerator() => this.source;
    }

    public static class IEnumerableConveter
    {
        public static IEnumerable<T> ConvertFrom<T>(IEnumerator<T> ienumerator)
        {
            return (new IEnumeratorWalker<T>(ienumerator));
        }
        public static IEnumerable<string> AsIEnumerable(this IEnumerator<string> ienumerator) => ConvertFrom(ienumerator);
        public static IEnumerable<byte> AsIEnumerable(this IEnumerator<byte> ienumerator) => ConvertFrom(ienumerator);
        public static IEnumerable<int> AsIEnumerable(this IEnumerator<int> ienumerator) => ConvertFrom(ienumerator);
        public static IEnumerable<short> AsIEnumerable(this IEnumerator<short> ienumerator) => ConvertFrom(ienumerator);
        public static IEnumerable<long> AsIEnumerable(this IEnumerator<long> ienumerator) => ConvertFrom(ienumerator);
    }
}
