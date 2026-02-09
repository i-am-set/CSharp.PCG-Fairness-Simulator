using System;
using System.IO;
using System.Text;

namespace Core.Infrastructure
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logPath = "simulation.log";

        static Logger()
        {
            try
            {
                File.WriteAllText(_logPath, $"--- Simulation Session Started: {DateTime.Now} ---\n");
            }
            catch {}
        }

        public static void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string line = $"[{timestamp}] INFO: {message}";

            lock (_lock)
            {
                Console.WriteLine(line);
                try
                {
                    File.AppendAllText(_logPath, line + Environment.NewLine);
                }
                catch { }
            }
        }

        public static void LogError(string message, Exception ex = null)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string line = $"[{timestamp}] ERROR: {message}";
            if (ex != null)
            {
                line += $"\nException: {ex.Message}\nStack: {ex.StackTrace}";
            }

            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(line);
                Console.ResetColor();
                try
                {
                    File.AppendAllText(_logPath, line + Environment.NewLine);
                }
                catch { }
            }
        }
    }
}