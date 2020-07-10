#define RULE_70_30
//#define CROSS_VALIDATION
//#define DEMO_APP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NLP;
using NUnit.Framework.Constraints;

#pragma warning disable CS0436

namespace PostAppConsole
{
    class Program
    {
        static ConsoleColor[] bkColor = new ConsoleColor[9] { ConsoleColor.DarkRed, ConsoleColor.Green, ConsoleColor.DarkYellow,
                ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Gray, ConsoleColor.DarkBlue, ConsoleColor.DarkGray };
        static ConsoleColor[] frColor = new ConsoleColor[9] { ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Black,
                ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Black, ConsoleColor.White, ConsoleColor.White };
        static string[] pos = new string[9] { "Noun", "Verb", "Pronoun", "Adjective", "Adverb", "Preposition",
            "Conjunction", "Article/Determiner", "Others" };

        private static void header()
        {
            Console.WriteLine("List of tags: ");
            for (int i = 0; i < 9; i++)
            {
                Console.ForegroundColor = frColor[i];
                Console.BackgroundColor = bkColor[i];
                Console.WriteLine("  " + pos[i] + "  ");

                Console.ResetColor();
            }
            Console.WriteLine();
        }

        private static void header(Dictionary<int, double> histogram)
        {
            Console.WriteLine("List of tags: ");
            for (int i = 0; i < 9; i++)
            {
                Console.ForegroundColor = frColor[i];
                Console.BackgroundColor = bkColor[i];
                Console.Write("  " + pos[i] + "  ");
                Console.ResetColor();

                if (histogram.ContainsKey(i))
                {
                    if (histogram[i] > 0.0d)
                        Console.Write(" " + histogram[i] + "%");
                }

                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void emptySpace()
        {
            Console.ResetColor();
            Console.Write(" ");
        }

        private static int getIndexForConversion(string tag)
        {
            switch (tag)
            {
                case "NN": return 0;
                case "VB": return 1;
                case "PN": return 2;
                case "JJ": return 3;
                case "RB": return 4;
                case "PP": return 5;
                case "CC": return 6;
                case "AT/DT": return 7;
                case "OT": return 8;
            }
            return -1;
        }

        private static string read()
        {
            string input;
            do
            {
                header();
                Console.Write("Enter your sentence here: ");
                input = Console.ReadLine();
                Console.Clear();
            } while (string.IsNullOrWhiteSpace(input));
            return input;
        }

        static string LoadAndReadFolderFiles(string folderName)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\" + folderName;
            Console.WriteLine("Read File Path: [" + path + "]");
            string text = FileReader.GetAllTextFromDirectoryAsString(path);
            return text;
        }

        static void WriteToTxtFile(string folderName, string fileName, string jsonFile)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\" + folderName + "\\" + fileName;
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
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\";

#if (RULE_70_30)
            Console.WriteLine("You chose Rule 70% - training, 30% - testing for the data-set!");
            const string BrownfolderTrain = "dataset\\70_30\\train", BrownfolderTest = "dataset\\70_30\\test";

            #region Load Train Files & pre-process data
            var text = LoadAndReadFolderFiles(BrownfolderTrain);
            var oldWords = Tokenizer.SeparateTagFromWord(Tokenizer.TokenizePennTreebank(text));
            var words = SpeechPartClassifier.GetNewHierarchicTags(oldWords);
            var capWords = TextPreprocessing.PreProcessingPipeline(words, toLowerOption: false, keepOnlyCapitalizedWords: true);
            var uncapWords = TextPreprocessing.PreProcessingPipeline(words, toLowerOption: true, keepOnlyCapitalizedWords: false);
            #endregion

            #region Load Test Files & pre-process data
            var textTest = LoadAndReadFolderFiles(BrownfolderTest);
            var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.TokenizePennTreebank(textTest));
            var wordsTest = SpeechPartClassifier.GetNewHierarchicTags(oldWordsTest);
            wordsTest = TextPreprocessing.PreProcessingPipeline(wordsTest);
            wordsTest = TextPreprocessing.Cleaning.EliminateDuplicateSequenceOfEndOfSentenceTags(wordsTest);
            #endregion

            Console.WriteLine("Done with loading and creating tokens for train & test files!");

