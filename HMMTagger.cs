using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NLP
{
    public class HMMTagger
    {
        private int N;// nr of tokens

        public List<EmissionModel> EmissionFreq;
        public Dictionary<string, int> UnigramFreq = new Dictionary<string, int>();
        public Dictionary<Tuple<string, string>, int> BigramTransition;
        public Dictionary<Tuple<string, string, string>, int> TrigramTransition;

        public List<EmissionProbabilisticModel> EmissionProbabilities;
        public Dictionary<string, double> UnigramProbabilities;
        public Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities;
        public Dictionary<Tuple<string, string, string>, double> TrigramTransitionProbabilities;

        public List<EmissionProbabilisticModel> SuffixesEmission, PreffixEmission;

        private Stopwatch TrainingTime;

        public HMMTagger() { }

        public HMMTagger(
            List<EmissionModel> EmissionFreq,
            Dictionary<string, int> UnigramFreq,
            Dictionary<Tuple<string, string>, int> BigramTransition,
            Dictionary<Tuple<string, string, string>, int> TrigramTransition)
        {
            this.EmissionFreq = EmissionFreq;
            this.UnigramFreq = UnigramFreq;
            this.BigramTransition = BigramTransition;
            this.TrigramTransition = TrigramTransition;
        }

        /// <summary>
        /// The Model struct definition (Word - Dic[Tag, Tag_Frequency]), eg. (The, [at, 1]) 
        /// </summary>
        public class EmissionModel
        {
            public string Word;
            public Dictionary<string, int> TagFreq;
            public EmissionModel()
            {
                this.TagFreq = new Dictionary<string, int>();
            }
        }

        public class EmissionProbabilisticModel
        {
            public string Word;
            public Dictionary<string, double> TagFreq;
            public EmissionProbabilisticModel()
            {
                this.TagFreq = new Dictionary<string, double>();
            }
        }

        /// <summary>
        /// Function that creates the Emission & Transition Matrix
        /// </summary>
        /// <param name="wordsInput">List of words - tag, eg. The - at)</param>
        public void TrainModel(List<Tokenizer.WordTag> wordsInput)
        {
            this.TrainingTime = new Stopwatch();
            this.TrainingTime.Start();

            this.N = wordsInput.Count; // nr of tokens

            this.TrainSuffixPreffixEmission(wordsInput);

            this.CalculateEmissionAndTransitionOccurrences(wordsInput);
            this.CalculateBigramOccurences(wordsInput);
            this.CalculateTrigramOccurences(wordsInput);

            this.TrainingTime.Stop();
        }

        private void TrainSuffixPreffixEmission(List<Tokenizer.WordTag> words)
        {
            List<string> suff = new List<string>() { "able", "ible", "ade", "al", "an", "ance",
                                                    "ary", "ate", "cian", "cule", "cy", "dom",
                                                    "ee", "en","ence", "ency", "er", "ese", "ess",
                                                    "esis", "osis", "et", "ful", "fy", "ine", "ion",
                                                    "ish", "ism", "ist", "ity", "less", "ly", "ness",
                                                    "ous", "ent", "ize", "ing", "ive" }; // ends with
            List<string> preff = new List<string>() { "mis", "dis", "re", "anti", "in", "over" }; // starts with

            var suffxem = new List<EmissionModel>();
            var preffxem = new List<EmissionModel>();

            foreach (var item in suff)
            {
                var em = new EmissionModel();
                em.Word = item;
                suffxem.Add(em);
            }

            foreach (var item in preff)
            {
                var em = new EmissionModel();
                em.Word = item;
                preffxem.Add(em);
            }
        

            foreach (var w in words)
            {
                foreach(var sfx in suffxem)
                {
                    if (w.word.EndsWith(sfx.Word))
                    {
                        var tag = sfx.TagFreq.FirstOrDefault(x => x.Key == w.tag);
                        if (tag.Key == null)
                        {
                            sfx.TagFreq.Add(w.tag, 1);
                        }
                        else
                        {
                            sfx.TagFreq[tag.Key] += 1;
                        }
                    }
                }

                foreach(var pfx in preffxem)
                {
                    if (w.word.StartsWith(pfx.Word))
                    {
                        var tag = pfx.TagFreq.FirstOrDefault(x => x.Key == w.tag);
                        if (tag.Key == null)
                        {
                            pfx.TagFreq.Add(w.tag, 1);
                        }
                        else
                        {
                            pfx.TagFreq[tag.Key] += 1;
                        }
                    }
                }
            }

            this.SuffixesEmission = new List<EmissionProbabilisticModel>();
            this.PreffixEmission = new List<EmissionProbabilisticModel>();

            foreach(var sfx in suffxem)
            {
                var tagSum = sfx.TagFreq.Sum(x => x.Value);
                Dictionary<string, double> tgfreq = new Dictionary<string, double>();
                foreach (var tg in sfx.TagFreq)
                {
                    tgfreq.Add(tg.Key, (double)tg.Value / tagSum);
                }

                var em = new EmissionProbabilisticModel();
                em.Word = sfx.Word;
                em.TagFreq = tgfreq;
                this.SuffixesEmission.Add(em);
            }

            foreach (var pfx in preffxem)
            {
                var tagSum = pfx.TagFreq.Sum(x => x.Value);
                Dictionary<string, double> tgfreq = new Dictionary<string, double>();
                foreach (var tg in pfx.TagFreq)
                {
                    tgfreq.Add(tg.Key, (double)tg.Value / tagSum);
                }

                var em = new EmissionProbabilisticModel();
                em.Word = pfx.Word;
                em.TagFreq = tgfreq;
                this.PreffixEmission.Add(em);
            }

        }

        public List<Tokenizer.WordTag> EliminateDuplicateSequenceOfEndOfSentenceTags(List<Tokenizer.WordTag> testWords)
        {
            var results = new List<Tokenizer.WordTag>();
            foreach (var tw in testWords)
            {
                if (results.Count == 0)
                    results.Add(tw);
                else
                {
                    if (results.Last().tag == "." && tw.tag == ".")
                        continue;
                    results.Add(tw);
                }
            }
            return results;
        }

        public void EliminateAllEndOfSentenceTags(List<Tokenizer.WordTag> testWords)
        {
            testWords.RemoveAll(x => x.tag == ".");
        }

        public void CalculateProbabilitiesForTestFiles(List<Tokenizer.WordTag> testWords, string model = "bigram")
        {
            this.EmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.UnigramProbabilities = new Dictionary<string, double>();
            this.BigramTransitionProbabilities = new Dictionary<Tuple<string, string>, double>();

         
            // emission stage
            foreach (var tw in testWords)
            {
                string sWord = tw.word.ToLower();
                HMMTagger.EmissionModel wmFind = EmissionFreq.Find(x => x.Word == sWord);
                EmissionProbabilisticModel wFind = EmissionProbabilities.Find(x => x.Word == sWord);
                if (wmFind != null && wFind == null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach (var tf in wmFind.TagFreq)
                    {
                        int cti = this.UnigramFreq.FirstOrDefault(x => x.Key == tf.Key).Value;
                        float pwiti = (float)tf.Value / cti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti)
                        epModel.TagFreq.Add(tf.Key, pwiti);
                    }
                    this.EmissionProbabilities.Add(epModel);
                }
            }

            // transition stage
            // unigram
            foreach(var uni in this.UnigramFreq)
            {
                double pi = (double)uni.Value / this.N;
                this.UnigramProbabilities.Add(uni.Key, pi);
            }

            // bigram
            foreach (var bi in this.BigramTransition)
            {
                var cti = this.UnigramFreq.FirstOrDefault(x => x.Key.Equals(bi.Key.Item1)).Value;
                double pti = (double)bi.Value / cti; // Transition probability: p(ti|ti-1) = C(ti-1, ti) / C(ti-1)
                this.BigramTransitionProbabilities.Add(bi.Key, pti);

            }

            // trigram
            if (model.Equals("trigram"))
            {
                this.TrigramTransitionProbabilities = new Dictionary<Tuple<string, string, string>, double>();
                foreach (var tri in this.TrigramTransition)
                {
                    Tuple<string, string> tuple = new Tuple<string, string>(tri.Key.Item1, tri.Key.Item2);
                    var cti = this.BigramTransition.FirstOrDefault(x => x.Key.Equals(tuple)).Value;
                    double pti = (double)tri.Value / cti; // Transition probability: p(ti|ti-1, ti-2) = C(ti-2, ti-1, ti) / C(ti-2, ti-1)
                    this.TrigramTransitionProbabilities.Add(tri.Key, pti);
                }
            }            
        }

        public Tuple<double, double> DeletedInterpolationBigram()
        {
            if (this.TrigramTransitionProbabilities == null)
                this.TrigramTransitionProbabilities = new Dictionary<Tuple<string, string, string>, double>();

            int lambda1 = 0, lambda2 = 0;
            foreach (var bi in this.BigramTransition)
            {
                string unituple = bi.Key.Item2;

                double univalue = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(unituple)).Value;
                double bivalue = this.BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(bi.Key)).Value;

                if (bivalue < univalue)
                {
                    lambda1 += bi.Value;
                }
                else
                {
                    lambda2 += bi.Value;
                }
            }
            int sum = lambda1 + lambda2;
            return new Tuple<double, double>((double)lambda1 / sum, (double)lambda2 / sum);
        }

        public Tuple<double, double, double> DeletedInterpolationTrigram()
        {
            if (this.TrigramTransitionProbabilities == null)
                this.TrigramTransitionProbabilities = new Dictionary<Tuple<string, string, string>, double>();

            int lambda1 = 0, lambda2 = 0, lambda3 = 0;
            foreach (var tri in this.TrigramTransition)
            {
                string unituple = tri.Key.Item3;
                Tuple<string, string> bituple = new Tuple<string, string>(tri.Key.Item2, tri.Key.Item3);

                double univalue = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(unituple)).Value;
                double bivalue = this.BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(bituple)).Value;
                double trivalue = this.TrigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tri.Key)).Value;

                if(bivalue < univalue && univalue > trivalue)
                {
                    lambda1 += tri.Value;
                }
                else if(univalue < bivalue && bivalue > trivalue)
                {
                    lambda2 += tri.Value;
                }
                else
                {
                    lambda3 += tri.Value;
                }
            }
            int sum = lambda1 + lambda2 + lambda3;
            return new Tuple<double, double, double>((double)lambda1 / sum, (double)lambda2 / sum, (double)lambda3 / sum);
        }


        private void CalculateEmissionAndTransitionOccurrences(List<Tokenizer.WordTag> wordsInput)
        {
            this.EmissionFreq = new List<EmissionModel>();
            foreach (var w in wordsInput)
            {
                EmissionModel wmFind = EmissionFreq.Find(x => x.Word == w.word);
                if (wmFind == null)
                {
                    EmissionModel wModel = new EmissionModel();
                    wModel.Word = w.word;
                    wModel.TagFreq.Add(w.tag, 1);
                    this.AddTagToUnigramOccurences(w.tag);
                    this.EmissionFreq.Add(wModel);
                }
                else
                {
                    var tag = wmFind.TagFreq.FirstOrDefault(x => x.Key == w.tag);
                    if (tag.Key == null)
                    {
                        this.AddTagToUnigramOccurences(w.tag);
                        wmFind.TagFreq.Add(w.tag, 1);
                    }
                    else
                    {
                        this.AddTagToUnigramOccurences(w.tag);
                        wmFind.TagFreq[tag.Key] += 1;
                    }
                }
            }
        }

        private void AddTagToUnigramOccurences(string wordTag)
        {
            var tag = this.UnigramFreq.FirstOrDefault(x => x.Key == wordTag);
            if (tag.Key == null)
            {
                this.UnigramFreq.Add(wordTag, 1);
            }
            else
            {
                this.UnigramFreq[tag.Key] += 1;
            }
        }

        private void CalculateBigramOccurences(List<Tokenizer.WordTag> wordsInput)
        {
            this.BigramTransition = new Dictionary<Tuple<string, string>, int>();
            bool firstFileChecked = false;
            for (int i = -1; i < wordsInput.Count - 1; i++)
            {
                if (!firstFileChecked)
                {
                    this.BigramTransition.Add(new Tuple<string, string>(".", wordsInput[i + 1].tag), 1);
                    firstFileChecked = true;
                    continue;
                }

                var tuple = new Tuple<string, string>(wordsInput[i].tag, wordsInput[i + 1].tag);
                var tag = this.BigramTransition.FirstOrDefault(x => x.Key.Equals(tuple));
                if (tag.Key == null)
                {
                    this.BigramTransition.Add(tuple, 1);
                }
                else
                {
                    this.BigramTransition[tag.Key] += 1;
                }
            }
        }

        private void CalculateTrigramOccurences(List<Tokenizer.WordTag> wordsInput)
        {
            this.TrigramTransition = new Dictionary<Tuple<string, string, string>, int>();
            bool firstFileChecked = false;
            for (int i = -1; i < wordsInput.Count - 2; i++)
            {
                if (!firstFileChecked)
                {
                    this.TrigramTransition.Add(new Tuple<string, string, string>(".", wordsInput[i + 1].tag, wordsInput[i + 2].tag), 1);
                    firstFileChecked = true;
                    continue;
                }

                var tuple = new Tuple<string, string, string>(wordsInput[i].tag, wordsInput[i + 1].tag, wordsInput[i + 2].tag);

                if (tuple.Item2.Equals("."))
                    continue;
                
                var tag = this.TrigramTransition.FirstOrDefault(x => x.Key.Equals(tuple));
                if (tag.Key == null)
                {
                    this.TrigramTransition.Add(tuple, 1);
                }
                else
                {
                    this.TrigramTransition[tag.Key] += 1;
                }
            }
        }

        /// <summary>
        /// Method that returns the elapsed time of trained model(Emission + Transition) (ms)
        /// </summary>
        public long GetTrainingTimeMs()
        {
            return this.TrainingTime.ElapsedMilliseconds;
        }

    }
}