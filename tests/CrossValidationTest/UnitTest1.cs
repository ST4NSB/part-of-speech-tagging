using NUnit.Framework;
using NLP;

namespace CrossValidationTest
{
    public class Tests
    {
        CrossValidation cv;
        [SetUp]
        public void Setup()
        {
            string path = "testdir\\";
            cv = new CrossValidation(filePath: path, fold: 4 , shuffle: false);
        }

        [Test]
        public void CrossValidationTestFiles()
        {
            string[] expectedTest =
            {
                "File:1-Text:a.\r\nFile:1-Text:b.",
                "File:2-Text:c.\r\nFile:2-Text:d.",
                "File:3-Text:e.\r\nFile:3-Text:f.",
                "File:4-Text:g.\r\nFile:4-Text:h."
            };

            Assert.AreEqual(expectedTest, cv.TestFile);
        }

        [Test]
        public void CrossValidationTrainFiles()
        {
            string[] expectedTest =
            {
                "File:2-Text:c.\r\nFile:2-Text:d.  File:3-Text:e.\r\nFile:3-Text:f.  File:4-Text:g.\r\nFile:4-Text:h.",
                "File:1-Text:a.\r\nFile:1-Text:b.  File:3-Text:e.\r\nFile:3-Text:f.  File:4-Text:g.\r\nFile:4-Text:h.",
                "File:1-Text:a.\r\nFile:1-Text:b.  File:2-Text:c.\r\nFile:2-Text:d.  File:4-Text:g.\r\nFile:4-Text:h.",
                "File:1-Text:a.\r\nFile:1-Text:b.  File:2-Text:c.\r\nFile:2-Text:d.  File:3-Text:e.\r\nFile:3-Text:f."
            };

            Assert.AreEqual(expectedTest, cv.TrainFile);
        }
    }
}