            #region Part of Speech Model Training
            PartOfSpeechModel tagger = new PartOfSpeechModel();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            tagger.CreateHiddenMarkovModel(uncapWords, capWords, smoothingCoef: 1);
            tagger.CalculateHiddenMarkovModelProbabilitiesForTestCorpus(wordsTest, model: "trigram");
            sw.Stop();
            #endregion

            #region Debug for Emissions & Transitions matrix & write trained files
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

            //WriteToTxtFile("Models", "emissionWithCapital.json", JsonConvert.SerializeObject(tagger.CapitalEmissionFreq));
            //WriteToTxtFile("Models", "emission.json", JsonConvert.SerializeObject(tagger.EmissionFreq));
            //WriteToTxtFile("Models", "unigram.json", JsonConvert.SerializeObject(tagger.UnigramFreq));
            //WriteToTxtFile("Models", "bigram.json", JsonConvert.SerializeObject(tagger.BigramTransition));
            //WriteToTxtFile("Models", "trigram.json", JsonConvert.SerializeObject(tagger.TrigramTransition));
            //WriteToTxtFile("Models", "nonCapitalizedPrefix.json", JsonConvert.SerializeObject(tagger.PrefixEmissionProbabilities));
            //WriteToTxtFile("Models", "capitalizedPrefix.json", JsonConvert.SerializeObject(tagger.PrefixCapitalizedWordEmissionProbabilities));
            //WriteToTxtFile("Models", "nonCapitalizedSuffix.json", JsonConvert.SerializeObject(tagger.SuffixEmissionProbabilities));
            //WriteToTxtFile("Models", "capitalizedSuffix.json", JsonConvert.SerializeObject(tagger.SuffixCapitalizedWordEmissionProbabilities));
            //Console.WriteLine("Done writing models on filesystem!");
            #endregion

            Console.WriteLine("Done with training POS MODEL & calculating probabilities! Time: " + sw.ElapsedMilliseconds + " ms");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            #region Decoding Viterbi Model
            Decoder decoder = new Decoder();

            sw.Reset(); sw.Start();
            decoder.ViterbiDecoding(tagger, wordsTest, modelForward: "trigram", modelBackward: "trigram", mode: "backward", beam: 0);
            sw.Stop();
            #endregion

            Console.WriteLine("Done with DECODING VITERBI MODEL! Time: " + sw.ElapsedMilliseconds + " ms");

            #region Old method to guess probabilities
            //decoder.UnknownWords = new HashSet<string>();
            //decoder.PredictedTags = new List<string>();
            //foreach (var tw in wordsTest)
            //{
            //    HMMTagger.EmissionModel modelMax;
            //    modelMax = tagger.WordTagsEmissionFrequence.Find(x => x.Word == tw.word);

            //    if (modelMax != null)
            //    {
            //        string maxTag = modelMax.TagFreq.OrderByDescending(x => x.Value).FirstOrDefault().Key;

            //        // case default-tag NN ONLY
            //        //decoder.PredictedTags.Add("NN");

            //        // case maxTag
            //        decoder.PredictedTags.Add(maxTag);
            //    }
            //    else
            //    {
            //        const string deftag = "NN";
            //        decoder.PredictedTags.Add(deftag); // NULL / NN
            //        decoder.UnknownWords.Add(tw.word);
            //    }
            //}
            #endregion

