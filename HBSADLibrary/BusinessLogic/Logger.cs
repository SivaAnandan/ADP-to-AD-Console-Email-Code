using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBSADLibrary.BusinessLogic
{
    public static class Logger
    {
        private static readonly string logDirectory = "Logs";
        private static string logFilePath;
        // Initialize the log file (create or clear it)
        public static void InitializeLogFile()
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string logFileName = $"log_{currentDate}.txt";
            logFilePath = Path.Combine(logDirectory, logFileName);

            try
            {
                // Ensure the Logs directory exists
                Directory.CreateDirectory(logDirectory);

                // Create or append to the log file
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"===== Log file started at {DateTime.Now} =====");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing log file: {ex.Message}");
            }
        }

        // Log message with Info severity
        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        // Log message with Warning severity
        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        // Log message with Error severity
        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        // Generic method to write log message
        private static void Log(string logLevel, string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now} [{logLevel}] - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        // Example of logging an exception
        public static void LogException(Exception ex)
        {
            LogError($"Exception: {ex.GetType().Name} - {ex.Message}");
            LogError($"Stack Trace:\n{ex.StackTrace}");
        }

    }
}
