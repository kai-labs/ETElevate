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
                
        [Test]
        public void CanReadCsvMultiLines()
        {
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes("One,Two,Three\r\nFour,Five,Six")))
            {
                using (var reader = new DelimitedDataReader(',', memoryStream))
                {
                    var lines = reader.ReadAllLines();
                    Assert.AreEqual("One", lines[0][0]);
                    Assert.AreEqual("Two", lines[0][1]);
                    Assert.AreEqual("Three", lines[0][2]);
                    Assert.AreEqual("Four", lines[1][0]);
                    Assert.AreEqual("Five", lines[1][1]);
                    Assert.AreEqual("Six", lines[1][2]);
                }
            }            
        }

        [Test]
        public void CanReadCsvMultiLinesWithNestedCRLF()
        {
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes("One,\"\r\nTwo\",Three\r\nFour,Five,Six")))
            {
                using (var reader = new DelimitedDataReader(',', memoryStream))
                {
                    var lines = reader.ReadAllLines();
                    Assert.AreEqual("One", lines[0][0]);
                    Assert.AreEqual("\r\nTwo", lines[0][1]);
                    Assert.AreEqual("Three", lines[0][2]);
                    Assert.AreEqual("Four", lines[1][0]);
                    Assert.AreEqual("Five", lines[1][1]);
                    Assert.AreEqual("Six", lines[1][2]);
                }
            }
        }

        [Test]
        public void CanReadCsvSingleLineWithEmptyFields()
        {
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes("One,,Three\r\n\"\",Five,Six")))
            {
                using (var reader = new DelimitedDataReader(',', memoryStream))
                {
                    var lines = reader.ReadAllLines();
                    Assert.AreEqual("One", lines[0][0]);
                    Assert.AreEqual(string.Empty, lines[0][1]);
                    Assert.AreEqual("Three", lines[0][2]);
                    Assert.AreEqual(string.Empty, lines[1][0]);
                    Assert.AreEqual("Five", lines[1][1]);
                    Assert.AreEqual("Six", lines[1][2]);
                }
            }
        }
    }
}