            #region Debug for Emissions & Transitions



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
            #endregion

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            #region Evaluations & results
            Evaluation eval = new Evaluation();
            eval.CreateSupervizedEvaluationsMatrix(wordsTest, decoder.PredictedTags, decoder.UnknownWords, fbeta: 1);
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + "statistics\\" + "bdt.csv"))
            //{
            //    file.WriteLine("TAG,ACCURACY,PRECISION,RECALL(TPR),SPECIFICITY(TNR),F1-SCORE");
            //    var fullMatrix = eval.PrintClassificationResultsMatrix();
            //    for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
            //    {
            //        for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
            //            file.Write(fullMatrix[i][j] + ",");
            //        file.WriteLine();
            //    }
            //}
            Console.WriteLine("TAG ACCURACY PRECISION RECALL(TPR) SPECIFICITY(TNR) F1-SCORE");
            var fullMatrix = eval.PrintClassificationResultsMatrix();
            for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
            {
                for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
                    Console.Write(fullMatrix[i][j] + " ");
                Console.WriteLine();
            }

            Console.WriteLine("\nAccuracy for known words: " + eval.GetNaiveAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k"));
            Console.WriteLine("Accuracy for unknown words: " + eval.GetNaiveAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "u"));
            Console.WriteLine("Accuracy on both: " + eval.GetNaiveAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k+u"));
            #endregion

            Console.WriteLine("+");

            #region Count known&unknown words
            int unkwordscount = 0, knownwordscount = 0;
            foreach (var item in wordsTest)
            {
                if (decoder.UnknownWords.Contains(item.word))
                    unkwordscount++;
                else knownwordscount++;
            }

            Console.WriteLine("Unknown words (count): " + unkwordscount + " | Procentage (%): " + (float)unkwordscount / wordsTest.Count);
            Console.WriteLine("Known words (count): " + knownwordscount + " | Procentage (%): " + (float)knownwordscount / wordsTest.Count);
            Console.WriteLine("Total words (count): " + wordsTest.Count);
            #endregion


            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + "statistics\\" + "unknown_words.csv"))
            //{
            //    file.WriteLine("Unknown Words");
            //    foreach(var item in decoder.UnknownWords)
            //    {
            //        file.WriteLine("\"" + item + "\"");
            //    }
            //}

            #region Suffix & Prefix hitrate
            //List<string> suffixStr = new List<string>();
            //List<string> prefixStr = new List<string>();
            //List<Tuple<int, int>> suffixHR = new List<Tuple<int, int>>();
            //List<Tuple<int, int>> prefixHR = new List<Tuple<int, int>>();

            //foreach (var item in tagger.SuffixEmissionProbabilities)
            //{
            //    suffixStr.Add(item.Word);
            //    suffixHR.Add(new Tuple<int, int>(0, 0));
            //}
            //foreach (var item in tagger.PrefixEmissionProbabilities)
            //{
            //    prefixStr.Add(item.Word);
            //    prefixHR.Add(new Tuple<int, int>(0, 0));
            //}

            //for (int i = 0; i < wordsTest.Count; i++)
            //{
            //    if (!decoder.UnknownWords.Contains(wordsTest[i].word)) continue;
            //    for (int j = 0; j < suffixStr.Count; j++)
            //    {
            //        if (wordsTest[i].word.EndsWith(suffixStr[j]))
            //        {
            //            int hitr = suffixHR[j].Item1;
            //            int allr = suffixHR[j].Item2 + 1;
            //            if (wordsTest[i].tag == decoder.PredictedTags[i])
            //                suffixHR[j] = new Tuple<int, int>(hitr + 1, allr);
            //            else suffixHR[j] = new Tuple<int, int>(hitr, allr);
            //            break;
            //        }
            //    }

            //    for (int j = 0; j < prefixStr.Count; j++)
            //    {
            //        if (wordsTest[i].word.ToLower().StartsWith(prefixStr[j]))
            //        {
            //            int hitr = prefixHR[j].Item1;
            //            int allr = prefixHR[j].Item2 + 1;
            //            if (wordsTest[i].tag == decoder.PredictedTags[i])
            //                prefixHR[j] = new Tuple<int, int>(hitr + 1, allr);
            //            else prefixHR[j] = new Tuple<int, int>(hitr, allr);
            //            break;
            //        }
            //    }
            //}

            //Console.WriteLine("Prefixes: ");
            //for (int i = 0; i < prefixStr.Count; i++)
            //{
            //    Console.WriteLine(prefixStr[i] + ": (" + prefixHR[i].Item1 + ", " + prefixHR[i].Item2 + ") -> " + (float)prefixHR[i].Item1 / prefixHR[i].Item2);
            //}

            //Console.WriteLine("\nSuffixes: ");
            //for (int i = 0; i < suffixStr.Count; i++)
            //{
            //    Console.WriteLine(suffixStr[i] + ": (" + suffixHR[i].Item1 + ", " + suffixHR[i].Item2 + ") -> " + (float)suffixHR[i].Item1 / suffixHR[i].Item2);
            //}
            #endregion

            #region Save predictions tags to excel
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + "statistics\\" + "trigram_bidirectional.csv"))
            //{
            //    file.WriteLine("Word,Real Tag,Prediction Tag,Is in Train T/F,Predicted T/F");
            //    for (int i = 0; i < wordsTest.Count; i++)
            //    {
            //        bool isInTrain = true, predictedB = false;
            //        if (decoder.UnknownWords.Contains(wordsTest[i].word))
            //            isInTrain = false;
            //        if (wordsTest[i].tag == decoder.PredictedTags[i])
            //            predictedB = true;
            //        file.WriteLine("\"" + wordsTest[i].word + "\"," + wordsTest[i].tag + "," + decoder.PredictedTags[i] + "," + isInTrain + "," + predictedB);
            //    }
            //}
            #endregion

