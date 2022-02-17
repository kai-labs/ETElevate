using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ETElevate.Tests
{
    [TestFixture]
    public class DelimitedDataReaderTests
    {
        [TestCase("Red,Green,Blue\r\n", "Red", "Green", "Blue")]        
        [TestCase("Red,\"Green,Light\",Blue\r\n", "Red", "Green,Light", "Blue")]
        [TestCase("Red,\"Green,\nLight\",Blue\r\n", "Red", "Green,\nLight", "Blue")]
        [TestCase("Red,\"Green,\r\nLight\",Blue\r\n", "Red", "Green,\r\nLight", "Blue")]
        [TestCase("\"Red\"\"Bright\"\"\",\"Green,\r\nLight\",Blue\r\n", "Red\"Bright\"", "Green,\r\nLight", "Blue")]
        [TestCase("\"Elephant,\r\nour favorite animal\",Africa", "Elephant,\r\nour favorite animal", "Africa")]
        public void CanReadCsvSingleLine(string line, params string[] expectedValues)
        {
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(line)))
            {
                using (var reader = new DelimitedDataReader(',', memoryStream))
                {
                    var fields = reader.ReadLine();

                    for (var i = 0; i < expectedValues.Length; i++)
                    {
                        Assert.AreEqual(expectedValues[i], fields[i]);
                    }
                }
            }
        }
    }
}
