namespace NdomsortTests.NdsortTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NdomsortTests;

    using Nds.Tools;

    using Xunit;

    public class NdsortTests
    {
        private const int SEED = 4;

        private void CheckFronts<T>(T[][] Seq, int[] Fronts) where T : IComparable<T>
        {
            int totalFronts = Fronts.Max() + 1;

            var intersection = Fronts.Intersect(Enumerable.Range(0, totalFronts));

            Assert.True(intersection.Count() == totalFronts);

            // Transform to the dictionary.
            var keyValues = from item in Fronts.Zip(Seq, (front, oneSeq) => new KeyValuePair<int, T[]>(front, oneSeq))
                            select item;

            Dictionary<int, LinkedList<T[]>> dict = new Dictionary<int, LinkedList<T[]>>(Fronts.Max() + 1);

            // Create dictionary. Keys are indices of the fronts. Values are the sequences correspond
            // to the index of the front.
            foreach (var keyValue in keyValues)
            {
                if (dict.ContainsKey(keyValue.Key))
                {
                    dict[keyValue.Key].AddLast(keyValue.Value);
                }
                else
                {
                    var value = new LinkedList<T[]>();
                    value.AddLast(keyValue.Value);
                    dict.Add(keyValue.Key, value);
                }
            }

            foreach (int frontIndex in Enumerable.Range(0, dict.Keys.Count - 1))
            {
                foreach (T[] seqCurrFront in dict[frontIndex])
                {
                    foreach (T[] seqCurrFront2 in dict[frontIndex])
                    {

                            Assert.False(Stools.IsDominate(seqCurrFront, seqCurrFront2, Cmp.CmpByIComparable));
                            Assert.False(Stools.IsDominate(seqCurrFront2, seqCurrFront, Cmp.CmpByIComparable));
                    }

                    foreach (T[] seqNextFront in dict[frontIndex + 1])
                    {
                        Assert.False(Stools.IsDominate(seqNextFront, seqCurrFront, Cmp.CmpByIComparable));
                    }
                }
            }
        }

        /// <summary>
        /// <paramref name="Number"/> to the <paramref name="Exp"/> th power. 
        /// </summary>
        /// <param name="Number"> Must be greater than 0. </param>
        /// <param name="Exp"> Must be greater than 0. </param>
        private int IntPow(int Number, int Exp)
        {
            int res = 1;

            for (int i = 0; i < Exp; i++)
            {
                checked
                {
                    res *= Number;
                }
            }

            return res;
        }

        [Fact]
        private void NdsortGetObjsTest()
        {
            List<DoubleClass[]> seq = new List<DoubleClass[]>()
            {
                new DoubleClass[3] {new DoubleClass(1), new DoubleClass(0), new DoubleClass(1)},
                new DoubleClass[3] {new DoubleClass(0), new DoubleClass(1), new DoubleClass(1)},
                new DoubleClass[3] {new DoubleClass(-2.2), new DoubleClass(-4), new DoubleClass(0)}
            };

            int[] resFronts = new Nds.Ndsort<double>(Cmp.CmpByIComparable).NonDominSort(seq, objs => objs.Select(obj => obj.Value).ToList());

            int[] exactFronts = { 1, 1, 0 };

            Assert.True(resFronts.SequenceEqual(exactFronts));
        }

        [Fact]
        private void NdsortManyFrontsTest()
        {
            int seqCount = 20, maxLenObjs = 4;

            int[][] seq = new int[seqCount][];

            int[] exactFronts = Enumerable.Range(0, seqCount).ToArray();

            Random rand = new Random(SEED);

            for (int lenObjs = 2; lenObjs <= maxLenObjs; lenObjs++)
            {
                for (int i = 0; i < seqCount; i++)
                {
                    seq[i] = Enumerable.Repeat(i, lenObjs).ToArray();
                }

                RandomPermutation(seq, rand);

                int[] resFronts = new Nds.Ndsort<int>(Cmp.CmpByIComparable).NonDominSort(seq);

                Array.Sort(resFronts);

                Assert.True(exactFronts.SequenceEqual(resFronts));
            }
        }

        [Fact]
        private void NdsortOneFrontTest()
        {
            double[][] seq = new double[3][]
            {
                new double[] {0, 0, 0},
                new double[] {0, 0, 0},
                new double[] {0, 0, 0}
            };

            int[] resFronts = new Nds.Ndsort<double>(Cmp.CmpByIComparable).NonDominSort(seq);

            Assert.True(resFronts.SequenceEqual(Enumerable.Repeat(0, seq.Length)));
        }

        [Fact]
        private void NdsortOneSeqTest()
        {
            int[][] seq = new int[1][] { new int[] { 1, 2, 3, 0 } };

            int[] resFronts = new Nds.Ndsort<int>(Cmp.CmpByIComparable).NonDominSort(seq);

            Assert.True(resFronts.Length == 1 && resFronts[0] == 0);
        }

        [Fact]
        private void NdsortSeqFromRangeTest()
        {
            Random rand = new Random(SEED);

            for (int dim = 2; dim <= 5; dim++)
            {
                int[] numbers = Enumerable.Range(-1, 3).ToArray();

                int totalSeq = IntPow(numbers.Length, dim);

                int[][] seq = new int[totalSeq][];

                for (int i = 0; i < totalSeq; i++)
                {
                    seq[i] = new int[dim];
                }

                // Generate an all possible combinations numbers from the range 'numbers'. For
                // example, if 'numbers' is [-1,0,1], than combinations are: {-1, -1, -1} {-1, -1, 0}
                // {-1, -1, 1} {-1, 0, -1} ... {1, 1, 0} {1, 1, 1}
                int num = 0;
                for (int j = 0; j < dim; j++)
                {
                    int maxCount = IntPow(numbers.Length, j + 1);
                    for (int count = 0; count < maxCount; count++)
                    {
                        num = numbers[count % numbers.Length];

                        int maxRepeat = IntPow(numbers.Length, dim - j - 1);

                        for (int repeat = 0; repeat < maxRepeat; repeat++)
                        {
                            seq[maxRepeat * count + repeat][j] = num;
                        }
                    }
                }

                RandomPermutation(seq, rand);

                int[] resFronts = new Nds.Ndsort<int>(Cmp.CmpByIComparable).NonDominSort(seq);

                CheckFronts(seq, resFronts);
            }
        }

        [Fact]
        private void NdsortTwoFront2Test()
        {
            int[][] seq = new int[3][];

            int[] exactFronts = { 0, 1, 1 };

            seq[0] = new int[4] { 1, 3, 0, 1};
            seq[1] = new int[4] { 1, 4, 5, 1};
            seq[2] = new int[4] { 3, 4, 3, 1};

            var nds = new Nds.Ndsort<int>(Cmp.CmpByIComparable);

            int[] fronts = nds.NonDominSort(seq);

            Assert.True(fronts.SequenceEqual(exactFronts));

        }

        [Fact]
        private void NdsortSortRandomSeqTest()
        {
            int[][] seq = new int[100][];

            Nds.Ndsort<int> nds = new Nds.Ndsort<int>(Cmp.CmpByIComparable);

            Random rand = new Random(SEED);

            for (int dim = 2; dim < 6; dim++)
            {
                for (int i = 0; i < seq.Length; i++)
                {
                    seq[i] = new int[dim];

                    for (int j = 0; j < seq[i].Length; j++)
                    {
                        seq[i][j] = rand.Next(-500, 501);
                    }
                }

                int[] fronts = nds.NonDominSort(seq);

                CheckFronts(seq, fronts);
            }
        }

        [Fact]
        private void NdsortTwoFrontTest()
        {
            List<DoubleClass[]> seq = new List<DoubleClass[]>()
            {
                new DoubleClass[3] {new DoubleClass(1), new DoubleClass(0), new DoubleClass(1)},
                new DoubleClass[3] {new DoubleClass(0), new DoubleClass(1), new DoubleClass(1)},
                new DoubleClass[3] {new DoubleClass(-2.2), new DoubleClass(-4), new DoubleClass(0)}
            };

            int[] resFronts = new Nds.Ndsort<DoubleClass>(Cmp.CmpByIComparable).NonDominSort(seq);

            int[] exactFronts = { 1, 1, 0 };

            Assert.True(resFronts.SequenceEqual(exactFronts));
        }

        /// <summary>
        /// A simple random permutation with uniform distribution. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Seq"></param>
        /// <param name="RandGen"></param>
        private void RandomPermutation<T>(IList<T> Seq, System.Random RandGen)
        {
            int j = 0;

            T temp;

            for (int i = 0; i < Seq.Count; i++)
            {
                j = RandGen.Next(i + 1);
                temp = Seq[i];
                Seq[i] = Seq[j];
                Seq[j] = temp;
            }
        }
    }
}