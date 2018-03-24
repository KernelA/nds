namespace Example
{
    using System;
    using System.Linq;

    using Nds;

    internal class Program
    {
        private static int CmpInt(int X, int Y)
        {
            if (X < Y)
            {
                return -1;
            }
            else if (X > Y)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static void Main(string[] args)
        {
            int[][] seq1 = new int[10][];

            Random rand = new Random(6);

            Ndsort<int> nds = new Ndsort<int>(CmpInt);

            for (int i = 0; i < seq1.Length; i++)
            {
                seq1[i] = new int[4];

                for (int j = 0; j < seq1[i].Length; j++)
                {
                    seq1[i][j] = rand.Next(-10, 11);
                }
            }

            int[] fronts1 = nds.NonDominSort(seq1);

            var seq2 = from item in seq1
                       select new { fitness = item };

            int[] fronts2 = nds.NonDominSort(seq2, item => item.fitness);

            Console.WriteLine($"The fronts are equal: {fronts1.SequenceEqual(fronts2)}.");

            var groupedFronts = fronts1.Zip(seq1, (front, seq) => new ValueTuple<int, int[]>(front, seq)).GroupBy(tuple => tuple.Item1, tuple => tuple.Item2);

            foreach (var front in groupedFronts)
            {
                Console.WriteLine($"The front index is {front.Key}.");

                foreach (var seq in front)
                {
                    Console.WriteLine($"\t({seq.Select(num => num.ToString()).Aggregate((num1, num2) => num1 + ", " + num2)})");
                }
            }

            Console.ReadKey();
        }
    }
}