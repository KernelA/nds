// This is an open source non-commercial project. Dear PVS-Studio, please check it. PVS-Studio Static
// Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Nds.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for the <see cref="IEnumerable{T}"/>. 
    /// </summary>
    internal static class SeqIComparableExtensions
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Seq"></param>
        /// <param name="OtherSeq"></param>
        /// <param name="Cmp"></param>
        /// <returns>
        /// <para>
        /// true, if <paramref name="Seq"/> and <paramref name="OtherSeq"/> are empty or they have
        /// equal size and their elements are equal, otherwise false.
        /// </para>
        /// </returns>
        public static bool CmpSeqEqual<T>(this IEnumerable<T> Seq, IEnumerable<T> OtherSeq, Comparison<T> Cmp)
        {
            bool isEqual = true, isNotEnd1, isNotEnd2;

            using (IEnumerator<T> enum1 = Seq.GetEnumerator(), enum2 = OtherSeq.GetEnumerator())
            {
                while ((isNotEnd1 = enum1.MoveNext()) & (isNotEnd2 = enum2.MoveNext()))
                {
                    T elem1 = enum1.Current;
                    T elem2 = enum2.Current;

                    if (ConverterResCmp.ConvertToResCmp(Cmp(elem1, elem2)) != ResComp.EQ)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (!isNotEnd1 || !isNotEnd2)
                {
                    isEqual = false;
                }
            }

            return isEqual;
        }

        /// <summary>
        /// Find a minimum and maximum value in the <see cref="IEnumerable{T}"/>. 
        /// </summary>
        /// <param name="Seq"></param>
        /// <param name="Cmp"> <see cref="Comparison{T}"/>. </param>
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