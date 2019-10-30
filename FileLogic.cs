using System.IO;
using System.Collections.Generic;
using Nlp.PosTagger;

namespace FileLogic
{
    public struct DirectoryFile
    {
        public string Name;
        public string Text;
        /// <summary>
        /// Public constructor to load the directory file name and file text.
        /// </summary>
        /// <param name="Name">File name. (ex. "info.csv" -> info)</param>
        /// <param name="Text">File data text.</param>
        public DirectoryFile(string Name, string Text)
        {
            this.Name = Name;
            this.Text = Text;
        }
    }

    class FileReader
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
        /// Static method to read all lines from inputFile (directory) and return a List of structs (DirectoryFile).
        /// </summary>
        /// <param name="inputFile">Directory input file.</param>
        /// <returns>Returns a List.</returns>
        public static List<DirectoryFile> GetAllTextFromFileAsList(string inputFile)
        {
            List<DirectoryFile> outputFile = new List<DirectoryFile>();
            var files = Directory.GetFiles(inputFile);
            foreach (string file in files)
            {
                DirectoryFile elem = new DirectoryFile(Path.GetFileName(file), GetTextFromFileAsString(file));
                outputFile.Add(elem);
            }
            return outputFile;
        }

    }
}

