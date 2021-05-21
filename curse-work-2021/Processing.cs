using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace curse_work_2021
{
    public static class Database
    {
        private static Dictionary<string, List<(string city, int population)>> database;

        public static void ReadCsv()
        {
            // open the file "data.csv" which is a CSV file with headers
            using (CsvReader csv =
            new CsvReader(new StreamReader("Russian_cities.csv"), true))
            {
                int fieldCount = csv.FieldCount;

                string[] headers = csv.GetFieldHeaders();
                while (csv.ReadNextRecord())
                {
                    //if (database.ContainsKey(csv[1]))
                    //{
                    //    database.Add(csv[1], new List<(string city, int population)>());
                    //}
                    //database[csv[1]].Add((csv[2], int.Parse(csv[3])));
                    for (int i = 2; i < fieldCount; i++)
                        Console.Write(string.Format("{0} = {1}; ",
                            headers[i], csv[i]));
                }
            }
        }

        public static void Get(string city, string id, out string outCity, out bool isLastLetter, out string wikiUrl,
                out string yandexUrl, out string googleUrl, out string mapUrl, out string photoUrl)
        {
            outCity = null;
            isLastLetter = false;
            wikiUrl = null;
            yandexUrl = null;
            googleUrl = null;
            mapUrl = null;
            photoUrl = null;
        }

        /// <summary>
        /// get с поиском по окружности в километрах
        /// </summary>
        /// <param name="coordinate"> latitude - широта, longitude - долгота</param>
        /// <param name="searchRadius"> в километрах</param>
        public static void Get(string city, string id, (double latitude, double longitude) coordinate, double searchRadius,
            out string outCity, out bool isLastLetter, out string wikiUrl, out string yandexUrl,
            out string googleUrl, out string mapUrl, out string photoUrl)
        {
            outCity = null;
            isLastLetter = false;
            wikiUrl = null;
            yandexUrl = null;
            googleUrl = null;
            mapUrl = null;
            photoUrl = null;
        }

        public static string ToString()
        {
            string s = "";
            foreach (KeyValuePair<string, List<(string city, int population)>> keyValuePair in database)
            {
                bool first = true;
                s += keyValuePair.Key + ": ";
                foreach ((string city, int population) tuple in keyValuePair.Value)
                {
                    if (first)
                    {
                        s += tuple.city + " " + tuple.population;
                        first = false;
                    }
                    else
                    {
                        s += "   " + tuple.city + " " + tuple.population;
                    }
                }
            }
            return s;
        }
    }
}