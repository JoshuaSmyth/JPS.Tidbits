using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace CSharp.SimpleErrorLogging
{
    public enum ExceptionType
    {
        FirstChance,
        Unhandled
    }

    public class ExceptionLogEntry
    {
        public ExceptionType ExceptionType;
        public String Message;
        public String StackTrace;
        public String Hash;                 // This hash is good to get a count of a particular exception
        public Int32 hResult;
    }

    public static class Logger
    {
        private static IExceptionLogConnector _exceptionLogConnector;

        // Note: A list might not be the best datastructure for this
        private static List<ExceptionLogEntry> m_ExceptionLogEntries = new List<ExceptionLogEntry>();
        
        public static void Initalize(IExceptionLogConnector logConnector) {
            _exceptionLogConnector = logConnector;
            AppDomain.CurrentDomain.UnhandledException += Logger.CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += Logger.CurrentDomain_FirstChanceException; // dotNet4.0 Feature
        }

        public static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            // This will fire on any exception even if caught
            LogException(ExceptionType.FirstChance, e.Exception);
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // This will fire on any exception
            LogException(ExceptionType.Unhandled, e.ExceptionObject as Exception);
        }

        private static void LogException(ExceptionType exceptionType, Exception exception) {

            var entry = new ExceptionLogEntry()
            {
                ExceptionType = exceptionType,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Hash = CalculateMd5Hash(exception.Message + exception.StackTrace),
                hResult = Marshal.GetHRForException(exception)
            };

            m_ExceptionLogEntries.Add(entry);
            _exceptionLogConnector.WriteException(entry);
        }

        public static string CalculateMd5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
