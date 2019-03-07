// This is an open source non-commercial project. Dear PVS-Studio, please check it. PVS-Studio Static
// Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Nds.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nds;

    /// <summary>
    /// The additional methods for the <see cref="Ndsort{TObj}"/>. 
    /// </summary>
    public static class Stools
    {

        /// <summary>
        /// Swap the elements <paramref name="item1"/> and <paramref name="item2"/>. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item1"> The first element. </param>
        /// <param name="item2"> The second element. </param>
        private static void Swap<T>(ref T item1, ref T item2)
        {
            T temp = item1;
            item1 = item2;
            item2 = temp;
        }

        /// <summary>
        /// Find median of sequence, if length of sequence is odd, otherwise the sequence has two
        /// median. The median is the smallest value from them.
        /// </summary>
        /// <remarks>Time complexity is O(n) in mean, where n is length of the <paramref name="Seq"/>. </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="Seq"></param>
        /// <param name="Cmp"> <see cref="Comparer{T}"/> </param>
        /// <returns>
        /// The "median" (a shallow copy, if <typeparamref name="T"/> is reference type) of sequence.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the length of <paramref name="Seq"/> is 0.
        /// </exception>
        /// <exception cref="ArgumentNullException"> If <paramref name="Seq"/> or <paramref name="Cmp"/> is null. </exception>
        public static T FindLowMedian<T>(IReadOnlyCollection<T> Seq, Comparison<T> Cmp)
        {
            if (Seq == null)
            {
                throw new ArgumentNullException(nameof(Seq));
            }

            if (Cmp == null)
            {
                throw new ArgumentNullException(nameof(Cmp));
            }

            if (Seq.Count == 0)
            {
                throw new ArgumentException($"The length of {nameof(Seq)} is 0.", nameof(Seq));
            }

            if(Seq.Count == 1)
            {
                return Seq.First();
            }
            else
            {
                T[] copySeq = Seq.ToArray();

                int medianIndex = (Seq.Count - 1) / 2, left = 0, right = Seq.Count - 1;
                int i = -1;
                ResComp resComp;


                do
                {
                    T splitElem = copySeq[right];
                    i = left - 1;

                    for (int j = left; j <= right; j++)
                    {
                        resComp = ConverterResCmp.ConvertToResCmp(Cmp(copySeq[j], splitElem));

                        if (resComp == ResComp.LE || resComp == ResComp.EQ)
                        {
                            i++;
                            Swap(ref copySeq[i], ref copySeq[j]);
                        }
                    }

                    if (i < medianIndex)
                    {
                        left = i + 1;
                    }
                    else if (i > medianIndex)
                    {
                        right = i - 1;
                    }
                }
                while (medianIndex != i);

                return copySeq[i];
            }
        }

        /// <summary>
        /// <para> Check. Does a <paramref name="LeftVec"/> dominate a <paramref name="RightVec"/>? </para>
        /// <para>
        /// A <paramref name="LeftVec"/> dominates a <paramref name="RightVec"/>, if and only if
        /// <paramref name="LeftVec"/>[i] &lt;= <paramref name="RightVec"/>[i], for all i in
        /// {0,1,..., len( <paramref name="LeftVec"/>) - 1}, and there exists j in {0,1,...,len(
        /// <paramref name="LeftVec"/>) - 1}: <paramref name="LeftVec"/>[j] &lt; <paramref name="RightVec"/>[j].
        /// </para>
        /// </summary>
        /// <remarks>
        /// If <paramref name="LeftVec"/> and <paramref name="RightVec"/> have the length is 0, then
        /// the result is false.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="LeftVec"> A first vector of the values. </param>
        /// <param name="RightVec"> A second vector of the values. </param>
        /// <param name="Cmp"> <see cref="Comparison{T}"/> </param>
        /// <returns>
        /// True, if <paramref name="LeftVec"/> dominates a <paramref name="RightVec"/>, otherwise False.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="LeftVec"/> and <paramref name="RightVec"/> have different lengths.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="LeftVec"/> or <paramref name="RightVec"/> is null.
        /// </exception>
        public static bool IsDominate<T>(IEnumerable<T> LeftVec, IEnumerable<T> RightVec, Comparison<T> Cmp)
        {
            if (LeftVec == null)
            {
                throw new ArgumentNullException(nameof(LeftVec));
            }

            if (RightVec == null)
            {
                throw new ArgumentNullException(nameof(RightVec));
            }

            if (LeftVec.Count() != RightVec.Count())
            {
                throw new ArgumentException($"{nameof(LeftVec)} must have a same length as {nameof(RightVec)}.");
            }

            bool isAllValuesLessOrEqual = true;
            bool isOneValueLess = false;
            ResComp resComp;

            using (IEnumerator<T> enumLeft = LeftVec.GetEnumerator(), enumRight = RightVec.GetEnumerator())
            {
                while (enumLeft.MoveNext() && enumRight.MoveNext())
                {
                    T leftItem = enumLeft.Current;
                    T rightItem = enumRight.Current;

                    resComp = ConverterResCmp.ConvertToResCmp(Cmp(leftItem, rightItem));

                    if (resComp == ResComp.LE)
                    {
                        isOneValueLess = true;
                    }
                    else if (resComp == ResComp.GR)
                    {
                        isAllValuesLessOrEqual = false;
                        break;
                    }
                }
            }

            return isAllValuesLessOrEqual && isOneValueLess;
        }
    }
}