// This is an open source non-commercial project. Dear PVS-Studio, please check it. PVS-Studio Static
// Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Nds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Extensions;

    using Tools;

    /// <summary>
    /// Non-dominated sorting. 
    /// </summary>
    /// <remarks>
    /// Original algorithm described in the paper: Buzdalov M., Shalyto A. A Provably Asymptotically
    /// Fast Version of the Generalized Jensen Algorithm for Non-dominated Sorting // Parallel
    /// Problem Solving from Nature XIII.- 2015. - P. 528-537. - (Lecture Notes on Computer Science ; 8672)
    /// </remarks>
    public class Ndsort<TObj>
    {
        private LexicographicCmp<TObj> _lexCmp;

        private Comparison<TObj> _objCmp;

        /// <summary>
        /// Merge the two list of the indices. Each list must be sorted. 
        /// </summary>
        /// <param name="indices1"> A sorted list of the indices. </param>
        /// <param name="indices2"> A sorted list of the indices. </param>
        /// <returns> The ordered list of indices. </returns>
        private LinkedList<int> MergeLists(LinkedList<int> indices1, LinkedList<int> indices2)
        {
            LinkedList<int> mergedList = new LinkedList<int>();

            bool isEnd1 = false;
            bool isEnd2 = false;

            using (LinkedList<int>.Enumerator enum1 = indices1.GetEnumerator(), enum2 = indices2.GetEnumerator())
            {
                isEnd1 = !enum1.MoveNext();
                isEnd2 = !enum2.MoveNext();

                while (!isEnd1 && !isEnd2)
                {
                    if (enum1.Current < enum2.Current)
                    {
                        mergedList.AddLast(enum1.Current);
                        isEnd1 = !enum1.MoveNext();
                    }
                    else
                    {
                        mergedList.AddLast(enum2.Current);
                        isEnd2 = !enum2.MoveNext();
                    }
                }

                if (!isEnd1)
                {
                    mergedList.AddLast(enum1.Current);

                    while (enum1.MoveNext())
                    {
                        mergedList.AddLast(enum1.Current);
                    }
                }

                if (!isEnd2)
                {
                    mergedList.AddLast(enum2.Current);

                    while (enum2.MoveNext())
                    {
                        mergedList.AddLast(enum2.Current);
                    }
                }
            }

            return mergedList;
        }

        /// <summary>
        /// Recursive procedure. It attributes front's indices to all elements in the
        /// <paramref name="SeqUniqObjs"/>, with the indices in the <paramref name="Indices"/>, for
        /// the first <paramref name="CountOfObjs"/> values of the objectives.
        /// </summary>
        /// to all elements in the 
        /// <paramref name="SeqUniqObjs"/>
        /// <param name="SeqUniqObjs"> The sequence of the unique objectives. </param>
        /// <param name="Fronts"> The values of the fronts. </param>
        /// <param name="Indices"> The indices of the <paramref name="SeqUniqObjs"/>. </param>
        /// <param name="CountOfObjs">
        /// The number of the values from the objectives, for the sorting.
        /// </param>
        private void NdHelperA(IReadOnlyList<TObj>[] SeqUniqObjs, int[] Fronts, LinkedList<int> Indices, int CountOfObjs)
        {
            if (Indices.Count < 2)
            {
                return;
            }
            else if (Indices.Count == 2)
            {
                int indexL = Indices.First.Value, indexR = Indices.Last.Value;
                IEnumerable<TObj> seqObjs1 = SeqUniqObjs[indexL].Take(CountOfObjs);
                IEnumerable<TObj> seqObjs2 = SeqUniqObjs[indexR].Take(CountOfObjs);

                if (Stools.IsDominate(seqObjs1, seqObjs2, _objCmp))
                {
                    Fronts[indexR] = Math.Max(Fronts[indexR], Fronts[indexL] + 1);
                }
            }
            else if (CountOfObjs == 2)
            {
                SweepA(SeqUniqObjs, Fronts, Indices);
            }
            else
            {
                var distinctObjs = (from index in Indices
                                    select SeqUniqObjs[index][CountOfObjs - 1]).Distinct().ToArray();

                if (distinctObjs.Length == 1)
                {
                    NdHelperA(SeqUniqObjs, Fronts, Indices, CountOfObjs - 1);
                }
                else
                {
                    TObj median = Stools.FindLowMedian(distinctObjs, _objCmp);

                    LinkedList<int> lessMedian, equalMedian, greaterMedian, lessAndEqual;

                    SplitBy(SeqUniqObjs, Indices, median, CountOfObjs - 1, out lessMedian, out equalMedian, out greaterMedian);

                    lessAndEqual = MergeLists(lessMedian, equalMedian);

                    NdHelperA(SeqUniqObjs, Fronts, lessMedian, CountOfObjs);
                    NdHelperB(SeqUniqObjs, Fronts, lessMedian, equalMedian, CountOfObjs - 1);
                    NdHelperA(SeqUniqObjs, Fronts, equalMedian, CountOfObjs - 1);
                    NdHelperB(SeqUniqObjs, Fronts, lessAndEqual, greaterMedian, CountOfObjs - 1);
                    NdHelperA(SeqUniqObjs, Fronts, greaterMedian, CountOfObjs);
                }
            }
        }

        /// <summary>
        /// Recursive procedure. It attributes a front's index to all elements in the
        /// <paramref name="SeqUniqObjs"/>, with the indices in the <paramref name="AssignIndices"/>,
        /// for the first for the first <paramref name="CountOfObjs"/> values of the objectives, by
        /// comparing them to elements in the <paramref name="SeqUniqObjs"/>, with the indices in the <paramref name="CompIndices"/>.
        /// </summary>
        /// <param name="SeqUniqObjs"> The sequence of the unique objectives. </param>
        /// <param name="Fronts"> The values of the fronts. </param>
        /// <param name="CompIndices"> The indices for comparing. </param>
        /// <param name="AssignIndices"> The indices for assign front. </param>
        /// <param name="CountOfObjs">
        /// The number of the values from the objectives, for the sorting.
        /// </param>
        private void NdHelperB(IReadOnlyList<TObj>[] SeqUniqObjs, int[] Fronts, LinkedList<int> CompIndices, LinkedList<int> AssignIndices, int CountOfObjs)
        {
            if (CompIndices.Count == 0 || AssignIndices.Count == 0)
            {
                return;
            }
            else if (CompIndices.Count == 1 || AssignIndices.Count == 1)
            {
                foreach (int assignIndex in AssignIndices)
                {
                    var hv = SeqUniqObjs[assignIndex].Take(CountOfObjs);

                    foreach (int compIndex in CompIndices)
                    {
                        var lv = SeqUniqObjs[compIndex].Take(CountOfObjs);

                        if (Stools.IsDominate(lv, hv, _objCmp) || lv.CmpSeqEqual(hv, _objCmp))
                        {
                            Fronts[assignIndex] = Math.Max(Fronts[assignIndex], Fronts[compIndex] + 1);
                        }
                    }
                }
            }
            else if (CountOfObjs == 2)
            {
                SweepB(SeqUniqObjs, Fronts, CompIndices, AssignIndices);
            }
            else
            {
                var uniqValuesObjFromCompIndices = (from index in CompIndices
                                                    select SeqUniqObjs[index][CountOfObjs - 1]).Distinct();
                var uniqValuesObjFromAssignIndices = (from index in AssignIndices
                                                      select SeqUniqObjs[index][CountOfObjs - 1]).Distinct();

                TObj minUniqValueObjFromCompIndices, maxUniqValueObjFromCompIndices;
                TObj minUniqValueObjFromAssignIndices, maxUniqValueObjFromAssignIndices;

                uniqValuesObjFromAssignIndices.MinMax(out minUniqValueObjFromAssignIndices, out maxUniqValueObjFromAssignIndices, _objCmp);
                uniqValuesObjFromCompIndices.MinMax(out minUniqValueObjFromCompIndices, out maxUniqValueObjFromCompIndices, _objCmp);

                ResComp resComp;

                resComp = ConverterResCmp.ConvertToResCmp(_objCmp(maxUniqValueObjFromCompIndices, minUniqValueObjFromAssignIndices));

                if (resComp == ResComp.LE || resComp == ResComp.EQ)
                {
                    NdHelperB(SeqUniqObjs, Fronts, CompIndices, AssignIndices, CountOfObjs - 1);
                }
                else
                {
                    resComp = ConverterResCmp.ConvertToResCmp(_objCmp(minUniqValueObjFromCompIndices, maxUniqValueObjFromAssignIndices));

                    if (resComp == ResComp.LE || resComp == ResComp.EQ)
                    {
                        TObj median = Stools.FindLowMedian(uniqValuesObjFromAssignIndices.Union(uniqValuesObjFromCompIndices).ToArray(), _objCmp);

                        LinkedList<int> lessMedian1, equalMedian1, greaterMedian1, lessMedian2, equalMedian2, greaterMedian2;

                        SplitBy(SeqUniqObjs, CompIndices, median, CountOfObjs - 1, out lessMedian1, out equalMedian1, out greaterMedian1);
                        SplitBy(SeqUniqObjs, AssignIndices, median, CountOfObjs - 1, out lessMedian2, out equalMedian2, out greaterMedian2);

                        LinkedList<int> lessAndEqualMedian1 = MergeLists(lessMedian1, equalMedian1);

                        NdHelperB(SeqUniqObjs, Fronts, lessMedian1, lessMedian2, CountOfObjs);
                        NdHelperB(SeqUniqObjs, Fronts, lessMedian1, equalMedian2, CountOfObjs - 1);
                        NdHelperB(SeqUniqObjs, Fronts, equalMedian1, equalMedian2, CountOfObjs - 1);
                        NdHelperB(SeqUniqObjs, Fronts, lessAndEqualMedian1, greaterMedian2, CountOfObjs - 1);
                        NdHelperB(SeqUniqObjs, Fronts, greaterMedian1, greaterMedian2, CountOfObjs);
                    }
                }
            }
        }

        /// <summary>
        /// Non-dominated sorting. 
        /// </summary>
        /// <param name="SeqObjs"> The sequence of the values of the objectives. </param>
        /// <returns> The indices of the fronts. </returns>
        /// <exception cref="ArgumentNullException">
        /// <para> If <paramref name="SeqObjs"/> is null. </para>
        /// <para> If <paramref name="SeqObjs"/> contains null. </para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para> If <paramref name="SeqObjs"/> is empty. </para>
        /// <para> If the number of the objectives is less than 2. </para>
        /// <para>
        /// If the elements in the <paramref name="SeqObjs"/> have not an equal number of the objective.
        /// </para>
        /// </exception>
        private int[] NonDominSortObj(IEnumerable<IReadOnlyList<TObj>> SeqObjs)
        {
            if (SeqObjs == null)
            {
                throw new ArgumentNullException(nameof(SeqObjs));
            }

            // The dictionary contains the sequences of the values of the objectives as keys and
            // their indices in the 'SeqValuesOfObj' as values.
            SortedDictionary<IReadOnlyList<TObj>, LinkedList<int>> uniqueObjAndIndices = new SortedDictionary<IReadOnlyList<TObj>, LinkedList<int>>(_lexCmp);

            int lengthObjs = -1;
            int indexInSeq = 0;

            foreach (var obj in SeqObjs)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException($"The element at position {indexInSeq}" +
                        $" in the {nameof(SeqObjs)} is null.", nameof(SeqObjs));
                }

                if (lengthObjs == -1)
                {
                    lengthObjs = obj.Count;

                    if (lengthObjs < 2)
                    {
                        throw new ArgumentException("The number of the objectives must be > 1.");
                    }
                }

                if (lengthObjs != obj.Count)
                {
                    throw new ArgumentException($"The element at position {indexInSeq} " +
                        $"in the {nameof(SeqObjs)} has length is not equal the length of the element at position 0.");
                }

                if (uniqueObjAndIndices.ContainsKey(obj))
                {
                    uniqueObjAndIndices[obj].AddLast(indexInSeq);
                }
                else
                {
                    uniqueObjAndIndices.Add(obj, new LinkedList<int>());
                    uniqueObjAndIndices[obj].AddLast(indexInSeq);
                }

                indexInSeq++;
            }

            if (indexInSeq == 0)
            {
                throw new ArgumentException("The sequence of values of the objectives is empty.", nameof(SeqObjs));
            }

            int[] resFronts = Enumerable.Repeat(0, indexInSeq).ToArray();

            IReadOnlyList<TObj>[] uniqueObjs = uniqueObjAndIndices.Keys.ToArray();

            // Further, algorithm works only with the indices.
            LinkedList<int> indicesUniqObjs = new LinkedList<int>(Enumerable.Range(0, uniqueObjs.Length));

            int[] fronts = Enumerable.Repeat(0, uniqueObjs.Length).ToArray();

            NdHelperA(uniqueObjs, fronts, indicesUniqObjs, lengthObjs);

            for (int i = 0; i < uniqueObjs.Length; i++)
            {
                foreach (int index in uniqueObjAndIndices[uniqueObjs[i]])
                {
                    resFronts[index] = fronts[i];
                }
            }

            return resFronts;
        }

        /// <summary>
        /// <paramref name="Indices"/> splits into three lists. 
        /// </summary>
        /// <param name="SeqUniqObjs"> The sequence of the unique objectives. </param>
        /// <param name="Indices"> The indices of the <paramref name="SeqUniqObjs"/>. </param>
        /// <param name="SplitValue"> A value for the splitting. </param>
        /// <param name="IndexOfValue"> The index of the value in the objectives, for the split. </param>
        /// <param name="LessSplitValue">
        /// The indices, where the <paramref name="IndexOfValue"/> th value of the objectives is less
        /// than <paramref name="SplitValue"/>.
        /// </param>
        /// <param name="EqualSplitValue">
        /// The indices, where the <paramref name="IndexOfValue"/> th value of the objectives are
        /// equal to <paramref name="SplitValue"/>.
        /// </param>
        /// <param name="GreaterSplitValue">
        /// The indices, where the <paramref name="IndexOfValue"/> th value of the objectives is
        /// greater than <paramref name="SplitValue"/>.
        /// </param>
        private void SplitBy(IReadOnlyList<TObj>[] SeqUniqObjs, LinkedList<int> Indices, TObj SplitValue, int IndexOfValue,
            out LinkedList<int> LessSplitValue, out LinkedList<int> EqualSplitValue, out LinkedList<int> GreaterSplitValue)
        {
            LessSplitValue = new LinkedList<int>();
            ;
            EqualSplitValue = new LinkedList<int>();
            ;
            GreaterSplitValue = new LinkedList<int>();
            ;

            ResComp resComp;

            foreach (int index in Indices)
            {
                resComp = ConverterResCmp.ConvertToResCmp(_objCmp(SeqUniqObjs[index][IndexOfValue], SplitValue));

                if (resComp == ResComp.LE)
                {
                    LessSplitValue.AddLast(index);
                }
                else if (resComp == ResComp.EQ)
                {
                    EqualSplitValue.AddLast(index);
                }
                else
                {
                    GreaterSplitValue.AddLast(index);
                }
            }
        }

        /// <summary>
        /// Two-objective sorting. It attributes front's index to the lexicographically ordered
        /// elements in the <paramref name="SeqUniqObjs"/>, with the indices in the
        /// <paramref name="Indices"/>, based on the first two values of the objectives using a
        /// line-sweep algorithm.
        /// </summary>
        /// <param name="SeqUniqObjs"> The sequence of the unique objectives. </param>
        /// <param name="Fronts"> The values of the fronts. </param>
        /// <param name="Indices"> The indices of the <paramref name="SeqUniqObjs"/>. </param>
        private void SweepA(IReadOnlyList<TObj>[] SeqUniqObjs, int[] Fronts, LinkedList<int> Indices)
        {
            HashSet<int> initInd = new HashSet<int>();

            LinkedList<int> indicesWhereSecondValuesLessOrEqual = new LinkedList<int>();

            using (IEnumerator<int> listEnumer = Indices.GetEnumerator())
            {
                listEnumer.MoveNext();

                initInd.Add(listEnumer.Current);

                int index = 0, maxFront = -1;

                ResComp resComp;

                while (listEnumer.MoveNext())
                {
                    index = listEnumer.Current;

                    indicesWhereSecondValuesLessOrEqual.Clear();

                    foreach (int initIndex in initInd)
                    {
                        resComp = ConverterResCmp.ConvertToResCmp(_objCmp(SeqUniqObjs[initIndex][1], SeqUniqObjs[index][1]));

                        if (resComp == ResComp.LE || resComp == ResComp.EQ)
                        {
                            indicesWhereSecondValuesLessOrEqual.AddLast(initIndex);
                        }
                    }

                    if (indicesWhereSecondValuesLessOrEqual.Count != 0)
                    {
                        maxFront = indicesWhereSecondValuesLessOrEqual.Max(idx => Fronts[idx]);
                        Fronts[index] = Math.Max(Fronts[index], maxFront + 1);
                    }

                    initInd.RemoveWhere(idx => Fronts[idx] == Fronts[index]);
                    initInd.Add(index);
                }
            }
        }

        /// <summary>
        /// Two-objective sorting. It attributes front's index to elements in the
        /// <paramref name="SeqUniqObjs"/>, with the indices in the <paramref name="AssignIndices"/>,
        /// based on the first two values of the objectives, by comparing them to elements in the
        /// <paramref name="SeqUniqObjs"/>, with the indices in the <paramref name="CompIndices"/>,
        /// using a line-sweep algorithm.
        /// </summary>
        /// <param name="SeqUniqObjs"> The sequence of the unique objectives. </param>
        /// <param name="Fronts"> The values of the fronts. </param>
        /// <param name="CompIndices"> The indices for comparing. </param>
        /// <param name="AssignIndices"> The indices for assign front. </param>
        private void SweepB(IReadOnlyList<TObj>[] SeqUniqObjs, int[] Fronts, LinkedList<int> CompIndices, LinkedList<int> AssignIndices)
        {
            HashSet<int> initInd = new HashSet<int>();

            bool isMoveNext = true;

            int compIndex = 0, maxFront = -1;

            TObj[] rightObjs = new TObj[2];
            TObj[] leftObjs = new TObj[2];

            ResComp resComp;

            LinkedList<int> listIndicesWithLessOrEqValue = new LinkedList<int>();

            using (IEnumerator<int> compListEnum = CompIndices.GetEnumerator())
            {
                isMoveNext = compListEnum.MoveNext();

                foreach (int assignIndex in AssignIndices)
                {
                    if (isMoveNext)
                    {
                        rightObjs[0] = SeqUniqObjs[assignIndex][0];
                        rightObjs[1] = SeqUniqObjs[assignIndex][1];
                    }

                    while (isMoveNext)
                    {
                        compIndex = compListEnum.Current;

                        leftObjs[0] = SeqUniqObjs[compIndex][0];
                        leftObjs[1] = SeqUniqObjs[compIndex][1];

                        resComp = ConverterResCmp.ConvertToResCmp(_lexCmp.Compare(leftObjs, rightObjs));

                        if (resComp == ResComp.LE || resComp == ResComp.EQ)
                        {
                            var indicesWithLessValueAndEqualFront = from index in initInd
                                                                    where Fronts[index] == Fronts[compIndex] && ConverterResCmp.ConvertToResCmp(_objCmp(SeqUniqObjs[index][1], SeqUniqObjs[compIndex][1])) == ResComp.LE
                                                                    select index;

                            if (indicesWithLessValueAndEqualFront.Count() == 0)
                            {
                                initInd.RemoveWhere(idx => Fronts[idx] == Fronts[compIndex]);
                                initInd.Add(compIndex);
                            }

                            isMoveNext = compListEnum.MoveNext();
                        }
                        else
                        {
                            break;
                        }
                    }

                    listIndicesWithLessOrEqValue.Clear();

                    var indicesWithLessOrEqValue = (from index in initInd
                                                    let locResComp = ConverterResCmp.ConvertToResCmp(_objCmp(SeqUniqObjs[index][1], SeqUniqObjs[assignIndex][1]))
                                                    where locResComp == ResComp.EQ || locResComp == ResComp.LE
                                                    select index);

                    foreach (int index in indicesWithLessOrEqValue)
                    {
                        listIndicesWithLessOrEqValue.AddLast(index);
                    }

                    if (listIndicesWithLessOrEqValue.Count != 0)
                    {
                        maxFront = listIndicesWithLessOrEqValue.Max(idx => Fronts[idx]);
                        Fronts[assignIndex] = Math.Max(Fronts[assignIndex], maxFront + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Class for the lexicographical compare. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class LexicographicCmp<T> : IComparer<IReadOnlyList<T>>
        {
            private Comparison<T> _cmp;

            public LexicographicCmp(Comparison<T> Cmp)
            {
                _cmp = Cmp;
            }

            /// <summary>
            /// <para>
            /// Compare <paramref name="LeftSeq"/> and <paramref name="RightSeq"/> in the
            /// lexicographical order.
            /// </para>
            /// <para>
            /// <paramref name="LeftSeq"/> is lexicographically less than
            /// <paramref name="RightSeq"/>, if and only if <paramref name="LeftSeq"/>[i] &lt;
            /// <paramref name="RightSeq"/>[i], for first i in {0,1,...,len(
            /// <paramref name="LeftSeq"/>)-1}, where <paramref name="LeftSeq"/>[i] != <paramref name="RightSeq"/>[i].
            /// </para>
            /// <para>
            /// <paramref name="LeftSeq"/> is equal to <paramref name="RightSeq"/>, if and only if
            /// <paramref name="LeftSeq"/>[i] == <paramref name="RightSeq"/>[i], for all i in
            /// {0,1,...,len( <paramref name="LeftSeq"/>)-1}.
            /// </para>
            /// <para>
            /// In other cases, <paramref name="LeftSeq"/> is lexicographically greater than <paramref name="RightSeq"/>.
            /// </para>
            /// </summary>
            /// <remarks>
            /// <para>
            /// <paramref name="LeftSeq"/> and <paramref name="RightSeq"/> must have the same lengths.
            /// </para>
            /// </remarks>
            /// <param name="LeftSeq"></param>
            /// <param name="RightSeq"></param>
            /// <returns>
            /// -1, if <paramref name="LeftSeq"/> is lexicographically less than <paramref name="RightSeq"/>.
            /// 0, if <paramref name="LeftSeq"/> is equal to <paramref name="RightSeq"/>,. 1, otherwise.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// If <paramref name="LeftSeq"/> or <paramref name="RightSeq"/> is null.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// <para>
            /// If <paramref name="LeftSeq"/> and <paramref name="RightSeq"/> have no the same length.
            /// </para>
            /// <para> If <paramref name="LeftSeq"/> and <paramref name="RightSeq"/> are empty. </para>
            /// </exception>
            public int Compare(IReadOnlyList<T> LeftSeq, IReadOnlyList<T> RightSeq)
            {
                if (LeftSeq == null)
                {
                    throw new ArgumentNullException(nameof(LeftSeq));
                }

                if (RightSeq == null)
                {
                    throw new ArgumentNullException(nameof(RightSeq));
                }

                if (LeftSeq.Count != RightSeq.Count)
                {
                    throw new ArgumentException($"{nameof(LeftSeq)} and {nameof(RightSeq)} must have the same length.");
                }
                else if (LeftSeq.Count == 0)
                {
                    throw new ArgumentException($"{nameof(LeftSeq)} and {nameof(RightSeq)} are empty.");
                }

                int res = 0;

                ResComp resComp;

                for (int i = 0; i < LeftSeq.Count; i++)
                {
                    resComp = ConverterResCmp.ConvertToResCmp(_cmp(LeftSeq[i], RightSeq[i]));

                    if (resComp == ResComp.LE)
                    {
                        res = -1;
                        break;
                    }
                    else if (resComp == ResComp.GR)
                    {
                        res = 1;
                        break;
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Create instance of the non-dominated sorting. 
        /// </summary>
        /// <param name="Cmp"> Defines totally order between any values in the objectives. </param>
        /// <exception cref="ArgumentNullException"> If <paramref name="Cmp"/> is null. </exception>
        public Ndsort(Comparison<TObj> Cmp)
        {
            if (Cmp == null)
            {
                throw new ArgumentNullException(nameof(Cmp));
            }

            _lexCmp = new LexicographicCmp<TObj>(Cmp);
            _objCmp = Cmp;
        }

        /// <summary>
        /// Non-dominated sorting. 
        /// </summary>
        /// <param name="SeqObjs"> The sequence of the values of the objectives. </param>
        /// <returns> The indices of the fronts. </returns>
        /// <exception cref="ArgumentNullException">
        /// <para> If <paramref name="SeqObjs"/> is null. </para>
        /// <para> If <paramref name="SeqObjs"/> contains null. </para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para> If <paramref name="SeqObjs"/> is empty. </para>
        /// <para> If the number of the objectives is less than 2. </para>
        /// <para>
        /// If the elements in the <paramref name="SeqObjs"/> have not an equal number of the objective.
        /// </para>
        /// </exception>
        public int[] NonDominSort(IEnumerable<IReadOnlyList<TObj>> SeqObjs)
        {
            return NonDominSortObj(SeqObjs);
        }

        /// <summary>
        /// Non-dominated sorting. 
        /// </summary>
        /// <typeparam name="TDecision"> A type of the decision. </typeparam>
        /// <param name="SeqDecisions"> The sequence of the decisions. </param>
        /// <param name="GetObjs">
        /// The function which maps a decision space into a objectives space.
        /// </param>
        /// <returns> The indices of the fronts. </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="SeqDecisions"/> or <paramref name="GetObjs"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException"> See correspond exception in the <see cref="Ndsort{TObj}.NonDominSort(IEnumerable{IReadOnlyList{TObj}})"/>. </exception>
        public int[] NonDominSort<TDecision>(IEnumerable<TDecision> SeqDecisions, Func<TDecision, IReadOnlyList<TObj>> GetObjs)
        {
            if (GetObjs == null)
            {
                throw new ArgumentNullException(nameof(GetObjs));
            }
            if (SeqDecisions == null)
            {
                throw new ArgumentNullException(nameof(SeqDecisions));
            }

            return NonDominSortObj(SeqDecisions.Select(item => GetObjs(item)));
        }
    }
}