using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LumenWorks.Framework.IO.Csv;

namespace Database
{
    public static class Processing
    {
        private static Dictionary<char, List<City>> databaseFind;
        private static string[] databaseCities;
        private static Dictionary<string, User> databaseUsers;

        private class City
        {
            public string Name;
            public int Population;

            public static implicit operator string(City c) => c.Name;
        }

        private class User
        {
            public string Id
            {
                get
                {
                    return _id;
                }
            }

            private string _id;

            public DateTime LastCall
            {
                get
                {
                    return _lastCall;
                }
            }

            private DateTime _lastCall;

            public Dictionary<char, List<City>> UsedCities
            {
                get
                {
                    return UsedCities;
                }

                set
                {
                    UsedCities = value;
                    _lastCall = DateTime.Now;
                }
            }

            public User(string id)
            {
                _id = id;
                UsedCities = new Dictionary<char, List<City>>();
                _lastCall = DateTime.Now;
            }
        }

        private class CityEqualityComparerClass : IEqualityComparer<City>
        {
            public bool Equals(City x, City y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode([DisallowNull] City obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private static CityEqualityComparerClass CityEqualityComparer = new CityEqualityComparerClass();

        public static void ReadCsv()
        {
            // инициализация коллекций
            databaseFind = new Dictionary<char, List<City>>();
            databaseUsers = new Dictionary<string, User>();

            // open the file "data.csv" which is a CSV file with headers
            using (CsvReader csv =
            new CsvReader(new StreamReader("Database.csv"), true))
            {
                List<string> cities = new List<string>();
                //int fieldCount = csv.FieldCount;
                //string[] headers = csv.GetFieldHeaders();
                while (csv.ReadNextRecord())
                {
                    if (!databaseFind.ContainsKey(csv[1][0]))
                    {
                        databaseFind.Add(csv[1][0], new List<City>());
                    }
                    databaseFind[csv[1][0]].Add(new City { Name = csv[2], Population = int.Parse(csv[3]) });
                    cities.Add(csv[2]);
                }
                databaseCities = cities.ToArray();
                for (int i = 0; i < databaseCities.Length; i++)
                {
                    databaseCities[i] = databaseCities[i].ToLower();
                }
            }
        }

        /// <param name="city"> Город отправленный пользователем </param>
        /// <param name="id"> ID пользователя (в начале советую писать из какого он мессенджера) </param>
        /// <param name="cityIsUsed"> Был ли использован этот город пользователем ранее </param>
        /// <param name="letterNumberFromEnd"> номер буквы с конца на которую бот возвращает значение (0 = ответ на последнюю букву)</param>
        /// <param name="wikiUrl"> Ссылка на википедию города</param>
        /// <param name="yandexUrl"> Ссылка на запрос в яндексе по городу</param>
        /// <param name="googleUrl"> Ссылка на запрос в гугле по городу</param>
        /// <param name="mapUrl"> Ссылка на место на карте</param>
        /// <param name="coordinateCity"> Координаты города (latitude - широта, longitude - долгота)</param>
        /// <param name="photoUrl"> Ссылка на фото из города</param>
        /// <param name="outCity"> null значит города не существует в бд, '' - боту нечего отвечать </param>
        public static void Get(string city, string id, out bool cityIsUsed, out string outCity, out int letterNumberFromEnd, out string wikiUrl,
            out string yandexUrl, out string googleUrl, out string mapUrl, out (double latitude, double longitude) coordinateCity, out string photoUrl)
        {
            // изначальные значения
            outCity = null;
            letterNumberFromEnd = 0;
            cityIsUsed = false;
            wikiUrl = null;
            yandexUrl = null;
            googleUrl = null;
            mapUrl = null;
            photoUrl = null;
            coordinateCity = default;

            city = city.Split()[0].ToLower();

            if (databaseCities.Contains(city))
            {
                if (!databaseUsers.ContainsKey(id))
                    databaseUsers.Add(id, new User(id));
                if (databaseUsers[id].UsedCities[city[0]].Contains(new City() { Name = city }, CityEqualityComparer))
                {
                    outCity = "";
                    cityIsUsed = true;
                }
                else
                {
                    foreach (char c in city)
                    {
                        List<City> except = databaseFind[c].Except(databaseUsers[id].UsedCities[c], CityEqualityComparer).ToList();
                        if (except.Count != 0)
                        {
                            outCity = except[0];
                            databaseUsers[id].UsedCities[c].Add(new City() { Name = city });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// get с поиском по окружности в километрах
        /// </summary>
        /// <param name="city"> Город отправленный пользователем </param>
        /// <param name="id"> ID пользователя (в начале советую писать из какого он мессенджера) </param>
        /// <param name="cityIsUsed"> Был ли использован этот город пользователем ранее </param>
        /// <param name="letterNumberFromEnd"> номер буквы с конца на которую бот возвращает значение (0 = ответ на последнюю букву)</param>
        /// <param name="wikiUrl"> Ссылка на википедию города</param>
        /// <param name="yandexUrl"> Ссылка на запрос в яндексе по городу</param>
        /// <param name="googleUrl"> Ссылка на запрос в гугле по городу</param>
        /// <param name="mapUrl"> Ссылка на место на карте</param>
        /// <param name="coordinateCity"> Координаты города (latitude - широта, longitude - долгота)</param>
        /// <param name="photoUrl"> Ссылка на фото из города</param>
        /// <param name="outCity"> null значит города не существует в бд, '' - все города отгаданы</param>
        /// <param name="coordinateUser"> latitude - широта, longitude - долгота</param>
        /// <param name="searchRadius"> в километрах</param>
        public static void Get(string city, string id, (double latitude, double longitude) coordinateUser, double searchRadius,
            out bool cityIsUsed, out string outCity, out int letterNumberFromEnd, out string wikiUrl, out string yandexUrl, out string googleUrl,
            out string mapUrl, out (double latitude, double longitude) coordinateCity, out string photoUrl)
        {
            outCity = null;
            letterNumberFromEnd = 0;
            cityIsUsed = false;
            wikiUrl = null;
            yandexUrl = null;
            googleUrl = null;
            mapUrl = null;
            photoUrl = null;
            coordinateCity = default;
        }

        public static string DatabaseFindToString()
        {
            string s = "";
            foreach (KeyValuePair<char, List<City>> keyValuePair in databaseFind)
            {
                bool first = true;
                s += keyValuePair.Key + ": ";
                foreach (City city in keyValuePair.Value)
                {
                    if (first)
                    {
                        s += city.Name + " " + city.Population + "\n";
                        first = false;
                    }
                    else
                    {
                        s += "   " + city.Name + " " + city.Population + "\n";
                    }
                }
            }
            return s;
        }

        public static string DatabaseCitiesToString()
        {
            string s = "";
            foreach (string city in databaseCities)
            {
                s += city + ", ";
            }
            return s;
        }
    }
}