using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLP;

namespace PostAppConsole
{
    class Program
    {
        static string LoadAndReadFolderFiles(string folderName)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\" + folderName;
            Console.WriteLine("Read File Path: [" + path + "]");
            string text = FileReader.GetAllTextFromDirectoryAsString(path);
            return text;
        }

        static void WriteToTxtFile(string fileName, Tagger gTagger)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\Informations\\" + fileName;
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
            var text = LoadAndReadFolderFiles(Brownfolder);
            var words = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(text));

            //  foreach (var item in words)
            //     Console.WriteLine(item.word + " -> " + item.tag);

            var tags = SpeechPart.SpeechPartFrequence(words);
            var sortedDict = from entry in tags orderby entry.Value descending select entry;

            //Console.WriteLine(tags.Count);
            // foreach (var item in sortedDict)
            //   Console.WriteLine(item.Key + " -> " + item.Value);

            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\Informations\\" + "FULL_LIST.tags.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                int k = 0;
                sw.WriteLine("Nr of tags: " + tags.Count);
                foreach (var model in sortedDict)
                {
                    sw.WriteLine(k + ": " + model);
                    k++;
                }
                sw.Dispose();
            }

            path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\Informations\\" + "words.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                int k = 0;
                sw.WriteLine("Nr of words: " + words.Count);
                foreach (var model in words)
                {
                    sw.WriteLine(k + ": " + model.word);
                    k++;
                }
                sw.Dispose();
            }

            var dictionar = new Dictionary<string, int>();
            foreach (var item in sortedDict)
                if (item.Key.Contains('-') || item.Key.Contains('+') || item.Key.Contains('*') || item.Key.Contains('$') && item.Key.Length > 1)
                    continue;
                else
                {
                    dictionar.Add(item.Key, item.Value);
                }

            path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\Informations\\" + "ONLY.tags.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                int k = 0;
                sw.WriteLine("Nr of tags: " + dictionar.Count);
                foreach (var model in dictionar)
                {
                    sw.WriteLine(k + ": " + model);
                    k++;
                }
                sw.Dispose();
            }


            //Console.WriteLine("Done with loading and creating tokens!");
            //Tagger gTagger = new Tagger(words);
            //Console.WriteLine("Done with training MODEL!");

            //foreach (var model in gTagger.Models)
            //{
            //    Console.WriteLine(model.Word);
            //    foreach (var item in model.TagFreq)
            //    {
            //        Console.WriteLine("     " + item.Key + " -> " + item.Value);
            //    }
            //}

            //Console.WriteLine("Duration of training model: " + gTagger.GetTrainingTimeMs() + " ms!");

            //WriteToTxtFile("svm_brown_corpus.txt", gTagger);

            //Console.WriteLine("\n\n. . .");
            //string tester = "I think perhaps you miss the point entirely.";
            //var testText = Tokenizer.WordsOnlyTokenize(tester);
            //foreach (var item in testText)
            //    Console.WriteLine(item);

            //Console.WriteLine("\n");
            //var grammar = gTagger.EasyWordTag(testText);
            //foreach (var elem in grammar)
            //    Console.WriteLine(elem.Key + " -> " + elem.Value);

        }
    }
}
