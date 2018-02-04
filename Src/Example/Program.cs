namespace Example
{
    using System;
    using System.Linq;
    using Nds;

    class Program
    {
        static void Main(string[] args)
        {
            int[][] seq1 = new int[10][];

            Random rand = new Random(6);

            for (int i = 0; i < seq1.Length; i++)
            {
                seq1[i] = new int[4];

                for (int j = 0; j < seq1[i].Length; j++)
                {
                    seq1[i][j] = rand.Next(-10, 11);
                }
            }

            int[] fronts1 = Ndsort.NonDominSort(seq1);


            var seq2 = from item in seq1
                       select new { fitness = item };

            int[] fronts2 = Ndsort.NonDominSort(seq2, item => item.fitness);

            Console.WriteLine(fronts1.SequenceEqual(fronts2));
            Console.ReadKey();
        }
    }
}
