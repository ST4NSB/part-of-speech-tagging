using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\";
            string BrownFolderPath = path + "Brown Corpus\\brown";
            const int fold = 4;

            const string BrownfolderTrain = "Brown Corpus\\Rule 70-30\\1_Train", BrownfolderTest = "Brown Corpus\\Rule 70-30\\2_Test";
            const string demoFileTrain = "demo files\\train", demoFileTest = "demo files\\test";
            string demoBrown = path + "demo files\\cross";
           

            var text = LoadAndReadFolderFiles(BrownfolderTrain);
            var oldWords = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(text));
            var words = SpeechPart.GetNewHierarchicTags(oldWords);
            var capWords = TextNormalization.PreProcessingPipeline(words, toLowerTxt: false);
            words = TextNormalization.PreProcessingPipeline(words, toLowerTxt: true);

            
            Console.WriteLine("Done with loading and creating tokens!");

            HMMTagger tagger = new HMMTagger();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            tagger.CreateHiddenMarkovModel(words, capWords);
            

            //foreach (var model in tagger.EmissionFreq)
            //{
            //    Console.WriteLine(model.Word);
            //    foreach (var item in model.TagFreq)
            //    {
            //        Console.WriteLine("     " + item.Key + " -> " + item.Value);
            //    }
            //}
            //foreach (var item in tagger.UnigramFreq)
            //    Console.WriteLine(item.Key + " -> " + item.Value);
            //foreach (var item in tagger.BigramTransition)
            //    Console.WriteLine(item.Key + " -> " + item.Value);
            //foreach (var item in tagger.TrigramTransition)
            //    Console.WriteLine(item.Key + " -> " + item.Value);

            //WriteToTxtFile("Trained Files", "emissionWithCapital.json", JsonConvert.SerializeObject(tagger.CapitalEmissionFreq));
            //WriteToTxtFile("Trained Files", "emission.json", JsonConvert.SerializeObject(tagger.EmissionFreq));
            // WriteToTxtFile("Trained Files", "unigram.json", JsonConvert.SerializeObject(tagger.UnigramFreq));
            //WriteToTxtFile("Trained Files", "bigram.json", JsonConvert.SerializeObject(tagger.BigramTransition));
            //WriteToTxtFile("Trained Files", "trigram.json", JsonConvert.SerializeObject(tagger.TrigramTransition));


            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            var textTest = LoadAndReadFolderFiles(BrownfolderTest);

            var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(textTest));
            var wordsTest = SpeechPart.GetNewHierarchicTags(oldWordsTest);
            wordsTest = TextNormalization.PreProcessingPipeline(wordsTest, toLowerTxt: false);


            wordsTest = tagger.EliminateDuplicateSequenceOfEndOfSentenceTags(wordsTest);
            tagger.CalculateHiddenMarkovModelProbabilitiesForTestCorpus(wordsTest, model: "trigram");

            sw.Stop();
            Console.WriteLine("Done with training HIDDEN MARKOV MODEL & loading test files! Time: " + sw.ElapsedMilliseconds + " ms");

            Decoder decoder = new Decoder();

            sw.Reset();
            sw.Start();
            decoder.ViterbiDecoding(tagger, wordsTest, modelForward: "trigram", modelBackward: "trigram", mode: "f+b");
            sw.Stop();
            tagger.EliminateAllEndOfSentenceTags(wordsTest);
            Console.WriteLine("Done with VITERBI DECODING MODEL! Time: " + sw.ElapsedMilliseconds + " ms");


            //Decoder decoder = new Decoder();
            //decoder.UnknownWords = new HashSet<string>();
            //const string deftag = "NN";
            //decoder.PredictedTags = new List<string>();
            //foreach (var tw in wordsTest)
            //{
            //    var modelMax = tagger.EmissionFreq.Find(x => x.Word == tw.word);
            //    if (modelMax != null)
            //    {
            //        string maxTag = modelMax.TagFreq.OrderByDescending(x => x.Value).FirstOrDefault().Key;
            //        decoder.PredictedTags.Add(deftag);
            //    }
            //    else
            //    {
            //        decoder.PredictedTags.Add(deftag); // NULL / NN
            //        decoder.UnknownWords.Add(tw.word);
            //    }
            //}

            //foreach (var item in decoder.EmissionProbabilities)
            //{
            //    Console.WriteLine(item.Word);
            //    foreach (var item2 in item.TagFreq)
            //        Console.WriteLine("\t" + item2.Key + " -> " + item2.Value);
            //}
            //foreach (var item in decoder.UnigramProbabilities)
            //    Console.WriteLine("UNI: " + item.Key + "->" + item.Value);
            //foreach (var item in decoder.BigramTransitionProbabilities)
            //    Console.WriteLine("BI: " + item.Key + " -> " + item.Value);
            //foreach (var item in decoder.TrigramTransitionProbabilities)
            //    Console.WriteLine("TRI: " + item.Key + " -> " + item.Value);

            //foreach (var item in decoder.ViterbiGraph)
            //{
            //    foreach (var item2 in item)
            //        Console.Write(item2.CurrentTag + ":" + item2.value + "    ");
            //    Console.WriteLine();
            //}

            //Console.WriteLine("Predicted tags: ");
            //foreach (var item in decoder.PredictedTags)
            //    Console.Write(item + " ");


            Console.WriteLine("testwords: " + wordsTest.Count + " , predwords: " + decoder.PredictedTags.Count);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            Evaluation eval = new Evaluation();
            eval.CreateSupervizedEvaluationsMatrix(wordsTest, decoder.PredictedTags, decoder.UnknownWords, fbeta: 1);
            Console.WriteLine("TAG\t\tACCURACY\t\tPRECISION\t\tRECALL\t\t\tF1-SCORE");
            var fullMatrix = eval.GetFullClassificationMatrix();
            for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
            {
                for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
                    Console.Write(fullMatrix[i][j] + "\t\t");
                Console.WriteLine();
            }

            Console.WriteLine("\nAccuracy for known words: " + eval.GetSimpleAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k"));
            Console.WriteLine("Accuracy for unknown words: " + eval.GetSimpleAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "u"));
            Console.WriteLine("Accuracy on both: " + eval.GetSimpleAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k+u"));

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");



            using (System.IO.StreamWriter file = new System.IO.StreamWriter("trigram_bidirectional.csv"))
            {
                file.WriteLine("Word,Real Tag,Prediction Tag,Is in Train T/F,Predicted T/F");
                for (int i = 0; i < wordsTest.Count; i++)
                {
                    bool isInTrain = true, predictedB = false;
                    if (decoder.UnknownWords.Contains(wordsTest[i].word))
                        isInTrain = false;
                    if (wordsTest[i].tag == decoder.PredictedTags[i])
                        predictedB = true;
                    file.WriteLine("\"" + wordsTest[i].word + "\"," + wordsTest[i].tag + "," + decoder.PredictedTags[i] + "," + isInTrain + "," + predictedB);
                }
            }

        }
    }
}