#elif (CROSS_VALIDATION)
            const int FOLDS = 4;
            const bool SHUFFLE = true;
            const string CVPATH = "dataset\\crossvalidation";
            Console.WriteLine("You chose Cross-Validation for the data-set! Folds: " + FOLDS + ", Shuffle-option: " + SHUFFLE);
            
            string BrownFolderPath = path + CVPATH;

            #region Part of Speech Tag Frequence Count
            //var tx = LoadAndReadFolderFiles("dataset\\crossvalidation");
            //var ow = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(tx));
            //var nw = SpeechPartClassification.GetNewHierarchicTags(ow);
            //var res = SpeechPartClassification.SpeechPartFrequence(nw);
            //foreach (var item in res)
            //    Console.WriteLine(item.Key + ": " + item.Value);
            #endregion

            List<float> knownacc = new List<float>(), unknownacc = new List<float>(), totalacc = new List<float>(), procentageunk = new List<float>();


            CrossValidation cv = new CrossValidation(filePath: BrownFolderPath, fold: FOLDS, shuffle: SHUFFLE); // with randomness
            Console.WriteLine("Done with loading dataset & splitting them into folds!\n");
            for(int foldNumber = 0; foldNumber < FOLDS; foldNumber++)
            {
            #region Load Train Files & pre-process data
                var text = cv.TrainFile[foldNumber];
                var oldWords = Tokenizer.SeparateTagFromWord(Tokenizer.TokenizePennTreebank(text));
                var words = SpeechPartClassifier.GetNewHierarchicTags(oldWords);
                var capWords = TextPreprocessing.PreProcessingPipeline(words, toLowerOption: false, keepOnlyCapitalizedWords: true);
                var uncapWords = TextPreprocessing.PreProcessingPipeline(words, toLowerOption: true, keepOnlyCapitalizedWords: false);
            #endregion

            #region Load Test Files & pre-process data
                var textTest = cv.TestFile[foldNumber];
                var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.TokenizePennTreebank(textTest));
                var wordsTest = SpeechPartClassifier.GetNewHierarchicTags(oldWordsTest);
                wordsTest = TextPreprocessing.PreProcessingPipeline(wordsTest);
                wordsTest = TextPreprocessing.Cleaning.EliminateDuplicateSequenceOfEndOfSentenceTags(wordsTest);
            #endregion

                Console.WriteLine("Done with loading and creating tokens for train & test files!");

            #region Hidden Markov Model Training
                PartOfSpeechModel tagger = new PartOfSpeechModel();

                Stopwatch sw = new Stopwatch();

                sw.Start();
                tagger.CreateHiddenMarkovModel(uncapWords, capWords);

                tagger.CalculateHiddenMarkovModelProbabilitiesForTestCorpus(wordsTest, model: "trigram");

                sw.Stop();
                Console.WriteLine("Done with training POS MODEL & calculating probabilities! Time: " + sw.ElapsedMilliseconds + " ms");
                //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            #endregion


            #region Decoding Viterbi Model
                Decoder decoder = new Decoder();

                sw.Reset(); sw.Start();
                decoder.ViterbiDecoding(tagger, wordsTest, modelForward: "trigram", modelBackward: "trigram", mode: "f+b");
                sw.Stop();

                Console.WriteLine("Done with DECODING VITERBI MODEL! Time: " + sw.ElapsedMilliseconds + " ms");
                //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            #endregion

            #region Evaluations & results
                Evaluation eval = new Evaluation();
                //eval.CreateSupervizedEvaluationsMatrix(wordsTest, decoder.PredictedTags, decoder.UnknownWords, fbeta: 1);
                //Console.WriteLine("TAG\t\tACCURACY\t\tPRECISION\t\tRECALL(TPR)\t\tF1-SCORE\t\tSPECIFICITY(TNR)");
                //var fullMatrix = eval.PrintClassificationResultsMatrix();
                //for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
                //{
                //    for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
                //        Console.Write(fullMatrix[i][j] + "\t\t");
                //    Console.WriteLine();
                //}

                var ka = eval.GetNaiveAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k");
                knownacc.Add(ka);
                var unkw = eval.GetNaiveAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "u");
                unknownacc.Add(unkw);
                var tot = eval.GetNaiveAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k+u");
                totalacc.Add(tot);


                Console.WriteLine("\nAccuracy for known words: " + ka);
                Console.WriteLine("Accuracy for unknown words: " + unkw);
                Console.WriteLine("Accuracy on both: " + tot);
            #endregion

                Console.WriteLine("+");

            #region Count known&unknown words
                int unkwordscount = 0, knownwordscount = 0;
                foreach (var item in wordsTest)
                {
                    if (decoder.UnknownWords.Contains(item.word))
                        unkwordscount++;
                    else knownwordscount++;
                }

                var proc = (float)unkwordscount / wordsTest.Count;
                procentageunk.Add(proc);

                Console.WriteLine("Unknown words (count): " + unkwordscount + " | Procentage (%): " + proc);
                Console.WriteLine("Known words (count): " + knownwordscount + " | Procentage (%): " + (float)knownwordscount / wordsTest.Count);
                Console.WriteLine("Total words (count): " + wordsTest.Count);
            #endregion

                Console.WriteLine("\n\n[FOLD " + (foldNumber + 1) + "/" + FOLDS + " DONE!]\n\n");
            }

            var known = (float)knownacc.Sum() / FOLDS;
            known = (float)Math.Round(known * 100, 3);
            var unk = (float)unknownacc.Sum() / FOLDS;
            unk = (float)Math.Round(unk * 100, 3);
            var total = (float)totalacc.Sum() / FOLDS;
            total = (float)Math.Round(total * 100, 3);
            var procunk = (float)procentageunk.Sum() / FOLDS;
            procunk = (float)Math.Round(procunk * 100, 3);

            Console.WriteLine("Procentage (%): " + procunk);
            Console.WriteLine("Accuracy for all unknown words: " + unk);
            Console.WriteLine("\nAccuracy for all known words: " + known);
            Console.WriteLine("Accuracy on all total: " + total);
            

