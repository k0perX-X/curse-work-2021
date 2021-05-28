using System;
using System.Diagnostics;
using Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Processing.ReadCsv();
            // Debug.Print(Processing.DatabaseCitiesToString());
            Processing.Get("Пермь", "Test", out bool cityIsUsed, out string outCity, out int letterNumberFromEnd,
                out string wikiUrl, out string yandexUrl, out string googleUrl, out string mapUrl,
                out (double latitude, double longitude) coordinateCity, out string photoUrl);
            Debug.Print($"cityIsUsed {cityIsUsed} outCity {outCity} letterNumberFromEnd {letterNumberFromEnd} wikiUrl {wikiUrl} " +
                        $"yandexUrl {yandexUrl} googleUrl {googleUrl} mapUrl {mapUrl} coordinateCity.latitude {coordinateCity.latitude} " +
                        $"coordinateCity.longitude {coordinateCity.longitude} photoUrl {photoUrl}");
        }
    }
}