using System;
using System.IO;

namespace Database
{
    public class Logging
    {
        public enum LevelLogging : byte
        {
            DEBUG = 0,
            INFO = 1,
            WARNING = 2,
            ERROR = 3,
            FATAL = 4,
            NOLOGGING = 5,
        }

        public Logging(LevelLogging levelLogging, string pathToLogFile, bool writeInConsole = false)
        {
            _levelLogging = levelLogging;
            FileInfo fileInf = new FileInfo(pathToLogFile);
            if (!fileInf.Exists)
                fileInf.Create();
            _pathToLogFile = pathToLogFile;
            _writeInConsole = writeInConsole;
        }

        private readonly string _pathToLogFile;
        private readonly LevelLogging _levelLogging;
        private readonly bool _writeInConsole;

        public void DEBUG(string message = "")
        {
            if (_levelLogging <= LevelLogging.DEBUG)
            {
                string mes = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} DEBUG: {message}";
                File.AppendAllText(_pathToLogFile, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void INFO(string message = "")
        {
            if (_levelLogging <= LevelLogging.INFO)
            {
                string mes = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} INFO: {message}";
                File.AppendAllText(_pathToLogFile, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void ERROR(string message = "")
        {
            if (_levelLogging <= LevelLogging.ERROR)
            {
                string mes = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} ERROR: {message}";
                File.AppendAllText(_pathToLogFile, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void WARNING(string message = "")
        {
            if (_levelLogging <= LevelLogging.WARNING)
            {
                string mes = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} WARNING: {message}";
                File.AppendAllText(_pathToLogFile, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }

        public void FATAL(string message = "")
        {
            if (_levelLogging <= LevelLogging.FATAL)
            {
                string mes = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} FATAL: {message}";
                File.AppendAllText(_pathToLogFile, mes);
                if (_writeInConsole)
                    Console.WriteLine(mes);
            }
        }
    }
}