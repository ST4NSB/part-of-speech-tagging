﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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

        static void WriteToTxtFile(string folderName, string fileName, string jsonFile)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\" + folderName + "\\" + fileName;
            Console.WriteLine("Write File Path: [" + path + "]");
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.Write(jsonFile);
                    sw.Dispose();
                }
            }
            else Console.WriteLine("Couldn't write to file (File already exists)!");
        }

        static void Main(string[] args)
        {
            const string Brownfolder = "Brown Corpus", testFile = "Test Files";
            var text = LoadAndReadFolderFiles(testFile);
            var words = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(text));

            foreach (var item in words)
                Console.WriteLine(item.word + "->" + item.tag);

            var tags = SpeechPart.SpeechPartFrequence(words);
            var sorted = from entry in tags orderby entry.Value descending select entry;

            var sortedDict = new Dictionary<string, int>(sorted.ToDictionary(x => x.Key, x => x.Value));
            foreach (var item in sortedDict)
                Console.WriteLine(item);
            //WriteToTxtFile("Informations", "List_Tags.json", JsonConvert.SerializeObject(sortedDict));
           // WriteToTxtFile("Informations", "wordAndTag.json", JsonConvert.SerializeObject(words));
            var dictionar = new Dictionary<string, int>();
            foreach (var item in sortedDict)
                if (item.Key.Contains('-') || item.Key.Contains('+'))
                    continue;
                else
                {
                    dictionar.Add(item.Key, item.Value);
                }
            // WriteToTxtFile("Informations", "Unique_Tags.json", JsonConvert.SerializeObject(dictionar));


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
