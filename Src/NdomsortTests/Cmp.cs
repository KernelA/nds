// This is an open source non-commercial project. Dear PVS-Studio, please check it. PVS-Studio Static
// Code Analyzer for C, C++ and C#: http://www.viva64.com
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