using System;
using System.Collections.Generic;
using System.IO;
using NLP;

namespace PostAppConsole
{
    class Program
    {
        static string LoadAndReadFolderFiles(string folderName)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\" + folderName;
            Console.WriteLine("Read File Path: [" + path +"]");
            string text = FileReader.GetAllTextFromDirectoryAsString(path);
            return text;
        }

        static void WriteToTxtFile(string fileName, GrammarTagger gTagger)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\output_files\\" + fileName;
            Console.WriteLine("Write File Path: [" + path + "]");
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    int k = 0;
                    sw.WriteLine("Duration of training model: " + gTagger.GetTrainingTimeMs() + " ms!");
                    foreach (var model in gTagger.Models)
                    {
                        sw.WriteLine("[" + k + "]  " + model.Word);
                        foreach (var item in model.TagFreq)
                        {
                            sw.WriteLine("          - " + item.Key + " -> " + item.Value);
                        }
                        k++;
                    }
                    sw.Dispose();
                }
            }
            else Console.WriteLine("Couldn't write to file (File already exists)!");
        }

        static void Main(string[] args)
        {
            const string Brownfolder = "Brown Corpus", testFile2 = "test_files2", testFile = "test_files";
            var text = LoadAndReadFolderFiles(testFile);
            var words = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenize(text));
            // foreach (var item in words)
            //      Console.WriteLine(item.word + " -> " + item.tag);

            Console.WriteLine("Done with loading and creating tokens!");
            GrammarTagger gTagger = new GrammarTagger(words);
            Console.WriteLine("Done with training MODEL!");

            //foreach (var model in gTagger.Models)
            //{
            //    Console.WriteLine(model.Word);
            //    foreach (var item in model.TagFreq)
            //    {
            //        Console.WriteLine("     " + item.Key + " -> " + item.Value);
            //    }
            //}

            Console.WriteLine("Duration of training model: " + gTagger.GetTrainingTimeMs() + " ms!");

            WriteToTxtFile("test.txt", gTagger);
        }
    }
}
