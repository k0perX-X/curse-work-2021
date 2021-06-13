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

        public Logging(LevelLogging levelLogging, string pathToLogFile)
        {
            if (levelLogging < 0 || levelLogging > 4)
                levelLogging = new(1);
            _levelLogging = levelLogging;
            FileInfo fileInf = new FileInfo(pathToLogFile);
            if (!fileInf.Exists)
                fileInf.Create();
            _pathToLogFile = pathToLogFile;
        }

        private string _pathToLogFile;
        private int _levelLogging;

        public void DEBUG(string message = "")
        {
            if (_levelLogging >= Level.DEBUG)
                File.AppendAllText(_pathToLogFile,
                    $"{DateTime.Now.ToShortTimeString()} DEBUG: {message}");
        }

        public void INFO(string message = "")
        {
            if (_levelLogging >= Level.INFO)
                File.AppendAllText(_pathToLogFile,
                    $"{DateTime.Now.ToShortTimeString()} INFO: {message}");
        }

        public void ERROR(string message = "")
        {
            if (_levelLogging >= Level.ERROR)
                File.AppendAllText(_pathToLogFile,
                    $"{DateTime.Now.ToShortTimeString()} ERROR: {message}");
        }

        public void WARNING(string message = "")
        {
            if (_levelLogging >= Level.WARNING)
                File.AppendAllText(_pathToLogFile,
                    $"{DateTime.Now.ToShortTimeString()} WARNING: {message}");
        }

        public void FATAL(string message = "")
        {
            if (_levelLogging >= Level.FATAL)
                File.AppendAllText(_pathToLogFile,
                    $"{DateTime.Now.ToShortTimeString()} FATAL: {message}");
        }
    }
}