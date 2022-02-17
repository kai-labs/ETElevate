using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ETElevate
{
    public class DelimitedDataReader : IDisposable
    {
        private readonly char delimiter;
        private readonly Stream stream;
        

        private enum ReadState
        {   
            StartField,            
            ReadBareField,
            ReadQuotedField,
            LineDone
        }
                
        public DelimitedDataReader(char delimiter, Stream stream)
        {
            this.delimiter = delimiter;
            this.stream = stream;
        }

        public bool EndOfStream { get; private set; }

        public void Dispose()
        {
        }

        public IList<IList<string>> ReadAllLines()
        {
            var lines = new List<IList<string>>();

            while (!EndOfStream)
            {
                lines.Add(ReadLine());
            }

            return lines;
        }

        
        public IList<string> ReadLine()
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            var pendingChars = new StringBuilder();            
            var readState = ReadState.StartField;

            while (readState != ReadState.LineDone)
            {
                var value = stream.ReadByte();
                
                if (value == -1)
                {
                    EndOfStream = true;

                    fields.Add(currentField.ToString());
                    readState = ReadState.LineDone;
                }

                switch (readState)
                {
                    case ReadState.StartField:
                        if (value == '"')
                        {                            
                            readState = ReadState.ReadQuotedField;                            
                        }
                        else if (value == '\r' || value == '\n')
                        {
                            readState = ReadState.LineDone;
                        }
                        else if (value == delimiter)
                        {
                            fields.Add(string.Empty);
                            readState = ReadState.StartField;
                        }
                        else
                        {
                            currentField.Append((char)value);                            
                            readState = ReadState.ReadBareField;
                        }
                        break;

                    case ReadState.ReadBareField:                        
                        if (value == delimiter)
                        {
                            fields.Add(currentField.ToString());
                            currentField.Clear();
                            readState = ReadState.StartField;
                        }
                        else if (value == '\r')
                        {
                            // ignore this.
                        }
                        else if (value == '\n')
                        {
                            fields.Add(currentField.ToString());
                            currentField.Clear();
                            readState = ReadState.LineDone;
                        }
                        else
                        {
                            currentField.Append((char)value);
                        }
                        
                        break;

                    case ReadState.ReadQuotedField:
                        if (pendingChars.Length == 0)
                        {
                            if (value == '"')
                            {
                                pendingChars.Append((char)value);
                            }
                            else
                            {
                                currentField.Append((char)value);
                            }
                        }
                        else
                        {
                            pendingChars.Append((char)value);
                            var pendingString = pendingChars.ToString();

                            if (pendingString == "\"\"")
                            {
                                currentField.Append('"');
                                pendingChars.Clear();
                            }
                            else if (pendingString == "\"\r\n" || pendingString == "\"\n")
                            {
                                fields.Add(currentField.ToString());
                                currentField.Clear();
                                pendingChars.Clear();
                                readState = ReadState.LineDone;
                            }
                            else if (pendingString == $"\"{delimiter}")
                            {
                                fields.Add(currentField.ToString());
                                currentField.Clear();
                                pendingChars.Clear();
                                readState = ReadState.StartField;
                            }
                            else
                            {
                                throw new Exception("Invalid character string.  Expecting delimiter, quote, CRLF, or newline");
                            }
                        }
                        
                        break;
                }
            }

            return fields;
        }
    }
}
