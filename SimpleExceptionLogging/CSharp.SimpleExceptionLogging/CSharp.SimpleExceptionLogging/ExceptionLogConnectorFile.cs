using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharp.SimpleErrorLogging
{
    public class ExceptionLogConnectorFile : IExceptionLogConnector, IDisposable
    {
        readonly StreamWriter m_FileStream;
        public ExceptionLogConnectorFile(String filename) {
           m_FileStream = new StreamWriter(filename);
           m_FileStream.AutoFlush = true;
        }

        public void WriteLine(string output) {
            m_FileStream.WriteLine(output);
        }

        public void Write(string output) {
            m_FileStream.Write(output);
        }

        public void Dispose() {
            m_FileStream.Close();
        }

        public void WriteException(ExceptionLogEntry entry) {
            WriteLine(entry.ExceptionType.ToString());
            WriteLine(entry.Message);
            WriteLine(entry.StackTrace);
            WriteLine(entry.Hash);
            WriteLine(entry.hResult.ToString());
        }
    }
}
