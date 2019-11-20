using System.IO;
using System.Collections.Generic;

namespace NLP
{
    public class FileReader
    {
        /// <summary>
        /// Static method to read all text from inputFile and return a string.
        /// </summary>
        /// <param name="inputFile">Data input file text.</param> 
        /// <returns>Returns a string.</returns>
        public static string GetTextFromFileAsString(string inputFile)
        {
            string outputFile = File.ReadAllText(inputFile);
            return outputFile;
        }
        /// <summary>
        /// Static method to read all lines from inputFile and return a List of strings.
        /// </summary>
        /// <param name="inputFile">Data input file text.</param>
        /// <returns>Returns a List.</returns>
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
        /// <param name="inputFile">Directory input file.</param>
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
        /// Static method to read all lines from inputFile (directory) and return a List of string.
        /// </summary>
        /// <param name="inputFile">Directory input file.</param>
        /// <returns>Returns a List.</returns>
        public static List<string> GetAllTextFromFileAsList(string inputFile)
        {
            List<string> outputFile = new List<string>();
            var files = Directory.GetFiles(inputFile);
            foreach (string file in files)
            {
                string elem = GetTextFromFileAsString(file);
                outputFile.Add(elem);
            }
            return outputFile;
        }
    }
}

