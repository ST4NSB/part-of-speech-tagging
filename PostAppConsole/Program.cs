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
            const string BrownfolderTrain = "Brown Corpus\\1_Train", BrownfolderTest = "Brown Corpus\\2_Test", demoFileTrain = "demo files\\train", demoFileTest = "demo files\\test";
            var text = LoadAndReadFolderFiles(demoFileTrain);
            var oldWords = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(text));

            var words = SpeechPart.GetNewHierarchicTags(oldWords);
            words = TextNormalization.Pipeline(words);


            Console.WriteLine("Done with loading and creating tokens!");
            Tagger tagger = new Tagger(words);
            Console.WriteLine("Done with training MODEL!");
            foreach (var model in tagger.EmissionFreq)
            {
                Console.WriteLine(model.Word);
                foreach (var item in model.TagFreq)
                {
                    Console.WriteLine("     " + item.Key + " -> " + item.Value);
                }
            }
            foreach(var item in tagger.UnigramFreq)
            {
                Console.WriteLine(item.Key + " -> " + item.Value);
            }
            foreach(var item in tagger.BigramTransition)
            {
                Console.WriteLine(item.Key + " -> " + item.Value);
            }
            
            Console.WriteLine("Duration of training model: " + tagger.GetTrainingTimeMs() + " ms!");
           // WriteToTxtFile("Trained Files", "SVM_trained_file.json", JsonConvert.SerializeObject(tagger.EmissionFreq));

            var textTest = LoadAndReadFolderFiles(demoFileTest);
            var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(textTest));
            var wordsTest = SpeechPart.GetNewHierarchicTags(oldWordsTest);
            wordsTest = TextNormalization.Pipeline(wordsTest);

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Decoder decoder = new Decoder(tagger.EmissionFreq, tagger.UnigramFreq, tagger.BigramTransition);
            decoder.CalculateProbabilitiesForTestFiles(wordsTest);
            foreach (var item in decoder.EmissionProbabilities)
            {
                Console.WriteLine(item.Word);
                foreach (var item2 in item.TagFreq)
                    Console.WriteLine("\t" + item2.Key + " -> " + item2.Value);
            }
            foreach (var item in decoder.BigramTransitionProbabilities)
                Console.WriteLine(item.Key + " -> " + item.Value);

            //int wordsFound = 0;
            //// List<Tokenizer.WordTag> notFoundWords = new List<Tokenizer.WordTag>();
            //List<string> algPredictions = new List<string>();
            //foreach (var w in wordsTest)
            //{
            //    Tagger.EmissionModel wordModelFinder = gTagger.EmissionFreq.Find(x => x.Word == w.word);
            //    if (wordModelFinder == null)
            //    {
            //        algPredictions.Add("NULL"); // NULL / NN
            //        //if ("NN".Equals(w.tag))
            //        //     wordsFound++;
            //        continue;
            //    }
            //    string maxValueTag = wordModelFinder.TagFreq.OrderByDescending(x => x.Value).FirstOrDefault().Key;
            //    if (maxValueTag.Equals(w.tag))
            //    {
            //        wordsFound++;
            //        algPredictions.Add(maxValueTag);
            //    }
            //    else
            //    {
            //        algPredictions.Add(maxValueTag);
            //    }
            //}

            //Console.WriteLine("Accuracy: " + (float)wordsFound / wordsTest.Count);

            ////using (System.IO.StreamWriter file = new System.IO.StreamWriter("cuvinte_nepredictionate.csv"))
            ////{
            ////    file.WriteLine("Word,My Prediction Tag,Actual Tag");
            ////    for (int i = 0; i < wordsTest.Count; i++)
            ////    {
            ////        file.WriteLine("\"" + wordsTest[i].word + "\"," + algPredictions[i] + "," + wordsTest[i].tag);
            ////    }
            ////}


            //Evaluation eval = new Evaluation();
            //eval.CreateSupervizedEvaluationsMatrix(wordsTest, algPredictions, fbeta:1);
            //Console.WriteLine("TAG       ACCURACY       PRECISION       RECALL       F-MEASURE");
            //var fullMatrix = eval.GetFullClassificationMatrix();
            //for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
            //{
            //    for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
            //        Console.Write(fullMatrix[i][j] + "       ");
            //    Console.WriteLine();
            //}
        }
    }
}
