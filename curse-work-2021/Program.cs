using System;
using System.Threading;

namespace Database
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Processing.ReadCsv();
            Console.WriteLine(Processing.DatabaseFindToString());
            Console.WriteLine(Processing.DatabaseCitiesToString());
            Console.ReadKey();
        }
    }
}