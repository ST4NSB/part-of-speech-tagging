using System.IO;
using System.Collections.Generic;

namespace FileLogic
{
    class FileReader
    {
        /// <summary>
        /// Static method to read all text from method argument file and returns a string.
        /// </summary>
        /// <returns>Returns a string.</returns>
        public static string GetTextFromFileAsString(string fileName)
        {
            string file = File.ReadAllText(fileName);
            return file;
        }
        /// <summary>
        /// Static method to read all lines from method argument file and returns a List of string.
        /// </summary>
        /// <returns>Returns List of string.</returns>
        public static List<string> GetTextFromFileAsList(string fileName)
        {
            List<string> textFileList = new List<string>();
            var lines = File.ReadLines(fileName);
            foreach (var line in lines)
                textFileList.Add(line);
            return textFileList;
        }
        
    }
}

