using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace Database
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Processing.ReadCsv();
            string s = Console.ReadLine();

            while (s.Split()[0] != "exit")
            {
                Processing.Get(s, "Test", out bool onLastLetter, out bool cityIsUsed, out string outCity, out char nextLetter, out int letterNumberFromEnd,
                    out string wikiUrl, out string yandexUrl, out string googleUrl, out string mapUrl,
                    out (double latitude, double longitude) coordinateCity, out string photoUrl);
                Console.WriteLine(
                    $"onLastLetter {onLastLetter}, cityIsUsed {cityIsUsed}, outCity {outCity}, nextLetter {nextLetter}, letterNumberFromEnd {letterNumberFromEnd}, wikiUrl {wikiUrl}, " +
                    $"yandexUrl {yandexUrl}, googleUrl {googleUrl}, mapUrl {mapUrl}, coordinateCity.latitude {coordinateCity.latitude}, " +
                    $"coordinateCity.longitude {coordinateCity.longitude}, photoUrl {photoUrl}");
                s = Console.ReadLine();
            }
            Console.ReadKey();
        }
    }
}