namespace NdomsortTests
{
    using System;

    internal static class Cmp
    {
        public static int CmpByIComparable<T>(T First, T Second) where T : IComparable<T>
        {
            return First.CompareTo(Second);
        }
    }
}