using System;
using System.Threading;

namespace curse_work_2021
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Database.ReadCsv();
            Console.WriteLine(Database.ToString());
        }
    }
}