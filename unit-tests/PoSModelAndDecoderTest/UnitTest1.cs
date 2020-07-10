using NUnit.Framework;
using NLP;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.Numerics;
using System.Linq;

namespace PartOfSpeechModelTest
{
    public class HiddenMarkovModelMainExampleTest
    {
        PartOfSpeechModel tagger;

        List<Tokenizer.WordTag> testw = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("Nolan", "NN"),
                new Tokenizer.WordTag("will", "MD"),
                new Tokenizer.WordTag("tip", "VB"),
                new Tokenizer.WordTag("Will", "NN"),
                new Tokenizer.WordTag(".", "."),
            };


        [SetUp]
        public void Setup()
        {
            List<Tokenizer.WordTag> uncap = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("cristopher", "NN"),
                new Tokenizer.WordTag("nolan", "NN"),
                new Tokenizer.WordTag("can", "MD"),
                new Tokenizer.WordTag("hire", "VB"),
                new Tokenizer.WordTag("will", "NN"),
                new Tokenizer.WordTag(".", "."),
                new Tokenizer.WordTag("tip", "NN"),
                new Tokenizer.WordTag("will", "MD"),
                new Tokenizer.WordTag("hire", "VB"),
                new Tokenizer.WordTag("cristopher", "NN"),
                new Tokenizer.WordTag(".", "."),
                new Tokenizer.WordTag("will", "MD"),
                new Tokenizer.WordTag("nolan", "NN"),
                new Tokenizer.WordTag("tip", "VB"),
                new Tokenizer.WordTag("cristopher", "NN"),
                new Tokenizer.WordTag("?", "."),
                new Tokenizer.WordTag("cristopher", "NN"),
                new Tokenizer.WordTag("will", "MD"),
                new Tokenizer.WordTag("pay", "VB"),
                new Tokenizer.WordTag("tip", "NN"),
                new Tokenizer.WordTag(".", "."),
            };

            List<Tokenizer.WordTag> cap = new List<Tokenizer.WordTag>()
                {
                new Tokenizer.WordTag("Cristopher", "NN"),
                new Tokenizer.WordTag("Nolan", "NN"),
                new Tokenizer.WordTag("Will", "NN"),
                new Tokenizer.WordTag("Tip", "NN"),
                new Tokenizer.WordTag("Cristopher", "NN"),
                new Tokenizer.WordTag("Will", "MD"),
                new Tokenizer.WordTag("Nolan", "NN"),
                new Tokenizer.WordTag("Cristopher", "NN"),
                new Tokenizer.WordTag("Cristopher", "NN"),
                new Tokenizer.WordTag("Tip", "NN"),
            };


