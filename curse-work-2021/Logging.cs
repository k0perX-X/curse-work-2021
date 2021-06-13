using System;
using System.IO;

namespace Database
{
    public class Logging
    {
        public static class Level
        {
            public static readonly LevelLogging DEBUG = new(0);
            public static readonly LevelLogging INFO = new(1);
            public static readonly LevelLogging ERROR = new(2);
            public static readonly LevelLogging WARNING = new(3);
            public static readonly LevelLogging FATAL = new(4);
        }

        public class LevelLogging
        {
            public LevelLogging(byte numberOfLevel) => _numberOfLevel = numberOfLevel;

            private readonly byte _numberOfLevel;

            public static implicit operator byte(LevelLogging d) => d._numberOfLevel;
        }

        public Logging(LevelLogging levelLogging, string pathToLogFile, bool writeInConsole = false)
        {
            if (levelLogging < 0 || levelLogging > 4)
                levelLogging = new(1);
            _levelLogging = levelLogging;
            FileInfo fileInf = new FileInfo(pathToLogFile);
            if (!fileInf.Exists)
                fileInf.Create();
            _pathToLogFile = pathToLogFile;
            _writeInConsole = writeInConsole;
        }

        private string _pathToLogFile;
        private int _levelLogging;
        private bool _writeInConsole;

        public void DEBUG(string message = "")
        {
            if (_levelLogging >= Level.DEBUG)
            {
                string mes = $"{DateTime.Now.ToShortTimeString()} DEBUG: {message}";
                File.AppendAllText(_pathToLogFile, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void INFO(string message = "")
        {
            if (_levelLogging >= Level.INFO)
            {
                string mes = $"{DateTime.Now.ToShortTimeString()} INFO: {message}";
                File.AppendAllText(_pathToLogFile, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void ERROR(string message = "")
        {
            if (_levelLogging >= Level.ERROR)
            {
                string mes = $"{DateTime.Now.ToShortTimeString()} ERROR: {message}";
                File.AppendAllText(_pathToLogFile,, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void WARNING(string message = "")
        {
            if (_levelLogging >= Level.WARNING)
            {
                string mes = $"{DateTime.Now.ToShortTimeString()} WARNING: {message}";
                File.AppendAllText(_pathToLogFile,, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void FATAL(string message = "")
        {
            if (_levelLogging >= Level.FATAL)
            {
                string mes = $"{DateTime.Now.ToShortTimeString()} FATAL: {message}";
                File.AppendAllText(_pathToLogFile,, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }
    }
}