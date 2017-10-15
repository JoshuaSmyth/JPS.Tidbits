using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleJsonWriter
{
    
    class Program
    {
        static void Main(string[] args)
        {
            var writer = new SimpleJsonTextWriter();

            writer.Start();
                writer.WriteProperty("CPU", "Intel");
                writer.WriteProperty("PSU", "500W");

                writer.StartArray("Drives");
                    writer.WriteString("DVD read/write");
                    writer.WriteString("256GB SSD");
                    writer.WriteString("1TB HDD");
                writer.EndArray();

            writer.End();

            var value = writer.ToString();
        }
    }
}
