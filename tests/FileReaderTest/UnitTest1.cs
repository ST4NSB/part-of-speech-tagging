using NUnit.Framework;
using NLP;
using System.Collections.Generic;

namespace FileReaderTest
{
    public class Tests
    {
        const string fileStr = "filereadertest";
        const string dir = "testdir\\";        
        
        [Test]
        public void GetTextFromFileAsStringTest()
        {
            const string expected = " This is a test file.\r\n\r\n\r\n" +
            "All text from here should be read! End.\r\n\r\n\r\n" +
            "Characters: ~!@#$%^&*()_+{}:\"<>?[];',./\\|";
            string res = FileReader.GetTextFromFileAsString(fileStr);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void GetTextFromFileAsListTest()
        {
            List<string> expected = new List<string>()
            {
                " This is a test file.",
                "",
                "",
                "All text from here should be read! End.",
                "",
                "",
                "Characters: ~!@#$%^&*()_+{}:\"<>?[];',./\\|"
            };
            List<string> res = FileReader.GetTextFromFileAsList(fileStr);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void GetAllTextFromDirectoryAsStringTest()
        {
            string expected = "This is from 1st document." +
                "This is from 2nd document.\r\n\r\n" +
                "2ndDoc text!";
            string res = FileReader.GetAllTextFromDirectoryAsString(dir);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void GetAllTextFromDirectoryAsListTest()
        {
            List<string> expected = new List<string>()
            {
                "This is from 1st document.",
                "This is from 2nd document.\r\n\r\n" +
                "2ndDoc text!"
            };
            List<string> res = FileReader.GetAllTextFromDirectoryAsList(dir);
            Assert.AreEqual(expected, res);
        }
    }
}