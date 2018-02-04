// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Nds.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for the <see cref="IEnumerable{T}"/>, where T supports <see cref="IComparable{T}"/>.
    /// </summary>
    internal static class SeqIComparableExtensions
    {
        /// <summary>
        /// Find a minimum and maximum value in the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Must support <see cref="IComparable{T}"/> interface.</typeparam>
        /// <param name="Seq"></param>
        /// <param name="MinValue">A minimum value of the <paramref name="Seq"/>.</param>
        /// <param name="MaxValue">A maximum value of the <paramref name="Seq"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="Seq"/> is null or empty.</exception>
        public static void MinMax<T>(this IEnumerable<T> Seq, out T MinValue, out T MaxValue)
            where T : IComparable<T>
        {
            if (Seq == null)
            {
                throw new ArgumentNullException(nameof(Seq));
            }

            using (IEnumerator<T> enumerator = Seq.GetEnumerator())
            {
                if(!enumerator.MoveNext())
                {
                    throw new ArgumentException($"The {nameof(Seq)} is empty.", nameof(Seq));
                }

                MinValue = enumerator.Current;
                MaxValue = enumerator.Current;

                ResComp resComp;

                while (enumerator.MoveNext())
                {
                    resComp = (ResComp)enumerator.Current.CompareTo(MinValue);

                    if (resComp == ResComp.LE)
                    {
                        MinValue = enumerator.Current;
                    }
                    else
                    {
                        resComp = (ResComp)enumerator.Current.CompareTo(MaxValue);

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
