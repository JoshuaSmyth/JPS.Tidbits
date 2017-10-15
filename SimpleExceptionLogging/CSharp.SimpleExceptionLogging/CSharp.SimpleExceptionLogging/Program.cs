using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace CSharp.SimpleErrorLogging
{
    class Program
    {
        static void Main(string[] args)
        {
            // This example demonstrates a way of logging both
            // First chance exceptions and Unhandled exceptions

            Logger.Initalize(new ExceptionLogConnectorFile("output.log"));
           // try 
            {
                string fileContents = File.ReadAllText(args[0]);
            }
         //   catch (IndexOutOfRangeException exception)
            {
                // Handled
            }

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
