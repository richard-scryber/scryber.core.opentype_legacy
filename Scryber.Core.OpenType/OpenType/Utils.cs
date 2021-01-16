using System;
namespace Scryber.OpenType
{
    public static class Utils
    {
        public static T[] CloneArray<T>(T[] original, int newArrLenExtend = 0)
        {
            int orgLen = original.Length;
            T[] newClone = new T[orgLen + newArrLenExtend];
            Array.Copy(original, newClone, orgLen);
            return newClone;
        }

        public static T[] ConcatArray<T>(T[] arr1, T[] arr2)
        {
            T[] newArr = new T[arr1.Length + arr2.Length];
            Array.Copy(arr1, 0, newArr, 0, arr1.Length);
            Array.Copy(arr2, 0, newArr, arr1.Length, arr2.Length);
            return newArr;
        }

        public static float ReadF2Dot14(this BigEndianReader reader)
        {
            return ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */
        }
    }
}
