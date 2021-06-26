using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using LumenWorks.Framework.IO.Csv;

namespace Database
{
    public static class Processing
    {
        private static Dictionary<char, List<City>> _databaseFind;
        private static string[] _databaseCities;
        private static Dictionary<string, User> _databaseUsers;
        private static readonly Random Random = new Random();
        private static Timer _userTimer;

#if DEBUG
        private static Logging Logging = new Logging(Logging.LevelLogging.DEBUG, "Database.log", true);
#else
        private static Logging Logging = new Logging(Logging.LevelLogging.INFO, "Database.log", true);
#endif

        public delegate void TelegramUserTimerMinuteDelegate(string ID, User user, DateTime currentTime);

        public static event TelegramUserTimerMinuteDelegate TelegramUserTimerMinute;

        public static Dictionary<string, User> DatabaseUsers => _databaseUsers;

        public static Dictionary<char, List<City>> DatabaseFind => _databaseFind; //Внешний доступ к базе

        private static readonly string[] DoubleNameCities = new[]
        {
            "нижняя", "петров", "сухой", "набережные", "верхняя", "новая", "красное", "малая", "белая", "советская",
            "минеральные", "новый", "старая", "сергиев", "старый", "западная", "красный", "вятские", "верхний",
            "мариинский", "гаврилов", "большой", "полярные", "лодейное", "дагестанские", "горячий", "сосновый",
            "вышний", "нижние", "великие", "великий", "павловский", "нижний",
        };

        public class City
        {
            public string Name;
            public int Population;
            public string GoogleUrl;
            public string YandexUrl;
            public string WikiUrl;
            public string PicUrl;
            public string MapUrl;
            public decimal Latitude;
            public decimal Longitude;

            public static implicit operator string(City c) => c.Name;

            public override string ToString() => Name;
        }

        public class User
        {
            public string Id => _id;

            private readonly string _id;

            public DateTime LastCall => _lastCall;

            private DateTime _lastCall;

            private Dictionary<char, List<City>> _usedCities;

            public char NextLetter { get; set; }

            public string OutCity { get; set; }

            public byte LetterNumberFromEnd { get; set; }

            public byte RequestsToContinueGame { get; set; }

            public Dictionary<char, List<City>> UsedCities
            {
                get
                {
                    _lastCall = DateTime.Now;
                    RequestsToContinueGame = 0;
                    return _usedCities;
                }

                set
                {
                    _usedCities = value;
                    RequestsToContinueGame = 0;
                    _lastCall = DateTime.Now;
                }
            }

            public User(string id)
            {
                _id = id;
                _usedCities = new Dictionary<char, List<City>>();
                foreach (char c1 in Enumerable.Range('а', 'я' - 'а' + 1).Select(c => (char)c))
                {
                    _usedCities.Add(c1, new List<City>());
                }
                _lastCall = DateTime.Now;
            }

            public override string ToString()
            {
                return $"User: LastCall: {LastCall}, NextLetter: {NextLetter}, " +
                       $"OutCity: {OutCity}, LetterNumberFromEnd: {LetterNumberFromEnd}, " +
                       $"RequestsToContinueGame {RequestsToContinueGame}";
            }
        }

        public class CityEqualityComparerClass : IEqualityComparer<City>
        {
            public bool Equals(City x, City y)
            {
                try
                {
                    return String.Equals(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }

            public int GetHashCode([DisallowNull] City obj)
            {
                return obj.Name.ToLower().GetHashCode();
            }
        }

        public static readonly CityEqualityComparerClass CityEqualityComparer = new CityEqualityComparerClass();

        public static char NextLetterUser(string id)
        {
            User user = _databaseUsers[id];
            user.LetterNumberFromEnd += 1;
            user.NextLetter = user.OutCity[^(user.LetterNumberFromEnd + 1)];
            Logging.DEBUG(user.ToString());
            return user.NextLetter;
        }

        public static void ReadCsv()
        {
            // инициализация коллекций
            _databaseFind = new Dictionary<char, List<City>>();
            _databaseUsers = new Dictionary<string, User>();

            // open the file "data.csv" which is a CSV file with headers
            using (CsvReader csv =
            new CsvReader(new StreamReader("NewDatabase.csv"), true))
            {
                List<string> cities = new List<string>();
                //int fieldCount = csv.FieldCount;
                //string[] headers = csv.GetFieldHeaders();
                while (csv.ReadNextRecord())
                {
                    if (!_databaseFind.ContainsKey(csv[1][0]))
                    {
                        _databaseFind.Add(csv[1][0], new List<City>());
                    }
                    Debug.Print($"{csv[2]} {csv[3]} {csv[4]} {csv[5]} {csv[6]} {csv[7]} {csv[8]} {csv[9]} {csv[10]}");
                    _databaseFind[csv[1][0]].Add(new City
                    {
                        Name = csv[2],
                        Population = int.Parse(csv[3]),
                        GoogleUrl = csv[4],
                        YandexUrl = csv[5],
                        WikiUrl = csv[6],
                        PicUrl = csv[7].Replace("https", "http"),
                        MapUrl = csv[8],
                        Latitude = decimal.Parse(csv[9].Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator), CultureInfo.InvariantCulture),
                        Longitude = decimal.Parse(csv[10].Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator), CultureInfo.InvariantCulture)
                    });
                    cities.Add(csv[2]);
                }
                _databaseCities = cities.ToArray();
                for (int i = 0; i < _databaseCities.Length; i++)
                {
                    _databaseCities[i] = _databaseCities[i].ToLower();
                }
            }

