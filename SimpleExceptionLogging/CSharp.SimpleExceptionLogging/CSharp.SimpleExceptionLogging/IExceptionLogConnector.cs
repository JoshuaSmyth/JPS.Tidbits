using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.SimpleErrorLogging
{
    public interface IExceptionLogConnector
    {
        void WriteLine(string output);
        void Write(string output);
        void WriteException(ExceptionLogEntry exception);
    }
}
