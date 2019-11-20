using System.IO;
using System.Collections.Generic;

namespace NLP
{
    public class FileReader
    {
        /// <summary>
        /// Static method to read all text from inputFile and returns a string.
        /// </summary>
        /// <param name="inputFile">Data input file text.</param> 
        /// <returns>Returns a string.</returns>
        public static string GetTextFromFileAsString(string inputFile)
        {
            string outputFile = File.ReadAllText(inputFile);
            return outputFile;
        }
        /// <summary>
        /// Static method to read all lines from inputFile and returns a List of strings.
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
        /// Static method to read all lines from inputDir (directory) and returns a string.
        /// </summary>
        /// <param name="inputDir">Directory input file.</param>
        /// <returns>Returns a string.</returns>
        public static string GetAllTextFromDirectoryAsString(string inputDir)
        {
            string outputFile = "";
            var files = Directory.EnumerateFiles(inputDir);
            foreach (string file in files)
            {
                outputFile += GetTextFromFileAsString(file);
            }
            return outputFile;
        }
        /// <summary>
        /// Static method to read all lines from inputDir (directory) and returns a List of string.
        /// </summary>
        /// <param name="inputDir">Directory input file.</param>
        /// <returns>Returns a List.</returns>
        public static List<string> GetAllTextFromDirectoryAsList(string inputDir)
        {
            List<string> outputFile = new List<string>();
            var files = Directory.GetFiles(inputDir);
            foreach (string file in files)
            {
                string elem = GetTextFromFileAsString(file);
                outputFile.Add(elem);
            }
            return outputFile;
        }
    }
}

