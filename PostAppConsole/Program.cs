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
            words = TextNormalization.Pipeline(words);
            var stem = new PorterStemmer();

            var stemWords = new List<Tokenizer.WordTag>();
            foreach(var item in words)
                stemWords.Add(new Tokenizer.WordTag(stem.StemWord(item.word), item.tag));


            Console.WriteLine("Done with loading and creating tokens!");
            Tagger gTagger = new Tagger(stemWords);
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
            WriteToTxtFile("Trained Files", "SVM_trained_file.json", JsonConvert.SerializeObject(gTagger.Models));

            var textTest = LoadAndReadFolderFiles(BrownfolderTest);
            var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(textTest));
            var wordsTest = SpeechPart.GetNewHierarchicTags(oldWordsTest);
            wordsTest = TextNormalization.Pipeline(wordsTest);

            var stemWordsTest = new List<Tokenizer.WordTag>();
            foreach (var item in wordsTest)
                stemWordsTest.Add(new Tokenizer.WordTag(stem.StemWord(item.word), item.tag));



            int wordsFound = 0;
            // List<Tokenizer.WordTag> notFoundWords = new List<Tokenizer.WordTag>();
            List<string> algPredictions = new List<string>();
            foreach (var w in stemWordsTest)
            {
                Tagger.WordModel wordModelFinder = gTagger.Models.Find(x => x.Word == w.word);
                if (wordModelFinder == null)
                {
                    algPredictions.Add("NULL"); // NULL / NN
                    //if ("NN".Equals(w.tag))
                    //     wordsFound++;
                    continue;
                }
                string maxValueTag = wordModelFinder.TagFreq.OrderByDescending(x => x.Value).FirstOrDefault().Key;
                if (maxValueTag.Equals(w.tag))
                {
                    wordsFound++;
                    algPredictions.Add(maxValueTag);
                }
                else
                {
                    algPredictions.Add(maxValueTag);
                }
            }

            Console.WriteLine("Accuracy: " + (float)wordsFound / stemWordsTest.Count);

            //using (System.IO.StreamWriter file = new System.IO.StreamWriter("cuvinte_nepredictionate.csv"))
            //{
            //    file.WriteLine("Word,My Prediction Tag,Actual Tag");
            //    for (int i = 0; i < wordsTest.Count; i++)
            //    {
            //        file.WriteLine("\"" + wordsTest[i].word + "\"," + algPredictions[i] + "," + wordsTest[i].tag);
            //    }
            //}

            Evaluation eval = new Evaluation();
            eval.CreateSupervizedEvaluationsMatrix(wordsTest, algPredictions, fbeta:1);
            Console.WriteLine("TAG       ACCURACY       PRECISION       RECALL       F-MEASURE");
            var fullMatrix = eval.GetFullClassificationMatrix();
            for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
            {
                for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
                    Console.Write(fullMatrix[i][j] + "       ");
                Console.WriteLine();
            }
        }
    }
}