#elif(DEMO_APP)
            #region Load & convert to model
            string modelsPath = path + "\\models\\";

            string unigram = File.ReadAllText(modelsPath + "unigram.json");
            string bigram = File.ReadAllText(modelsPath + "bigram.json");
            string trigram = File.ReadAllText(modelsPath + "trigram.json");
            string capitalizedPrefix = File.ReadAllText(modelsPath + "capitalizedPrefix.json");
            string nonCapitalizedPrefix = File.ReadAllText(modelsPath + "nonCapitalizedPrefix.json");
            string capitalizedSuffix = File.ReadAllText(modelsPath + "capitalizedSuffix.json");
            string nonCapitalizedSuffix = File.ReadAllText(modelsPath + "nonCapitalizedSuffix.json");
            string emission = File.ReadAllText(modelsPath + "emission.json");
            string emissionWithCapital = File.ReadAllText(modelsPath + "emissionWithCapital.json");

            var unigramFreq = JsonConvert.DeserializeObject<Dictionary<string, int>>(unigram);
            var bigramNonConverted = JsonConvert.DeserializeObject<Dictionary<string, int>>(bigram);
            var trigramNonConverted = JsonConvert.DeserializeObject<Dictionary<string, int>>(trigram);
            var capitalizedPrefixProb = JsonConvert.DeserializeObject<List<PartOfSpeechModel.EmissionProbabilisticModel>>(capitalizedPrefix);
            var nonCapitalizedPrefixProb = JsonConvert.DeserializeObject<List<PartOfSpeechModel.EmissionProbabilisticModel>>(nonCapitalizedPrefix);
            var capitalizedSuffixProb = JsonConvert.DeserializeObject<List<PartOfSpeechModel.EmissionProbabilisticModel>>(capitalizedSuffix);
            var nonCapitalizedSuffixProb = JsonConvert.DeserializeObject<List<PartOfSpeechModel.EmissionProbabilisticModel>>(nonCapitalizedSuffix);
            var emissionFreq = JsonConvert.DeserializeObject<List<PartOfSpeechModel.EmissionModel>>(emission);
            var emissionWithCapitalFreq = JsonConvert.DeserializeObject<List<PartOfSpeechModel.EmissionModel>>(emissionWithCapital);

            Dictionary<Tuple<string, string>, int> bigramFreq = new Dictionary<Tuple<string, string>, int>();
            Dictionary<Tuple<string, string, string>, int> trigramFreq = new Dictionary<Tuple<string, string, string>, int>();

            foreach (var item in bigramNonConverted)
            {
                string[] split = item.Key.Split(',');
                var charsToRemove = new string[] { "(", ")", " " };
                foreach (var c in charsToRemove)
                {
                    split[0] = split[0].Replace(c, string.Empty);
                    split[1] = split[1].Replace(c, string.Empty);
                }
                bigramFreq.Add(new Tuple<string, string>(split[0], split[1]), item.Value);
            }

            foreach (var item in trigramNonConverted)
            {
                string[] split = item.Key.Split(',');
                var charsToRemove = new string[] { "(", ")", " " };
                foreach (var c in charsToRemove)
                {
                    split[0] = split[0].Replace(c, string.Empty);
                    split[1] = split[1].Replace(c, string.Empty);
                    split[2] = split[2].Replace(c, string.Empty);
                }
                trigramFreq.Add(new Tuple<string, string, string>(split[0], split[1], split[2]), item.Value);
            }
            #endregion

            PartOfSpeechModel model = new PartOfSpeechModel(emissionFreq, emissionWithCapitalFreq, unigramFreq, bigramFreq, trigramFreq,
                nonCapitalizedSuffixProb, nonCapitalizedPrefixProb, capitalizedSuffixProb, capitalizedPrefixProb);
            NLP.Decoder decoder = new NLP.Decoder();

            string input = null;
            List<string> preprocessedInput;

            while (true)
            {
                do
                {
                    if (string.IsNullOrWhiteSpace(input))
                        input = read();
                    preprocessedInput = Tokenizer.TokenizeSentenceWords(input);
                    input = null;
                } while (preprocessedInput.Count == 0 || preprocessedInput[0] == string.Empty);

                preprocessedInput = TextPreprocessing.PreProcessingPipeline(preprocessedInput);
                model.CalculateHiddenMarkovModelProbabilitiesForTestCorpus(preprocessedInput, model: "trigram");

                List<Tokenizer.WordTag> inputTest = new List<Tokenizer.WordTag>();
                foreach (var item in preprocessedInput)
                {
                    if (item == "." || item == "!" || item == "?")
                        inputTest.Add(new Tokenizer.WordTag(item, "."));
                    else inputTest.Add(new Tokenizer.WordTag(item, ""));
                }
                if (inputTest[inputTest.Count - 1].tag != ".") // safe case check
                    inputTest.Add(new Tokenizer.WordTag(".", "."));

                decoder.ViterbiDecoding(model, inputTest, modelForward: "trigram", modelBackward: "trigram", mode: "f+b");

                Dictionary<string, int> histogram = new Dictionary<string, int>();
                Dictionary<int, double> freqHisto = new Dictionary<int, double>();
                foreach (var item in decoder.PredictedTags)
                    if (histogram.ContainsKey(item))
                        histogram[item] += 1;
                    else histogram.Add(item, 1);
                int sum = histogram.Sum(x => x.Value);
                foreach (var item in histogram)
                {
                    int index = getIndexForConversion(item.Key);
                    double val = Math.Round(((double)item.Value / sum) * 100.0d, 1);
                    freqHisto.Add(index, val);
                }

                header(freqHisto);
                Console.ResetColor();
                Console.Write("Tagged Sentence: ");

                for (int i = 0; i < decoder.PredictedTags.Count; i++)
                {
                    int index = getIndexForConversion(decoder.PredictedTags[i]);
                    Console.ForegroundColor = frColor[index];
                    Console.BackgroundColor = bkColor[index];
                    Console.Write(" " + inputTest[i].word + " ");
                    emptySpace();
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.Write("Enter your sentence here: ");
                input = Console.ReadLine();
                Console.Clear();                
            }
#endif
        }
    }
}
