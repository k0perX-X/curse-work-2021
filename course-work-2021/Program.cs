using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Database;
using TelegramBot;

namespace course_work_2021
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Processing.ReadCsv();
            TelegramBot.Program.Main();
        }
    }
}