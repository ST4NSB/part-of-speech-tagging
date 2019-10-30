using System.IO;
using System.Collections.Generic;

namespace FileLogic
{
    class FileReader
    {
        /// <summary>
        /// Static method to read all text from inputFile and return a string.
        /// </summary>
        /// <param name="inputFile"></param> 
        /// <returns>Returns a string.</returns>
        public static string GetTextFromFileAsString(string inputFile)
        {
            string outputFile = File.ReadAllText(inputFile);
            return outputFile;
        }
        /// <summary>
        /// Static method to read all lines from inputFile and return a List of strings.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns>Returns List of string.</returns>
        public static List<string> GetTextFromFileAsList(string inputFile)
        {
            List<string> outputFile = new List<string>();
            var lines = File.ReadLines(inputFile);
            foreach (var line in lines)
            {
                outputFile.Add(line);
            }
            return outputFile;
        }
        /// <summary>
        /// Static method to read all lines from inputFile (directory) and return a string.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns>Returns a string.</returns>
        public static string GetAllTextFromFileAsString(string inputFile)
        {
            string outputFile = "";
            var files = Directory.EnumerateFiles(inputFile);
            foreach (string file in files)
            {
                outputFile += GetTextFromFileAsString(file);
            }
            return outputFile;
        }
        /// <summary>
        /// Static method to read all lines from inputFile (directory) and return a List of strings.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns>Returns List of strings.</returns>
        public static List<string> GetAllTextFromFileAsList(string inputFile)
        {
            List<string> outputFile = new List<string>();
            var files = Directory.EnumerateFiles(inputFile);
            foreach (string file in files)
            {
                outputFile.Add(GetTextFromFileAsString(file));
            }
            return outputFile;
        }

    }
}