            _userTimer = new Timer(On_userTimer, null, 0, 60000);
        }

        private static void On_userTimer(object state)
        {
            foreach (KeyValuePair<string, User> databaseUser in _databaseUsers)
            {
                var s = databaseUser.Key.Split(".");
                var currentTime = DateTime.Now;
                switch (s[0])
                {
                    case "Telegram":
                        TelegramUserTimerMinute?.Invoke(databaseUser.Key, databaseUser.Value, currentTime);
                        break;

                    default:
                        break;
                }
            }
        }

        public static void DeleteUser(string id)
        {
            _databaseUsers.Remove(id);
        }

        public static double NormalRandom(double mu = 0, double sigma = 1, double left = -1.7, double right = 1.3)
        {
            var u1 = 1.0 - Random.NextDouble();
            var u2 = 1.0 - Random.NextDouble();
            var rand = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            while (!(left <= rand && rand < right))
            {
                u1 = 1.0 - Random.NextDouble();
                u2 = 1.0 - Random.NextDouble();
                rand = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            }

            rand = mu + sigma * rand;
            return (rand - left) / Math.Abs(right - left);
        }

        /// <param name="city"> Город отправленный пользователем </param>
        /// <param name="id"> ID пользователя (в начале советую писать из какого он мессенджера) </param>
        /// <param name="cityIsUsed"> Был ли использован этот город пользователем ранее </param>
        /// <param name="nextLetter"> Буква на которую должен отвечать пользователь</param>
        /// <param name="letterNumberFromEnd"> номер буквы с конца на которую бот возвращает значение (0 = ответ на последнюю букву)</param>
        /// <param name="wikiUrl"> Ссылка на википедию города</param>
        /// <param name="yandexUrl"> Ссылка на запрос в яндексе по городу</param>
        /// <param name="googleUrl"> Ссылка на запрос в гугле по городу</param>
        /// <param name="mapUrl"> Ссылка на место на карте</param>
        /// <param name="coordinateCity"> Координаты города (latitude - широта, longitude - долгота)</param>
        /// <param name="photoUrl"> Ссылка на фото из города</param>
        /// <param name="outCity"> null значит города не существует в бд, "" - боту нечего отвечать </param>
        /// <param name="onLastLetter"> ответил ли пользователь на последнюю букву предыдущего слова </param>
        public static void Get(ref string city, string id, out bool onLastLetter, out bool cityIsUsed, out string outCity, out char nextLetter, out byte letterNumberFromEnd, out string wikiUrl,
            out string yandexUrl, out string googleUrl, out string mapUrl, out (decimal latitude, decimal longitude) coordinateCity, out string photoUrl)
        {
            // изначальные значения
            outCity = null;
            letterNumberFromEnd = 0;
            onLastLetter = true;
            cityIsUsed = false;
            wikiUrl = null;
            yandexUrl = null;
            googleUrl = null;
            mapUrl = null;
            photoUrl = null;
            nextLetter = default;
            coordinateCity.longitude = default;
            coordinateCity.latitude = default;

            string[] splittedCities = city.Split();
            if (splittedCities.Length > 1)
            {
                splittedCities[0] = splittedCities[0].ToLower();
                splittedCities[1] = splittedCities[1].ToLower();
                // если название города состоит из 2 слов
                if (DoubleNameCities.Contains(splittedCities[0].ToLower()))
                    city = splittedCities[0] + " " + splittedCities[1];
                else
                    city = splittedCities[0];
            }
            else
            {
                city = splittedCities[0];
            }

            city = city.ToLower();
            if (_databaseCities.Contains(city))
            {
                outCity = "";
                if (!_databaseUsers.ContainsKey(id))
                    _databaseUsers.Add(id, new User(id)); // Если человека нет в базе пользователей
                else
                {
                    if (city[0] != _databaseUsers[id].NextLetter) // проверка введено ли на правильную букву
                    {
                        onLastLetter = false;
                        return;
                    }
                }

                if (_databaseUsers[id].UsedCities[city[0]].Contains(new City() { Name = city }, CityEqualityComparer)) // проверка на использованность города
                {
                    cityIsUsed = true;
                }
                else
                {
                    foreach (char c in city.Reverse())
                    {
                        if (!_databaseFind.ContainsKey(c))
                        {
                            letterNumberFromEnd++;
                            continue;
                        }
                        List<City> except = _databaseFind[c]
                            .Except(_databaseUsers[id].UsedCities[c], CityEqualityComparer).ToList();
                        if (except.Count != 0)
                        {
                            int numberOfCity =
                                (int)Math.Round((NormalRandom() - 1d / (except.Count * 2)) * except.Count);
                            outCity = except[numberOfCity].Name; // используется смещенное нормальное распределение чтобы давать более редкие города чаще

                            _databaseUsers[id].UsedCities[city[0]].Add(new City() { Name = city });
                            _databaseUsers[id].UsedCities[c].Add(new City() { Name = outCity.ToLower() });

                            bool userWin = true; // проверка на победу + nextLetter
                            for (int i = outCity.Length - 1; i > 0; i--)
                            {
                                nextLetter = outCity[i];
                                if (_databaseFind.ContainsKey(nextLetter))
                                    if (_databaseFind[nextLetter].Except(_databaseUsers[id].UsedCities[nextLetter], CityEqualityComparer).Any())
                                    {
                                        _databaseUsers[id].NextLetter = nextLetter;
                                        _databaseUsers[id].OutCity = outCity;
                                        _databaseUsers[id].LetterNumberFromEnd = letterNumberFromEnd;
                                        userWin = false;
                                        break;
                                    }
                            }

                            if (userWin)
                            {
                                outCity = "";
                            }
                            else
                            {
                                City outCityClass = except[numberOfCity];
                                wikiUrl = outCityClass.WikiUrl;
                                yandexUrl = outCityClass.YandexUrl;
                                googleUrl = outCityClass.GoogleUrl;
                                mapUrl = outCityClass.MapUrl;
                                photoUrl = outCityClass.PicUrl;
                                coordinateCity.longitude = outCityClass.Longitude;
                                coordinateCity.latitude = outCityClass.Latitude;
                            }
                            break;
                        }
                        else
                        {
                            letterNumberFromEnd++;
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
        /// <param name="onLastLetter"> ответил ли пользователь на последнюю букву предыдущего слова </param>
        /// <param name="letterNumberFromEnd"> номер буквы с конца на которую бот возвращает значение (0 = ответ на последнюю букву)</param>
        /// <param name="wikiUrl"> Ссылка на википедию города</param>
        /// <param name="yandexUrl"> Ссылка на запрос в яндексе по городу</param>
        /// <param name="googleUrl"> Ссылка на запрос в гугле по городу</param>
        /// <param name="nextLetter"> Буква на которую должен отвечать пользователь</param>
        /// <param name="mapUrl"> Ссылка на место на карте</param>
        /// <param name="coordinateCity"> Координаты города (latitude - широта, longitude - долгота)</param>
        /// <param name="photoUrl"> Ссылка на фото из города</param>
        /// <param name="outCity"> null значит города не существует в бд, '' - все города отгаданы</param>
        /// <param name="coordinateUser"> latitude - широта, longitude - долгота</param>
        /// <param name="searchRadius"> в километрах</param>
        public static void Get(ref string city, string id, (decimal latitude, decimal longitude) coordinateUser, double searchRadius,
            out bool onLastLetter, out bool cityIsUsed, out string outCity, out char nextLetter, out byte letterNumberFromEnd, out string wikiUrl,
            out string yandexUrl, out string googleUrl, out string mapUrl, out (decimal latitude, decimal longitude) coordinateCity, out string photoUrl)
        {
            // изначальные значения
            outCity = null;
            letterNumberFromEnd = 0;
            onLastLetter = true;
            cityIsUsed = false;
            wikiUrl = null;
            yandexUrl = null;
            googleUrl = null;
            mapUrl = null;
            photoUrl = null;
            nextLetter = default;
            coordinateCity.longitude = default;
            coordinateCity.latitude = default;

            string[] splittedCities = city.Split();
            if (splittedCities.Length > 1)
            {
                splittedCities[0] = splittedCities[0].ToLower();
                splittedCities[1] = splittedCities[1].ToLower();
                // если название города состоит из 2 слов
                if (DoubleNameCities.Contains(splittedCities[0].ToLower()))
                    city = splittedCities[0] + " " + splittedCities[1];
                else
                    city = splittedCities[0];
            }
            else
            {
                city = splittedCities[0];
            }

            city = city.ToLower();
            if (_databaseCities.Contains(city))
            {
                outCity = "";
                if (!_databaseUsers.ContainsKey(id))
                    _databaseUsers.Add(id, new User(id)); // Если человека нет в базе пользователей
                else
                {
                    if (city[0] != _databaseUsers[id].NextLetter) // проверка введено ли на правильную букву
                    {
                        onLastLetter = false;
                        return;
                    }
                }

                if (_databaseUsers[id].UsedCities[city[0]].Contains(new City() { Name = city }, CityEqualityComparer)) // проверка на использованность города
                {
                    cityIsUsed = true;
                }
                else
                {
                    foreach (char c in city.Reverse())
                    {
                        if (!_databaseFind.ContainsKey(c))
                        {
                            letterNumberFromEnd++;
                            continue;
                        }
                        List<City> except = _databaseFind[c]
                            .Except(_databaseUsers[id].UsedCities[c], CityEqualityComparer).ToList();
                        foreach (City city1 in except)
                        {
                            double alpha2 = Math.Pow((searchRadius * 180d) / (Math.PI * 6371d), 2);
                            if (Math.Pow((double)(city1.Latitude - coordinateUser.latitude), 2) +
                                Math.Pow((double)(city1.Longitude - coordinateUser.longitude), 2) > alpha2)
                            {
                                except.Remove(city1);
                            }
                        }

                        if (except.Count != 0)
                        {
                            int numberOfCity =
                                (int)Math.Round((NormalRandom() - 1d / (except.Count * 2)) * except.Count);
                            outCity = except[numberOfCity].Name; // используется смещенное нормальное распределение чтобы давать более редкие города чаще

                            _databaseUsers[id].UsedCities[city[0]].Add(new City() { Name = city });
                            _databaseUsers[id].UsedCities[c].Add(new City() { Name = outCity.ToLower() });

                            bool userWin = true; // проверка на победу + nextLetter
                            for (int i = outCity.Length - 1; i > 0; i--)
                            {
                                nextLetter = outCity[i];
                                if (_databaseFind.ContainsKey(nextLetter))
                                    if (_databaseFind[nextLetter].Except(_databaseUsers[id].UsedCities[nextLetter], CityEqualityComparer).Any())
                                    {
                                        _databaseUsers[id].NextLetter = nextLetter;
                                        _databaseUsers[id].OutCity = outCity;
                                        _databaseUsers[id].LetterNumberFromEnd = letterNumberFromEnd;
                                        userWin = false;
                                        break;
                                    }
                            }

                            if (userWin)
                            {
                                outCity = "";
                            }
                            else
                            {
                                City outCityClass = except[numberOfCity];
                                wikiUrl = outCityClass.WikiUrl;
                                yandexUrl = outCityClass.YandexUrl;
                                googleUrl = outCityClass.GoogleUrl;
                                mapUrl = outCityClass.MapUrl;
                                photoUrl = outCityClass.PicUrl;
                                coordinateCity.longitude = outCityClass.Longitude;
                                coordinateCity.latitude = outCityClass.Latitude;
                            }
                            break;
                        }
                        else
                        {
                            letterNumberFromEnd++;
                        }
                    }
                }
            }
        }

        public static string DatabaseFindToString()
        {
            string s = "";
            foreach (KeyValuePair<char, List<City>> keyValuePair in _databaseFind)
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
            foreach (string city in _databaseCities)
            {
                s += city + ", ";
            }
            return s;
        }
    }
}