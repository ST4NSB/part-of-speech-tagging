using System;
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
            const string BrownfolderTrain = "Brown Corpus\\1_Train", BrownfolderTest = "Brown Corpus\\2_Test", testFile = "Test Files";
            var text = LoadAndReadFolderFiles(BrownfolderTrain);
            var oldWords = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(text));
            var words = SpeechPart.GetNewHierarchicTags(oldWords);

            //using (System.IO.StreamWriter file = new System.IO.StreamWriter("Brown_Corpus_cu_tagurile_mele.txt"))
            //{
            //    foreach (var item in words)
            //        file.WriteLine(item.word + " " + item.tag);
            //}

            // foreach (var item in oldWords)
            //     Console.WriteLine(item.word + "->" + item.tag);

            //int k = 0;
            //foreach (var item in words)
            //{
            //    Console.WriteLine(k+1 + ": " + item.word + "->" + item.tag);
            //    k++;
            //}

            //var tags = SpeechPart.SpeechPartFrequence(words);
            //var sorted = from entry in tags orderby entry.Value descending select entry;
            //var sortedDict = new Dictionary<string, int>(sorted.ToDictionary(x => x.Key, x => x.Value));
            //WriteToTxtFile("Informations", "[new]List_Tags_Abstract.json", JsonConvert.SerializeObject(sortedDict));

            Console.WriteLine("Done with loading and creating tokens!");
            Tagger gTagger = new Tagger(words);
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
            //WriteToTxtFile("Trained Files", "SVM_trained_file.json", JsonConvert.SerializeObject(gTagger.Models));

            var textTest = LoadAndReadFolderFiles(BrownfolderTest);
            var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(textTest));
            var wordsTest = SpeechPart.GetNewHierarchicTags(oldWordsTest);

            //using (System.IO.StreamWriter file = new System.IO.StreamWriter("cuvinte_pt_testare.csv"))
            //{
            //    file.WriteLine("Word, Tag");
            //    foreach (var item in wordsTest)
            //        file.WriteLine(item.word + "," + item.tag);
            //}

            int wordsFound = 0;
            List<Tokenizer.WordTag> notFoundWords = new List<Tokenizer.WordTag>();
            List<string> algPredictions = new List<string>();
            foreach (var w in wordsTest)
            {
                Tagger.WordModel wordModelFinder = gTagger.Models.Find(x => x.Word == w.word);
                if (wordModelFinder == null)
                {
                    notFoundWords.Add(w);
                    algPredictions.Add("NULL");
                    if ("NN".Equals(w.tag))
                        wordsFound++;
                    continue;
                }
                string maxValueTag = wordModelFinder.TagFreq.OrderByDescending(x => x.Value).FirstOrDefault().Key;
                if (maxValueTag == null)
                {
                    if ("NN".Equals(w.tag))
                        wordsFound++;
                    notFoundWords.Add(w);
                    algPredictions.Add("NULL");
                    Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~entered here LOL");
                    continue;
                }
                if (maxValueTag.Equals(w.tag))
                    wordsFound++;
                else
                {
                    notFoundWords.Add(w);
                    algPredictions.Add(maxValueTag);
                }
            }

            Console.WriteLine("Accuracy: " + (float)wordsFound / wordsTest.Count);

            //using (System.IO.StreamWriter file = new System.IO.StreamWriter("cuvinte_nepredictionate.csv"))
            //{
            //    file.WriteLine("Word,My Prediction Tag,Actual Tag");
            //    for (int i = 0; i < notFoundWords.Count; i++)
            //    {
            //        file.WriteLine(notFoundWords[i].word + "," + algPredictions[i] + "," + notFoundWords[i].tag);
            //    }
            //}


        }
    }
}
