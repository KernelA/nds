namespace NdomsortTests.NdsortTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Linq.Expressions;

    using Nds;
    using Nds.Tools;

    using NdomsortTests;
    using Xunit;

    public class NdsortTests
    {
        private const int SEED = 4;

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

        /// <summary>
        /// <paramref name="Number"/> to the <paramref name="Exp"/>th power.
        /// </summary>
        /// <param name="Number">Must be greater than 0.</param>
        /// <param name="Exp">Must be greater than 0.</param>
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
        void NdsortManyFrontsTest()
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

                RandomPermutation(seq,rand);

                int[] resFronts = Ndsort.NonDominSort(seq);

                Array.Sort(resFronts);

                Assert.True(exactFronts.SequenceEqual(resFronts));
            }    
        }

        [Fact]
        void NdsortOneSeqTest()
        {
            int[][] seq = new int[1][] { new int[] { 1, 2, 3, 0 } };

            int[] resFronts = Ndsort.NonDominSort(seq);

            Assert.True(resFronts.Length == 1 && resFronts[0] == 0);
        }

        [Fact]
        void NdsortOneFrontTest()
        {
            double[][] seq = new double[3][]
            {
                new double[] {0, 0, 0},
                new double[] {0, 0, 0},
                new double[] {0, 0, 0}
            };

            int[] resFronts = Ndsort.NonDominSort(seq);

            Assert.True(resFronts.SequenceEqual(Enumerable.Repeat(0, seq.Length)));
        }

        [Fact]
        void NdsortTwoFrontTest()
        {
            List<DoubleClass[]> seq = new List<DoubleClass[]>()
            {
                new DoubleClass[3] {new DoubleClass(1), new DoubleClass(0), new DoubleClass(1)},
                new DoubleClass[3] {new DoubleClass(0), new DoubleClass(1), new DoubleClass(1)},
                new DoubleClass[3] {new DoubleClass(-2.2), new DoubleClass(-4), new DoubleClass(0)}
            };


            int[] resFronts = Ndsort.NonDominSort(seq);

            int[] exactFronts = { 1, 1, 0 };

            Assert.True(resFronts.SequenceEqual(exactFronts));
        }

        [Fact]
        void NdsortSeqFromRangeTest()
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

                // Generate an all possible combinations numbers from the range 'numbers'.
                // For example, if 'numbers' is  [-1,0,1], than combinations are:
                // {-1, -1, -1}
                // {-1, -1, 0}
                // {-1, -1, 1}
                // {-1, 0, -1}
                // ...
                // {1, 1, 0}
                // {1, 1, 1}
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

                int[] resFronts = Ndsort.NonDominSort(seq);

                int totalFronts = resFronts.Max() + 1;

                var intersection = resFronts.Intersect(Enumerable.Range(0, totalFronts));

                Assert.True(intersection.Count() == totalFronts);

                // Transform to the dictionary.
                var keyValues = from item in resFronts.Zip(seq, (front, oneSeq) => new KeyValuePair<int, int[]>(front, oneSeq))
                            select item;

                Dictionary<int, LinkedList<int[]>> dict = new Dictionary<int, LinkedList<int[]>>(resFronts.Max() + 1);

                // Create dictionary.
                // Keys are indices of the fronts.
                // Values are the sequences correspond to the index of the front.
                foreach (var keyValue in keyValues)
                {
                    if(dict.ContainsKey(keyValue.Key))
                    {
                        dict[keyValue.Key].AddLast(keyValue.Value);
                    }
                    else
                    {
                        var value = new LinkedList<int[]>();
                        value.AddLast(keyValue.Value);
                        dict.Add(keyValue.Key, value);
                    }
                }

                foreach (int frontIndex in Enumerable.Range(0, dict.Keys.Count - 1))
                {
                    foreach (int[] seqCurrFront in dict[frontIndex])
                    {
                        foreach (int[] seqCurrFront2 in dict[frontIndex])
                        {
                            if(!seqCurrFront.SequenceEqual(seqCurrFront2))
                            {
                                Assert.False(Stools.IsDominate(seqCurrFront, seqCurrFront2));
                                Assert.False(Stools.IsDominate(seqCurrFront2, seqCurrFront));
                            }
                        }

                        foreach (int[] seqNextFront in dict[frontIndex + 1])
                        {
                            Assert.False(Stools.IsDominate(seqNextFront, seqCurrFront));
                        }
                    }
                }
            }
        }
    }
}
