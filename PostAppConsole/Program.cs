#define RULE_70_30
#define CROSS_VALIDATION

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
#if (RULE_70_30)
            Console.WriteLine("You chose Rule 70% - training, 30% - testing for the data-set!");
            const string BrownfolderTrain = "Brown_Corpus\\70_30\\1_Train", BrownfolderTest = "Brown_Corpus\\70_30\\2_Test";
            const string demoFileTrain = "demo files\\train", demoFileTest = "demo files\\test";

            #region Load Train Files & pre-process data
            var text = LoadAndReadFolderFiles(BrownfolderTrain);
            var oldWords = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(text));
            var words = SpeechPart.GetNewHierarchicTags(oldWords);
            var capWords = TextNormalization.PreProcessingPipeline(words, toLowerOption: false, keepOnlyCapitalizedWords: true);
            var uncapWords = TextNormalization.PreProcessingPipeline(words, toLowerOption: true, keepOnlyCapitalizedWords: false);
            #endregion

            #region Load Test Files & pre-process data
            var textTest = LoadAndReadFolderFiles(BrownfolderTest);
            var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(textTest));
            var wordsTest = SpeechPart.GetNewHierarchicTags(oldWordsTest);
            wordsTest = TextNormalization.PreProcessingPipeline(wordsTest);
            #endregion

            Console.WriteLine("Done with loading and creating tokens for train & test files!");

            #region Hidden Markov Model Training
            HMMTagger tagger = new HMMTagger();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            tagger.CreateHiddenMarkovModel(uncapWords, capWords);

            wordsTest = tagger.EliminateDuplicateSequenceOfEndOfSentenceTags(wordsTest);
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

            Console.WriteLine("Done with training HIDDEN MARKOV MODEL & calculating probabilities! Time: " + sw.ElapsedMilliseconds + " ms");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            #region Decoding Viterbi Model
            Decoder decoder = new Decoder();

            sw.Reset(); sw.Start();
            decoder.ViterbiDecoding(tagger, wordsTest, modelForward: "trigram", modelBackward: "trigram", mode: "f+b");
            sw.Stop();
            tagger.EliminateAllEndOfSentenceTags(wordsTest);
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
            Console.WriteLine("TAG\t\tACCURACY\t\tPRECISION\t\tRECALL(TPR)\t\tF1-SCORE\t\tSPECIFICITY(TNR)");
            var fullMatrix = eval.PrintClassificationResultsMatrix();
            for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
            {
                for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
                    Console.Write(fullMatrix[i][j] + "\t\t");
                Console.WriteLine();
            }

            Console.WriteLine("\nAccuracy for known words: " + eval.GetHitRateAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k"));
            Console.WriteLine("Accuracy for unknown words: " + eval.GetHitRateAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "u"));
            Console.WriteLine("Accuracy on both: " + eval.GetHitRateAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k+u"));
            #endregion

            Console.WriteLine("+");

            #region Count known&unknown words
            int unkwordscount = 0, knownwordscount = 0;
            foreach(var item in wordsTest)
            {
                if (decoder.UnknownWords.Contains(item.word))
                    unkwordscount++;
                else knownwordscount++;
            }

            Console.WriteLine("Unknown words (count): " + unkwordscount + " | Procentage (%): " + (float)unkwordscount/wordsTest.Count);
            Console.WriteLine("Known words (count): " + knownwordscount + " | Procentage (%): " + (float)knownwordscount/wordsTest.Count);
            Console.WriteLine("Total words (count): " + wordsTest.Count);
            #endregion

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
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + "Informations\\" + "trigram_bidirectional.csv"))
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
            #endregion

#elif (CROSS_VALIDATION)
            const int folds = 4;
            const bool shuffle = true;
            Console.WriteLine("You chose Cross-Validation for the data-set! Folds: " + folds + ", Shuffle-option: " + shuffle);
            string BrownFolderPath = path + "Brown_Corpus\\full_brown";
            string demoBrown = path + "demo files\\cross";

            CrossValidation cv = new CrossValidation(filePath: BrownFolderPath, fold: folds, shuffle: shuffle); // with randomness
            Console.WriteLine("Done with loading dataset & splitting them into folds!\n");
            for(int foldNumber = 0; foldNumber < folds; foldNumber++)
            {
            #region Load Train Files & pre-process data
                var text = cv.TrainFile[foldNumber];
                var oldWords = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(text));
                var words = SpeechPart.GetNewHierarchicTags(oldWords);
                var capWords = TextNormalization.PreProcessingPipeline(words, toLowerOption: false, keepOnlyCapitalizedWords: true);
                var uncapWords = TextNormalization.PreProcessingPipeline(words, toLowerOption: true, keepOnlyCapitalizedWords: false);
            #endregion

            #region Load Test Files & pre-process data
                var textTest = cv.TestFile[foldNumber];
                var oldWordsTest = Tokenizer.SeparateTagFromWord(Tokenizer.WordTokenizeCorpus(textTest));
                var wordsTest = SpeechPart.GetNewHierarchicTags(oldWordsTest);
                wordsTest = TextNormalization.PreProcessingPipeline(wordsTest);
            #endregion

                Console.WriteLine("Done with loading and creating tokens for train & test files!");

            #region Hidden Markov Model Training
                HMMTagger tagger = new HMMTagger();

                Stopwatch sw = new Stopwatch();

                sw.Start();
                tagger.CreateHiddenMarkovModel(uncapWords, capWords);

                wordsTest = tagger.EliminateDuplicateSequenceOfEndOfSentenceTags(wordsTest);
                tagger.CalculateHiddenMarkovModelProbabilitiesForTestCorpus(wordsTest, model: "trigram");

                sw.Stop();
                Console.WriteLine("Done with training HIDDEN MARKOV MODEL & calculating probabilities! Time: " + sw.ElapsedMilliseconds + " ms");
                //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            #endregion


            #region Decoding Viterbi Model
                Decoder decoder = new Decoder();

                sw.Reset(); sw.Start();
                decoder.ViterbiDecoding(tagger, wordsTest, modelForward: "trigram", modelBackward: "trigram", mode: "f+b");
                sw.Stop();
                tagger.EliminateAllEndOfSentenceTags(wordsTest);

                Console.WriteLine("Done with DECODING VITERBI MODEL! Time: " + sw.ElapsedMilliseconds + " ms");
                //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            #endregion

            #region Evaluations & results
                Evaluation eval = new Evaluation();
                eval.CreateSupervizedEvaluationsMatrix(wordsTest, decoder.PredictedTags, decoder.UnknownWords, fbeta: 1);
                Console.WriteLine("TAG\t\tACCURACY\t\tPRECISION\t\tRECALL(TPR)\t\tF1-SCORE\t\tSPECIFICITY(TNR)");
                var fullMatrix = eval.PrintClassificationResultsMatrix();
                for (int i = 0; i < eval.GetFullMatrixLineLength(); i++)
                {
                    for (int j = 0; j < eval.GetFullMatrixColLength(); j++)
                        Console.Write(fullMatrix[i][j] + "\t\t");
                    Console.WriteLine();
                }

                Console.WriteLine("\nAccuracy for known words: " + eval.GetHitRateAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k"));
                Console.WriteLine("Accuracy for unknown words: " + eval.GetHitRateAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "u"));
                Console.WriteLine("Accuracy on both: " + eval.GetHitRateAccuracy(wordsTest, decoder.PredictedTags, decoder.UnknownWords, evalMode: "k+u"));
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

                Console.WriteLine("\n\n[FOLD " + (foldNumber + 1) + "/" + folds + " DONE!]\n\n");
            }

#endif
        }
    }
}
