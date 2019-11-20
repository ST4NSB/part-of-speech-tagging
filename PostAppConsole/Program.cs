using System;
using System.Collections.Generic;
using System.IO;
using NLP;

namespace PostAppConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\test_files\\ca01";
            //Console.WriteLine(path);
            string text = FileReader.GetTextFromFileAsString(path);
            var texts = Tokenizer.WordTokenize(text);
            //  foreach (var item in texts)
            //       Console.WriteLine(item);

            var words = Tokenizer.SeparateTagFromWord(texts);
            foreach (var item in words)
                Console.WriteLine(item.word + "->" + item.tag);

        }
    }
}