            tagger = new PartOfSpeechModel();
            tagger.CreateHiddenMarkovModel(uncap, cap);
            tagger.CalculateHiddenMarkovModelProbabilitiesForTestCorpus(testw, model: "trigram");
        }

        [Test]
        public void EmissionFreqTest()
        {
            List<PartOfSpeechModel.EmissionModel> uncap = new List<PartOfSpeechModel.EmissionModel>()
            {
                new PartOfSpeechModel.EmissionModel("cristopher", new Dictionary<string, int>() { { "NN", 4 } }),
                new PartOfSpeechModel.EmissionModel("nolan", new Dictionary<string, int>() { { "NN", 2 } }),
                new PartOfSpeechModel.EmissionModel("can", new Dictionary<string, int>() { { "MD", 1 } }),
                new PartOfSpeechModel.EmissionModel("hire", new Dictionary<string, int>() { { "VB", 2 } }),
                new PartOfSpeechModel.EmissionModel("will", new Dictionary<string, int>() { { "NN", 1 }, { "MD", 3 } }),
                new PartOfSpeechModel.EmissionModel(".", new Dictionary<string, int>() { { ".", 3 } }),
                new PartOfSpeechModel.EmissionModel("tip", new Dictionary<string, int>() { { "NN", 2 }, { "VB", 1 } }),
                new PartOfSpeechModel.EmissionModel("?", new Dictionary<string, int>() { { ".", 1 } }),
                new PartOfSpeechModel.EmissionModel("pay", new Dictionary<string, int>() { { "VB", 1 } }),
            };

            List<PartOfSpeechModel.EmissionModel> capped = new List<PartOfSpeechModel.EmissionModel>()
            {
                new PartOfSpeechModel.EmissionModel("Cristopher", new Dictionary<string, int>() { { "NN", 4 } }),
                new PartOfSpeechModel.EmissionModel("Nolan", new Dictionary<string, int>() { { "NN", 2 } }),
                new PartOfSpeechModel.EmissionModel("Will", new Dictionary<string, int>() { { "NN", 1 }, { "MD", 1 } }),
                new PartOfSpeechModel.EmissionModel("Tip", new Dictionary<string, int>() { { "NN", 2 } }),
            };

            string res = "";
            foreach (var item in tagger.WordTagsEmissionFrequence)
            {
                res += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    res += "" + i.Key + "-" + i.Value + "\n";
            }
            string exp = "";
            foreach (var item in uncap)
            {
                exp += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    exp += "" + i.Key + "-" + i.Value + "\n";
            }

            string rescapped = "";
            foreach (var item in tagger.WordCapitalizedTagsEmissionFrequence)
            {
                rescapped += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    rescapped += "" + i.Key + "-" + i.Value + "\n";
            }
            string expcapped = "";
            foreach (var item in capped)
            {
                expcapped += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    expcapped += "" + i.Key + "-" + i.Value + "\n";
            }

            Assert.AreEqual(exp, res);
            Assert.AreEqual(expcapped, rescapped);
        }

        [Test]
        public void EmissionProbTest()
        {
            List<PartOfSpeechModel.EmissionProbabilisticModel> uncapped = new List<PartOfSpeechModel.EmissionProbabilisticModel>()
            {
                new PartOfSpeechModel.EmissionProbabilisticModel("nolan", new Dictionary<string, double>() { { "NN", (double)2/9 } }),
                new PartOfSpeechModel.EmissionProbabilisticModel("will", new Dictionary<string, double>() { { "NN", (double)1 /9 }, { "MD", (double)3 / 4 } }),
                new PartOfSpeechModel.EmissionProbabilisticModel("tip", new Dictionary<string, double>() { { "NN", (double)2 /9 }, { "VB", (double)1 / 4 } }),
                new PartOfSpeechModel.EmissionProbabilisticModel(".", new Dictionary<string, double>() { { ".", (double)3 /4 } }),
            };

            List<PartOfSpeechModel.EmissionProbabilisticModel> capped = new List<PartOfSpeechModel.EmissionProbabilisticModel>()
            {
                new PartOfSpeechModel.EmissionProbabilisticModel("Nolan", new Dictionary<string, double>() { { "NN", (double)2/9 } }),
                new PartOfSpeechModel.EmissionProbabilisticModel("Will", new Dictionary<string, double>() { { "NN", (double)1/9 }, { "MD", (double)1/4 } }),
            };

            string res = "";
            foreach (var item in tagger.WordTagsEmissionProbabilities)
            {
                res += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    res += "" + i.Key + "-" + i.Value + "\n";
            }
            string exp = "";
            foreach (var item in uncapped)
            {
                exp += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    exp += "" + i.Key + "-" + i.Value + "\n";
            }

            string rescapped = "";
            foreach (var item in tagger.WordCapitalizedTagsEmissionProbabilities)
            {
                rescapped += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    rescapped += "" + i.Key + "-" + i.Value + "\n";
            }
            string expcapped = "";
            foreach (var item in capped)
            {
                expcapped += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    expcapped += "" + i.Key + "-" + i.Value + "\n";
            }

            Assert.AreEqual(exp, res);
            Assert.AreEqual(expcapped, rescapped);
        }

        [Test]
        public void UnigramTest()
        {
            int n = 21; // nr of words
            Dictionary<string, double> input = new Dictionary<string, double>()
            {
                { "NN", (double)9 / n },
                { "MD", (double)4 / n },
                { "VB", (double)4 / n },
                { ".", (double)4 / n },
            };

            Assert.AreEqual(input, tagger.UnigramProbabilities);
        }

        [Test]
        public void BigramTest()
        {
            Dictionary<Tuple<string, string>, double> input = new Dictionary<Tuple<string, string>, double>()
            {
                { new Tuple<string, string>(".", "NN"), (double)3 / 4 },
                { new Tuple<string, string>("NN", "NN"), (double)1 / 9 },
                { new Tuple<string, string>("NN", "MD"), (double)3 / 9 },
                { new Tuple<string, string>("MD", "VB"), (double)3 / 4 },
                { new Tuple<string, string>("VB", "NN"), (double)4 / 4 },
                { new Tuple<string, string>("NN", "."), (double)4 / 9 },
                { new Tuple<string, string>(".", "MD"), (double)1 / 4 },
                { new Tuple<string, string>("MD", "NN"), (double)1 / 4 },
                { new Tuple<string, string>("NN", "VB"), (double)1 / 9 },
            };

            Assert.AreEqual(input, tagger.BigramTransitionProbabilities);
        }

        [Test]
        public void TrigramTest()
        {
            Dictionary<Tuple<string, string, string>, double> input = new Dictionary<Tuple<string, string, string>, double>()
            {
                { new Tuple<string, string, string>(".", "NN", "NN"), (double)1 / 3 },
                { new Tuple<string, string, string>("NN", "NN", "MD"), (double)1 / 1 },
                { new Tuple<string, string, string>("NN", "MD", "VB"), (double)3 / 3 },
                { new Tuple<string, string, string>("MD", "VB", "NN"), (double)3 / 3 },
                { new Tuple<string, string, string>("VB", "NN", "."), (double)4 / 4 },
                { new Tuple<string, string, string>(".", "NN", "MD"), (double)2 / 3 },
                { new Tuple<string, string, string>(".", "MD", "NN"), (double)1 / 1 },
                { new Tuple<string, string, string>("MD", "NN", "VB"), (double)1 / 1 },
                { new Tuple<string, string, string>("NN", "VB", "NN"), (double)1 / 1 },
            };

            Assert.AreEqual(input, tagger.TrigramTransitionProbabilities);
        }

        [Test]
        public void DeletedInterpolationBigram()
        {
            int lam1 = 1 + 1 + 1;
            int lam2 = 3 + 3 + 3 + 4 + 4 + 1;

            int sum = lam1 + lam2;

            var res = new Tuple<double, double>(tagger.BgramLambda1, tagger.BgramLambda2);
            var exp = new Tuple<double, double>((double)lam1 / sum, (double)lam2 / sum);
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void DeletedInterpolationTrigram()
        {
            int lam1 = 1;
            int lam2 = 0;
            int lam3 = 1 + 3 + 3 + 4 + 2 + 1 + 1 + 1;

            int sum = lam1 + lam2 + lam3;

            var res = new Tuple<double, double, double>(tagger.TgramLambda1, tagger.TgramLambda2, tagger.TgramLambda3);
            var exp = new Tuple<double, double, double>((double)lam1 / sum, (double)lam2 / sum, (double)lam3 / sum);
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void ViterbiGraphForwardConsoleExample()
        {
            Decoder decoder = new Decoder();
            decoder.ViterbiDecoding(tagger, testw, modelForward: "trigram", modelBackward: "trigram", mode: "forward");
            foreach (var item in decoder.ViterbiGraph)
            {
                foreach (var item2 in item)
                {
                    Console.Write(item2.CurrentTag + " - " + item2.value + "           ");
                }
                Console.WriteLine();
            }

            double expected = 0.008449166144314171d;
            double result = decoder.ViterbiGraph[decoder.ViterbiGraph.Count - 1][0].value;

            string exptag = "NN";
            string restag = decoder.ViterbiGraph[decoder.ViterbiGraph.Count - 1][0].CurrentTag;
            Assert.AreEqual(expected, result);
            Assert.AreEqual(exptag, restag);
        }

        [Test]
        public void ViterbiGraphBackwardConsoleExample()
        {
            Decoder decoder = new Decoder();
            decoder.ViterbiDecoding(tagger, testw, modelForward: "trigram", modelBackward: "trigram", mode: "backward");
            foreach(var item in decoder.ViterbiGraph)
            {
                foreach(var item2 in item)
                {
                    Console.Write(item2.CurrentTag + " - " + item2.value + "           ");
                }
                Console.WriteLine();
            }

            double expected = 0.014995894543048682d;
            double result = decoder.ViterbiGraph[decoder.ViterbiGraph.Count - 1][0].value;

            string exptag = "NN";
            string restag = decoder.ViterbiGraph[decoder.ViterbiGraph.Count - 1][0].CurrentTag;
            Assert.AreEqual(expected, result);
            Assert.AreEqual(exptag, restag);
        }

        [Test]
        public void ViterbiTrigramBidirectionalDecodingSequence()
        {
            Decoder decoder = new Decoder();
            List<string> expected = new List<string>() { "NN", "MD", "VB", "NN" };
            decoder.ViterbiDecoding(tagger, testw, modelForward: "trigram", modelBackward: "trigram", mode: "f+b");
            var predicted = decoder.PredictedTags;
            Assert.AreEqual(expected, predicted);
        }
    }

    public class HiddenMarkovModelUnknownWordsExampleTest
    {
        PartOfSpeechModel tagger;

        List<Tokenizer.WordTag> testw = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("nano--tech", "NN"),
                new Tokenizer.WordTag("nano-tech", "NN"),
                new Tokenizer.WordTag("Epoch-tech", "NN"),
                new Tokenizer.WordTag("lovely", "JJ"),
                new Tokenizer.WordTag("test_Successfully", "NN"),
                new Tokenizer.WordTag("Epoch", "NN"),
                new Tokenizer.WordTag("unknown", "NN"),
                new Tokenizer.WordTag("unknown", "NN"),
                new Tokenizer.WordTag("unknown", "NN"),
                new Tokenizer.WordTag("nano--tech", "NN"),
                new Tokenizer.WordTag("testy", "NN"),
                new Tokenizer.WordTag("semingly", "NN"),
                new Tokenizer.WordTag("semingly", "NN"),
            };


        [SetUp]
        public void Setup()
        {
            List<Tokenizer.WordTag> uncap = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("nano-tech", "NN"),
                new Tokenizer.WordTag("nano-tech2", "VB"),
                new Tokenizer.WordTag("lovely", "JJ"),
                new Tokenizer.WordTag("tested", "VB"),
                new Tokenizer.WordTag("semingly", "NN"),
                new Tokenizer.WordTag("testly", "RB"),
            };

            List<Tokenizer.WordTag> cap = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("Northeasterly", "NN"),
                new Tokenizer.WordTag("Epoch", "NN"),
                new Tokenizer.WordTag("Epilog", "NN"),
            };

            tagger = new PartOfSpeechModel();
            tagger.CreateHiddenMarkovModel(uncap, cap, smoothingCoef: 1);
            tagger.CalculateHiddenMarkovModelProbabilitiesForTestCorpus(testw, model: "bigram");
        }

        [Test]
        public void PrefixListTest()
        {
            int n = 99; // prefix count
            List<PartOfSpeechModel.EmissionProbabilisticModel> uncapped = new List<PartOfSpeechModel.EmissionProbabilisticModel>
            {
                new PartOfSpeechModel.EmissionProbabilisticModel("semi", new Dictionary<string, double>() { {"NN", (double)(1+1)/ (1 + n)} } ),
                new PartOfSpeechModel.EmissionProbabilisticModel("se", new Dictionary<string, double>() { {"NN", (double)(1+1)/ (1 + n)} } ),
                new PartOfSpeechModel.EmissionProbabilisticModel("nano", new Dictionary<string, double>() { {"NN", (double)(1+1)/ (2 + n)}, { "VB", (double)(1 + 1) / (2 + 99) } }),
            };

            List<PartOfSpeechModel.EmissionProbabilisticModel> capped = new List<PartOfSpeechModel.EmissionProbabilisticModel>
            {
                new PartOfSpeechModel.EmissionProbabilisticModel("epi", new Dictionary<string, double>() { {"NN", (double)(1+1)/ (1 + n)} } ),
                new PartOfSpeechModel.EmissionProbabilisticModel("ep", new Dictionary<string, double>() { {"NN", (double)(2+1)/ (2 + n)} }),
            };
            

            string res = "";
            foreach (var item in tagger.PrefixEmissionProbabilities)
            {
                if (item.TagFreq.Count <= 0)
                    continue;
                res += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    res += "" + i.Key + "-" + i.Value + "\n";
            }
            string exp = "";
            foreach (var item in uncapped)
            {
                exp += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    exp += "" + i.Key + "-" + i.Value + "\n";
            }

            string rescap = "";
            foreach (var item in tagger.PrefixCapitalizedWordEmissionProbabilities)
            {
                if (item.TagFreq.Count <= 0)
                    continue;
                res += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    res += "" + i.Key + "-" + i.Value + "\n";
            }
            string expcap = "";
            foreach (var item in capped)
            {
                exp += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    exp += "" + i.Key + "-" + i.Value + "\n";
            }

            Assert.AreEqual(exp, res);
            Assert.AreEqual(expcap, rescap);
        }

        [Test]
        public void SuffixListTest()
        {
            int n = 87; // suffix count
            List<PartOfSpeechModel.EmissionProbabilisticModel> uncapped = new List<PartOfSpeechModel.EmissionProbabilisticModel>
            {
                new PartOfSpeechModel.EmissionProbabilisticModel("ed", new Dictionary<string, double>() { {"VB", (double)(1+1)/ (1 + n)} } ),
                new PartOfSpeechModel.EmissionProbabilisticModel("ly", new Dictionary<string, double>() { { "JJ", (double)(1 + 1) / (3 + n) }, { "NN", (double)(1+1)/ (3 + n)}, { "RB", (double)(1+1)/ (3 + n)}  }),
            };

            List<PartOfSpeechModel.EmissionProbabilisticModel> capped = new List<PartOfSpeechModel.EmissionProbabilisticModel>
            {
                new PartOfSpeechModel.EmissionProbabilisticModel("ly", new Dictionary<string, double>() { {"NN", (double)(1+1)/ (1 + n)} } ),
            };

            string res = "";
            foreach (var item in tagger.SuffixEmissionProbabilities)
            {
                if (item.TagFreq.Count <= 0)
                    continue;
                res += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    res += "" + i.Key + "-" + i.Value + "\n";
            }
            string exp = "";
            foreach (var item in uncapped)
            {
                exp += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    exp += "" + i.Key + "-" + i.Value + "\n";
            }

            string rescap = "";
            foreach (var item in tagger.SuffixCapitalizedWordEmissionProbabilities)
            {
                if (item.TagFreq.Count <= 0)
                    continue;
                res += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    res += "" + i.Key + "-" + i.Value + "\n";
            }
            string expcap = "";
            foreach (var item in capped)
            {
                exp += item.Word + "->\n";
                foreach (var i in item.TagFreq)
                    exp += "" + i.Key + "-" + i.Value + "\n";
            }

            Assert.AreEqual(exp, res);
            Assert.AreEqual(expcap, rescap);
        }

        [Test]
        public void DecoderUnknownWordsHashSetTest()
        {
            Decoder decoder = new Decoder();
            HashSet<string> expected = new HashSet<string>() { "nano--tech", "Epoch-tech", "test_Successfully", "unknown", "testy" };
            decoder.ViterbiDecoding(tagger, testw, modelForward: "bigram", modelBackward: "bigram", mode: "forward");
            var res = decoder.UnknownWords;
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void UnknownWordsWeightTest()
        {
            Console.WriteLine("Romania (NN): " + tagger.GetValueWeightForUnknownWord("Romania", "NN"));
            Console.WriteLine("Romania (JJ): " + tagger.GetValueWeightForUnknownWord("Romania", "JJ"));
            Console.WriteLine("Romania (VB): " + tagger.GetValueWeightForUnknownWord("Romania", "VB"));
            Console.WriteLine("enemies (NN): " + tagger.GetValueWeightForUnknownWord("enemies", "NN"));
            Console.WriteLine("enemies (JJ): " + tagger.GetValueWeightForUnknownWord("enemies", "JJ"));
            Console.WriteLine("Michael's (NN): " + tagger.GetValueWeightForUnknownWord("Michael's", "NN"));
            Console.WriteLine("Michael's (VB): " + tagger.GetValueWeightForUnknownWord("Michael's", "VB"));
            Console.WriteLine();
            Console.WriteLine("I've (NN): " + tagger.GetValueWeightForUnknownWord("I've", "NN"));
            Console.WriteLine("I've (PN): " + tagger.GetValueWeightForUnknownWord("I've", "PN"));
            Console.WriteLine("he'll (NN): " + tagger.GetValueWeightForUnknownWord("he'll", "NN"));
            Console.WriteLine("he'll (PN): " + tagger.GetValueWeightForUnknownWord("he'll", "PN"));
            Console.WriteLine();
            Console.WriteLine("lovely (NN): " + tagger.GetValueWeightForUnknownWord("lovely", "NN"));
            Console.WriteLine("lovely (JJ): " + tagger.GetValueWeightForUnknownWord("lovely", "JJ"));
            Console.WriteLine("lovely (VB): " + tagger.GetValueWeightForUnknownWord("lovely", "VB"));
            Console.WriteLine("lovely (RB): " + tagger.GetValueWeightForUnknownWord("lovely", "RB"));
            Console.WriteLine();
            Console.WriteLine("provoked (NN): " + tagger.GetValueWeightForUnknownWord("provoked", "NN"));
            Console.WriteLine("provoked (JJ): " + tagger.GetValueWeightForUnknownWord("provoked", "JJ"));
            Console.WriteLine("provoked (VB): " + tagger.GetValueWeightForUnknownWord("provoked", "VB"));
            Console.WriteLine("provoked (RB): " + tagger.GetValueWeightForUnknownWord("provoked", "RB"));
            Console.WriteLine();
            Console.WriteLine("Oh-the-pain-of-it (NN): " + tagger.GetValueWeightForUnknownWord("Oh-the-pain-of-it", "NN"));
            Console.WriteLine("Oh-the-pain-of-it (OT): " + tagger.GetValueWeightForUnknownWord("Oh-the-pain-of-it", "OT"));
            Console.WriteLine("Oh-the-pain-of-it (RB): " + tagger.GetValueWeightForUnknownWord("Oh-the-pain-of-it", "RB"));
            Console.WriteLine("oh-the-pain-of-it (NN): " + tagger.GetValueWeightForUnknownWord("oh-the-pain-of-it", "NN"));
            Assert.Pass();
        }
    }
}