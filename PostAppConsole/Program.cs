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
            HMMTagger tagger = new HMMTagger();
            tagger.TrainModel(words, model: "bigram");
            Console.WriteLine("Done with training MODEL!");
            foreach (var model in tagger.EmissionFreq)
            {
                Console.WriteLine(model.Word);
                foreach (var item in model.TagFreq)
                {
                    Console.WriteLine("     " + item.Key + " -> " + item.Value);
                }
            }
            foreach (var item in tagger.UnigramFreq)
                Console.WriteLine(item.Key + " -> " + item.Value);

            foreach (var item in tagger.BigramTransition)
                Console.WriteLine(item.Key + " -> " + item.Value);

            Console.WriteLine("Duration of training model: " + tagger.GetTrainingTimeMs() + " ms!");
            // WriteToTxtFile("Trained Files", "SVM_trained_file.json", JsonConvert.SerializeObject(tagger.EmissionFreq));

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            var textTest = LoadAndReadFolderFiles(demoFileTest);
            var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(textTest));
            var wordsTest = SpeechPart.GetNewHierarchicTags(oldWordsTest);
            wordsTest = TextNormalization.Pipeline(wordsTest);

            tagger.CalculateProbabilitiesForTestFiles(wordsTest, model: "bigram");

            Decoder decoder = new Decoder(tagger.EmissionProbabilities, tagger.BigramTransitionProbabilities);
            foreach (var item in decoder.EmissionProbabilities)
            {
                Console.WriteLine(item.Word);
                foreach (var item2 in item.TagFreq)
                    Console.WriteLine("\t" + item2.Key + " -> " + item2.Value);
            }
            foreach (var item in decoder.BigramTransitionProbabilities)
                Console.WriteLine(item.Key + " -> " + item.Value);


            decoder.ViterbiDecoding(wordsTest, model: "bigram", mode: "f+b");
            foreach (var line in decoder.ViterbiGraph)
            {
                foreach (var col in line)
                    Console.Write("[" + col.CurrentTag + " -> " + col.value + "]    ");
                Console.WriteLine();
            }

            foreach (var item in decoder.PredictedTags)
                Console.Write(item + " ");

            Console.WriteLine("\nDuration of Viterbi Decoding: " + decoder.GetViterbiDecodingTime() + " ms!\n");

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

           // foreach (var item in wordsTest)
           ///     Console.WriteLine(item.word + "  ");


            Evaluation eval = new Evaluation();
            eval.CreateSupervizedEvaluationsMatrix(wordsTest, decoder.PredictedTags, fbeta: 1);
            Console.WriteLine("Simple Accuracy: " + eval.GetSimpleAccuracy(wordsTest, decoder.PredictedTags));
            Console.WriteLine("TAG\tACCURACY\tPRECISION\t\tRECALL\t\tF-MEASURE");
            var fullMatrix = eval.GetFullClassificationMatrix();
            for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
            {
                for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
                    Console.Write(fullMatrix[i][j] + "\t\t");
                Console.WriteLine();
            }

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            ////using (System.IO.StreamWriter file = new System.IO.StreamWriter("cuvinte_nepredictionate.csv"))
            ////{
            ////    file.WriteLine("Word,My Prediction Tag,Actual Tag");
            ////    for (int i = 0; i < wordsTest.Count; i++)
            ////    {
            ////        file.WriteLine("\"" + wordsTest[i].word + "\"," + algPredictions[i] + "," + wordsTest[i].tag);
            ////    }
            ////}
        }
    }
}
