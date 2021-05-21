using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Console.WriteLine("123");
            Database.Processing.Get("Пермь", "Test", out bool cityIsUsed, out string outCity, out int letterNumberFromEnd,
                out string wikiUrl, out string yandexUrl, out string googleUrl, out string mapUrl,
                out (double latitude, double longitude) coordinateCity, out string photoUrl);
        }
    }
}