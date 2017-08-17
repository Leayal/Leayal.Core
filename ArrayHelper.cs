using System;

namespace Leayal
{
    public static class ArrayHelper
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            System.Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static bool ChangeKey<TKey, TValue>(this System.Collections.Generic.IDictionary<TKey, TValue> dict,
                                           TKey oldKey, TKey newKey)
        {
            TValue value;
            if (!dict.TryGetValue(oldKey, out value))
                return false;

            dict.Remove(oldKey);  // do not change order
            dict[newKey] = value;  // or dict.Add(newKey, value) depending on ur comfort
            return true;
        }
    }
}
