namespace NdomsortTests.StoolsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;
    using Nds.Tools;
    using NdsortTests;

    public class StoolsTests
    {
        private const int MAX_SIZE = 30;

        private const int SEED = 4;

        private Array CreateAndInitArrayForTest(int Size, Type ElemType)
        {
            if(ElemType != typeof(int) && ElemType != typeof(DoubleClass))
            {
                throw new ArgumentException($"{nameof(ElemType)} must be type of {typeof(int).Name} or {typeof(DoubleClass).Name}");
            }

            Array array = Array.CreateInstance(ElemType, Size);

            Random rand = new Random(4);

            if (ElemType == typeof(int))
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array.SetValue(rand.Next(-100, 101), i);
                }
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array.SetValue(new DoubleClass(rand.NextDouble() - 0.5), i);
                }
            }

            return array;
        }

        [Fact]
        public void IsDominateTest()
        {

            List<int[]> pairs = new List<int[]>
            {
                new int[2] {0, 0},
                new int[2] {1, 1},
                new int[2] {1, 0},
                new int[2] {1, 1},
                new int[2] {1, 1},
                new int[2] {1, 1},
                new int[2] {2, 1},
                new int[2] {1, 1},
                new int[2] {2, 2},
                new int[2] {1, 1},
                new int[3] {0, 1, 0},
                new int[3] {1, 0, 0}
            };

            bool[] answers = { true, true, false, false, false, false };

            int j = 0;

            for (int i = 0; i < pairs.Count - 1; i += 2)
            {
                Assert.True(Stools.IsDominate(pairs[i], pairs[i + 1]) == answers[j]);
                j++;
            }
        }

        [Fact]
        public void IsDominateDiffLengthsTest()
        {
            int[] seq1 = new int[2] { 3, 4 };
            int[] seq2 = new int[3] { 1, 2, 3 };

            Assert.Throws<ArgumentException>(() => Stools.IsDominate(seq1, seq2));
        }

        [Fact]
        public void IsDominateZeroLengthTest()
        {
            int[] seq1 = new int[0];
            int[] seq2 = new int[0];

            Assert.False(Stools.IsDominate(seq1, seq2));
        }

        [Fact]
        public void IsDominateNullPass1Test()
        {
            int[] seq = new int[2] { 1, 2};

            Assert.Throws<ArgumentNullException>(() => Stools.IsDominate(null, seq));
        }

        [Fact]
        public void IsDominateNullPass2Test()
        {
            int[] seq = new int[2] { 1, 2 };

            Assert.Throws<ArgumentNullException>(() => Stools.IsDominate(seq, null));
        }

        [Fact]
        public void FindLowMedianStructTest()
        {
            for (int i = 1; i <= MAX_SIZE; i++)
            {
                    int[] seq = CreateAndInitArrayForTest(i, typeof(int)).Cast<int>().ToArray();

                    int median = Stools.FindLowMedian(seq);

                    Array.Sort(seq);
                    Assert.Equal<int>(median, seq[(seq.Length - 1) / 2]);
            }
        }

        [Fact]
        public void FindLowMedianClassTest()
        {
            for (int i = 1; i <= MAX_SIZE; i++)
            {

                    DoubleClass[] seq = CreateAndInitArrayForTest(i, typeof(DoubleClass)).Cast<DoubleClass>().ToArray();

                    DoubleClass median = Stools.FindLowMedian(seq);

                    Array.Sort(seq);
                    Assert.Equal<DoubleClass>(median, seq[(seq.Length - 1) / 2]);

            }
        }

        [Fact]
        public void FindLowMedianZeroLengthTest()
        {
            Assert.Throws<ArgumentException>(() => Stools.FindLowMedian(new int[0] { }));
        }

        [Fact]
        public void FindLowMedianNullPassTest()
        {
            int[] seq = null;

            Assert.Throws<ArgumentNullException>(() => Stools.FindLowMedian(seq));
        }
    }
}
