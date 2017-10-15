using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleJsonWriter
{
    /// <summary>
    /// A simple json writer, provides no string formating
    /// </summary>
    public class SimpleJsonTextWriter
    {
        private StringBuilder _StringBuilder = new StringBuilder();
        bool startProperty = false;
        bool startObject = false;
        bool startArray = false;
        public void Start() {
            _StringBuilder.Append("{");
            startObject = true;
        }

        public void WriteProperty(string key, string value) {
            CheckComma();
            startProperty = true;
            _StringBuilder.Append("\"");
            _StringBuilder.Append(key);
            _StringBuilder.Append("\":\"");
            _StringBuilder.Append(value);
            _StringBuilder.Append("\"");
        }

        private void CheckComma() {
            if (startProperty == true) {
                _StringBuilder.Append(",");
            }
        }

        public void End() {
            _StringBuilder.Append("}");
        }

        public string ToString() {
            return _StringBuilder.ToString();
        }

        public void StartArray(string name) {
            CheckComma();

            _StringBuilder.Append("\"");
            _StringBuilder.Append(name);
            _StringBuilder.Append("\":[");

            startProperty = false;
            startArray = true;
        }

        public void EndArray() {
            startProperty = true;
            startArray = false;
            _StringBuilder.Append("]");
        }

        public void WriteString(string value) {
            CheckComma();
            startProperty = true;
            _StringBuilder.Append("\"");
            _StringBuilder.Append(value);
            _StringBuilder.Append("\"");
        }
    }
}
