using System;
using System.Collections.Generic;
using System.IO;
using NLP;

namespace PostAppConsole
{
    class Program
    {
        static string ReadTestFile(string folderName)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\" + folderName;
            Console.WriteLine("File Path: [" + path +"]");
            string text = FileReader.GetAllTextFromDirectoryAsString(path);
            return text;
        }

        static void Main(string[] args)
        {
            var text = ReadTestFile("test_files");
            var words = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenize(text));
            //foreach (var item in words)
            //    Console.WriteLine(item.word + " -> " + item.tag);

            var timer = new System.Diagnostics.Stopwatch();

            timer.Start();
            GrammarTagger gTagger = new GrammarTagger(words);
            timer.Stop();
            Console.WriteLine("Duration of training model: " + timer.ElapsedMilliseconds + " ms!");

            foreach (var model in gTagger.Models)
            {
                Console.WriteLine(model.Word);
                foreach (var item in model.TagFreq)
                {
                    Console.WriteLine("     " + item.Key + " -> " + item.Value);
                }
            }

        }
    }
}
