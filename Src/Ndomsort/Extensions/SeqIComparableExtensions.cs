// This is an open source non-commercial project. Dear PVS-Studio, please check it. PVS-Studio Static
// Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Nds.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for the <see cref="IEnumerable{T}"/>, where T supports <see cref="IComparable{T}"/>. 
    /// </summary>
    internal static class SeqIComparableExtensions
    {
        public static bool CmpSeqEqual<T>(this IEnumerable<T> Seq, IEnumerable<T> OtherSeq, Comparison<T> Cmp)
        {
            bool isEqual = true, isEnd1, isEnd2;

            using (IEnumerator<T> enum1 = Seq.GetEnumerator(), enum2 = OtherSeq.GetEnumerator())
            {
                while (!(isEnd1 = enum1.MoveNext()) & !(isEnd2 = enum2.MoveNext()))
                {
                    T elem1 = enum1.Current;
                    T elem2 = enum2.Current;

                    if (Cmp(elem1, elem2) != 0)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (!isEnd1 || !isEnd2)
                {
                    isEqual = false;
                }
            }

            return isEqual;
        }

        /// <summary>
        /// Find a minimum and maximum value in the <see cref="IEnumerable{T}"/>. 
        /// </summary>
        /// <param name="Seq">     </param>
        /// <param name="Cmp">      <see cref="Comparison{T}"/>. </param>
        /// <param name="MinValue"> A minimum value of the <paramref name="Seq"/>. </param>
        /// <param name="MaxValue"> A maximum value of the <paramref name="Seq"/>. </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="Seq"/> is null or empty.
        /// </exception>
        public static void MinMax<T>(this IEnumerable<T> Seq, out T MinValue, out T MaxValue, Comparison<T> Cmp)
        {
            if (Seq == null)
            {
                throw new ArgumentNullException(nameof(Seq));
            }

            using (IEnumerator<T> enumerator = Seq.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentException($"The {nameof(Seq)} is empty.", nameof(Seq));
                }

                MinValue = enumerator.Current;
                MaxValue = enumerator.Current;

                ResComp resComp;

                while (enumerator.MoveNext())
                {
                    resComp = ConverterResCmp.ConvertToResCmp(Cmp(enumerator.Current, MinValue));

                    if (resComp == ResComp.LE)
                    {
                        MinValue = enumerator.Current;
                    }
                    else
                    {
                        resComp = ConverterResCmp.ConvertToResCmp(Cmp(enumerator.Current, MaxValue));

                        if (resComp == ResComp.GR)
                        {
                            MaxValue = enumerator.Current;
                        }
                    }
                }
            }
        }
    }
}