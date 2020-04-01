using System;
using System.Collections.Generic;
using System.Linq;

namespace NLP
{
    public class HMMTagger
    {
        private int N; // nr of tokens
        public List<EmissionModel> WordCapitalizedTagsEmissionFrequence;
        public List<EmissionModel> WordTagsEmissionFrequence;
        private Dictionary<string, int> UnigramFrequence = new Dictionary<string, int>();
        private Dictionary<Tuple<string, string>, int> BigramTransitionFrequence;
        private Dictionary<Tuple<string, string, string>, int> TrigramTransitionFrequence;

        public List<EmissionProbabilisticModel> WordCapitalizedTagsEmissionProbabilities;
        public List<EmissionProbabilisticModel> WordTagsEmissionProbabilities;
        public Dictionary<string, double> UnigramProbabilities;
        public Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities;
        public Dictionary<Tuple<string, string, string>, double> TrigramTransitionProbabilities;

        public List<EmissionProbabilisticModel> SuffixCapitalizedWordEmissionProbabilities, PrefixCapitalizedWordEmissionProbabilities;
        public List<EmissionProbabilisticModel> SuffixEmissionProbabilities, PrefixEmissionProbabilities;

        public double BgramLambda1, BgramLambda2, TgramLambda1, TgramLambda2, TgramLambda3;

        public HMMTagger()
        {
            this.WordCapitalizedTagsEmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.WordTagsEmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.UnigramProbabilities = new Dictionary<string, double>();
            this.BigramTransitionProbabilities = new Dictionary<Tuple<string, string>, double>();
            this.TrigramTransitionProbabilities = new Dictionary<Tuple<string, string, string>, double>();

            this.SuffixCapitalizedWordEmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.PrefixCapitalizedWordEmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.SuffixEmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.PrefixEmissionProbabilities = new List<EmissionProbabilisticModel>();
        }

        public HMMTagger(
            List<EmissionProbabilisticModel> WordTagsEmissionProbabilities,
            List<EmissionProbabilisticModel> WordCapitalizedTagsEmissionProbabilities,
            Dictionary<string, double> UnigramProbabilities,
            Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities,
            Dictionary<Tuple<string, string, string>, double> TrigramTransitionProbabilities,
            List<EmissionProbabilisticModel> SuffixEmissionProbabilities,
            List<EmissionProbabilisticModel> PrefixEmissionProbabilities,
            List<EmissionProbabilisticModel> SuffixCapitalizedWordEmissionProbabilities,
            List<EmissionProbabilisticModel> PrefixCapitalizedWordEmissionProbabilities)
        {
            this.WordCapitalizedTagsEmissionProbabilities = WordTagsEmissionProbabilities;
            this.WordCapitalizedTagsEmissionProbabilities = WordCapitalizedTagsEmissionProbabilities;
            this.UnigramProbabilities = UnigramProbabilities;
            this.BigramTransitionProbabilities = BigramTransitionProbabilities;
            this.TrigramTransitionProbabilities = TrigramTransitionProbabilities;

            this.SuffixEmissionProbabilities = SuffixEmissionProbabilities;
            this.PrefixEmissionProbabilities = PrefixEmissionProbabilities;
            this.SuffixCapitalizedWordEmissionProbabilities = SuffixCapitalizedWordEmissionProbabilities;
            this.PrefixCapitalizedWordEmissionProbabilities = PrefixCapitalizedWordEmissionProbabilities;
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

        public void CreateHiddenMarkovModel(List<Tokenizer.WordTag> uncapitalizedWords, List<Tokenizer.WordTag> capitalizedWords)
        {
            //foreach (var item in uncapitalizedWords)
            //    if (item.tag != ".")
            //        this.N++;

            this.N = uncapitalizedWords.Count;

            this.GetEmissionProbabilitiesForSuffixesAndPrefixes(uncapitalizedWords, capitalizedWords);

            this.CalculateEmissionForWordTags(uncapitalizedWords, capitalizedWords);
            this.CalculateBigramOccurences(uncapitalizedWords);
            this.CalculateTrigramOccurences(uncapitalizedWords);
        }

        private void GetEmissionProbabilitiesForSuffixesAndPrefixes(List<Tokenizer.WordTag> uncapitalizedWords, List<Tokenizer.WordTag> capitalizedWords)
        {
            // BlackList: -> prefix: neo[(0, 2) -> 0], over[(17, 45) -> 0.3777778], mega[(0, 2) -> 0]
            //                       eco[(1, 3) -> 0.3333333], dif[(1, 3) -> 0.3333333], post[post: (1, 3) -> 0.3333333],
            //                       exo[(0, 2) -> 0], contra[(1, 3) -> 0.3333333], quad[(1, 3) -> 0.3333333],
            //                       per[(12, 27) -> 0.4444444], sup[(0, 2) -> 0], sym[(1, 3) -> 0.3333333], up[(3, 7) -> 0.4285714]
            //            -> suffix: cule[(0, 2) -> 0], dom[dom: (0, 1) -> 0], ward[dom: (1, 3) -> 0.333333] 
            //                       less[(12, 29) -> 0.4137931], ize[(9, 23) -> 0.3913043], cy[(1, 3) -> 0.3333333], fy[(0, 1) -> 0]
             
            // list of prefixes & suffixes
            List<string> pref = new List<string>() { "inter", "intra", "mis", "mid", "mini", "dis", "di", "re", "anti", "in", "en", "em", "auto",
                                                    "il", "im", "ir", "ig", "non", "ob", "op", "octo", "oc", "pre", "pro", "under", "epi", "off", "on", "circum",
                                                    "multi", "bio", "bi", "mono", "demo", "de", "super", "supra", "cyber", "fore", "for", "para", "extra", "extro",
                                                    "ex", "hyper", "hypo", "hy", "sub","com", "counter", "con", "co", "semi", "vice", "poly", "trans",
                                                    "out", "step", "ben", "with", "an", "el", "ep", "geo", "iso", "meta", "ab", "ad", "ac", "as", "ante",
                                                    "pan", "ped", "peri", "socio", "sur", "syn", "sy", "tri", "uni", "un", "eu", "ecto",
                                                    "mal", "macro", "micro", "sus", "ultra", "omni", "prim", "sept", "se", "nano", "tera", "giga", "kilo", "cent", 
                                                    "penta"}; // starts with   

            List<string> suff = new List<string>() { "able", "ible", "ble", "ade", "cian", "ance", "ite", "genic", "phile", "ian", "ery", "ory",
                                                    "ary", "ate", "man", "an", "ency", "eon", "ex", "ix","acy", "escent", "tial", "cial", "al",
                                                    "ee", "en","ence", "ancy", "eer", "ier", "er", "or", "ar", "ium", "ous", "est", 
                                                    "ment", "ese", "ness", "ess", "ship", "ed", "ant", "ow", "land", "ure", "ity", 
                                                    "esis", "osis", "et", "ette", "ful", "ify", "ine", "sion", "fication", "tion", "ion",
                                                    "ish", "ism", "ist", "ty", "ly", "em", "fic", "olve", "ope",
                                                    "ent", "ise", "ling", "ing", "ive", "ic", "ways", "in", "ology",
                                                    "hood", "logy", "ice", "oid", "id", "ide", "age", "worthy", "ae", "es" }; // ends with   


            var capitalSuff = new List<EmissionModel>();
            var capitalPref = new List<EmissionModel>();
            var suffxem = new List<EmissionModel>();
            var preffxem = new List<EmissionModel>();

            CalculateSuffixPrefixFrequence(uncapitalizedWords, capitalizedWords, pref, suff, capitalSuff, capitalPref, suffxem, preffxem);

            CalculateSuffixPrefixProbabilities(capitalSuff, capitalPref, suffxem, preffxem);
        }

        private void CalculateSuffixPrefixFrequence(
            List<Tokenizer.WordTag> uncapitalizedWords, 
            List<Tokenizer.WordTag> capitalizedWords, 
            List<string> pref, 
            List<string> suff,
            List<EmissionModel> capitalSuff,
            List<EmissionModel> capitalPref, 
            List<EmissionModel> suffxem, 
            List<EmissionModel> preffxem)
        {
            foreach (var item in suff)
            {
                var emuw = new EmissionModel();
                var emcw = new EmissionModel();
                emuw.Word = item;
                emcw.Word = item;
                suffxem.Add(emuw);
                capitalSuff.Add(emcw);
            }

            foreach (var item in pref)
            {
                var emuwp = new EmissionModel();
                var emcwp = new EmissionModel();
                emuwp.Word = item;
                emcwp.Word = item;
                preffxem.Add(emuwp);
                capitalPref.Add(emcwp);
            }

            foreach (var w in capitalizedWords)
            {
                //if (!char.IsUpper(w.word[0])) continue;
                foreach (var sfx in capitalSuff)
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
                   // else
                    //{
                        //string singularWord = "";
                        //bool isPlural = false;
                        //if (w.word.EndsWith("s\'") || w.word.EndsWith("\'s")) //
                        //{
                        //    singularWord = w.word.Remove(w.word.Length - 2);
                        //    isPlural = true;
                        //}
                        //else if (w.word.EndsWith("s"))
                        //{
                        //    singularWord = w.word.Remove(w.word.Length - 1);
                        //    isPlural = true;
                        //}
                        //if (isPlural)
                        //{
                        //    if (singularWord.EndsWith(sfx.Word))
                        //    {
                        //        var tag = sfx.TagFreq.FirstOrDefault(x => x.Key == w.tag);
                        //        if (tag.Key == null)
                        //        {
                        //            sfx.TagFreq.Add(w.tag, 1);
                        //        }
                        //        else
                        //        {
                        //            sfx.TagFreq[tag.Key] += 1;
                        //        }
                        //    }
                        //}
                    //}
                }

                foreach (var pfx in capitalPref)
                {
                    string wordLow = w.word.ToLower();
                    if (wordLow.StartsWith(pfx.Word))
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

            foreach (var w in uncapitalizedWords)
            {
                foreach (var sfx in suffxem)
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
                    //else
                    //{
                        //string singularWord = "";
                        //bool isPlural = false;
                        //if (w.word.EndsWith("s\'") || w.word.EndsWith("\'s")) // 
                        //{
                        //    singularWord = w.word.Remove(w.word.Length - 2);
                        //    isPlural = true;
                        //}
                        //else if (w.word.EndsWith("s"))
                        //{
                        //    singularWord = w.word.Remove(w.word.Length - 1);
                        //    isPlural = true;
                        //}
                        //if (isPlural)
                        //{
                        //    if (singularWord.EndsWith(sfx.Word))
                        //    {
                        //        var tag = sfx.TagFreq.FirstOrDefault(x => x.Key == w.tag);
                        //        if (tag.Key == null)
                        //        {
                        //            sfx.TagFreq.Add(w.tag, 1);
                        //        }
                        //        else
                        //        {
                        //            sfx.TagFreq[tag.Key] += 1;
                        //        }
                        //    }
                        //}
                    //}
                }

                foreach (var pfx in preffxem)
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
        }

        private void CalculateSuffixPrefixProbabilities(
            List<EmissionModel> capitalSuff,
            List<EmissionModel> capitalPref,
            List<EmissionModel> suffxem,
            List<EmissionModel> preffxem)
        {
            foreach (var sfx in capitalSuff)
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
                this.SuffixCapitalizedWordEmissionProbabilities.Add(em);
            }

            foreach (var pfx in capitalPref)
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
                this.PrefixCapitalizedWordEmissionProbabilities.Add(em);
            }

            foreach (var sfx in suffxem)
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
                this.SuffixEmissionProbabilities.Add(em);
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
                this.PrefixEmissionProbabilities.Add(em);
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

        public void CalculateHiddenMarkovModelProbabilitiesForTestCorpus(List<Tokenizer.WordTag> testWords, string model = "bigram")
        {
            // emission stage
            foreach (var tw in testWords)
            {
                if (!char.IsUpper(tw.word[0])) continue;

                string sWord = tw.word;
                HMMTagger.EmissionModel wmFind = WordCapitalizedTagsEmissionFrequence.Find(x => x.Word == sWord);
                EmissionProbabilisticModel wFind = WordCapitalizedTagsEmissionProbabilities.Find(x => x.Word == sWord);
                if (wmFind != null && wFind == null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach (var tf in wmFind.TagFreq)
                    {
                        int cti = this.UnigramFrequence.FirstOrDefault(x => x.Key == tf.Key).Value;
                        float pwiti = (float)tf.Value / cti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti)
                        epModel.TagFreq.Add(tf.Key, pwiti);
                    }
                    this.WordCapitalizedTagsEmissionProbabilities.Add(epModel);
                }
            }

            foreach (var tw in testWords)
            {
                string sWord = tw.word.ToLower();

                HMMTagger.EmissionModel wmFind = WordTagsEmissionFrequence.Find(x => x.Word == sWord);
                EmissionProbabilisticModel wFind = WordTagsEmissionProbabilities.Find(x => x.Word == sWord);
                if (wmFind != null && wFind == null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach (var tf in wmFind.TagFreq)
                    {
                        int cti = this.UnigramFrequence.FirstOrDefault(x => x.Key == tf.Key).Value;
                        float pwiti = (float)tf.Value / cti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti)
                        epModel.TagFreq.Add(tf.Key, pwiti);
                    }
                    this.WordTagsEmissionProbabilities.Add(epModel);
                }
            }
            

            // transition stage
            // unigram
            foreach(var uni in this.UnigramFrequence)
            {
                double pi = (double)uni.Value / this.N;
                this.UnigramProbabilities.Add(uni.Key, pi);
            }

            // bigram
            foreach (var bi in this.BigramTransitionFrequence)
            {
                var cti = this.UnigramFrequence.FirstOrDefault(x => x.Key.Equals(bi.Key.Item1)).Value;
                double pti = (double)bi.Value / cti; // Transition probability: p(ti|ti-1) = C(ti-1, ti) / C(ti-1)
                this.BigramTransitionProbabilities.Add(bi.Key, pti);

            }

            // trigram
            if (model.Equals("trigram"))
            {
                foreach (var tri in this.TrigramTransitionFrequence)
                {
                    Tuple<string, string> tuple = new Tuple<string, string>(tri.Key.Item1, tri.Key.Item2);
                    var cti = this.BigramTransitionFrequence.FirstOrDefault(x => x.Key.Equals(tuple)).Value;
                    double pti = (double)tri.Value / cti; // Transition probability: p(ti|ti-1, ti-2) = C(ti-2, ti-1, ti) / C(ti-2, ti-1)
                    this.TrigramTransitionProbabilities.Add(tri.Key, pti);
                }
            }

            this.DeletedInterpolationBigram();
            this.DeletedInterpolationTrigram();
        }

        public void DeletedInterpolationBigram()
        {
            if (this.TrigramTransitionProbabilities == null)
                this.TrigramTransitionProbabilities = new Dictionary<Tuple<string, string, string>, double>();

            int lambda1 = 0, lambda2 = 0;
            foreach (var bi in this.BigramTransitionFrequence)
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

            this.BgramLambda1 = (double)lambda1 / sum;
            this.BgramLambda2 = (double)lambda2 / sum;
        }

        public void DeletedInterpolationTrigram()
        {
            if (this.TrigramTransitionProbabilities == null)
                this.TrigramTransitionProbabilities = new Dictionary<Tuple<string, string, string>, double>();

            int lambda1 = 0, lambda2 = 0, lambda3 = 0;
            foreach (var tri in this.TrigramTransitionFrequence)
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

            this.TgramLambda1 = (double)lambda1 / sum;
            this.TgramLambda2 = (double)lambda2 / sum;
            this.TgramLambda3 = (double)lambda3 / sum;
        }


        private void CalculateEmissionForWordTags(List<Tokenizer.WordTag> uncapitalizedWords, List<Tokenizer.WordTag> capitalizedWords)
        {
            this.WordCapitalizedTagsEmissionFrequence = new List<EmissionModel>();
            this.WordTagsEmissionFrequence = new List<EmissionModel>();

            foreach (var w in capitalizedWords)
            {
                //if (!char.IsUpper(w.word[0])) continue; // ignore words that don't start with capitalized letter

                EmissionModel wmFind = WordCapitalizedTagsEmissionFrequence.Find(x => x.Word == w.word);
                if (wmFind == null)
                {
                    EmissionModel wModel = new EmissionModel();
                    wModel.Word = w.word;
                    wModel.TagFreq.Add(w.tag, 1);
                    this.WordCapitalizedTagsEmissionFrequence.Add(wModel);
                }
                else
                {
                    var tag = wmFind.TagFreq.FirstOrDefault(x => x.Key == w.tag);
                    if (tag.Key == null)
                    {
                        wmFind.TagFreq.Add(w.tag, 1);
                    }
                    else
                    {
                        wmFind.TagFreq[tag.Key] += 1;
                    }
                }
            }


            foreach (var w in uncapitalizedWords)
            {
                EmissionModel wmFind = WordTagsEmissionFrequence.Find(x => x.Word == w.word);
                if (wmFind == null)
                {
                    EmissionModel wModel = new EmissionModel();
                    wModel.Word = w.word;
                    wModel.TagFreq.Add(w.tag, 1);
                    this.AddTagToUnigramOccurences(w.tag);
                    this.WordTagsEmissionFrequence.Add(wModel);
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
            var tag = this.UnigramFrequence.FirstOrDefault(x => x.Key == wordTag);
            if (tag.Key == null)
            {
                this.UnigramFrequence.Add(wordTag, 1);
            }
            else
            {
                this.UnigramFrequence[tag.Key] += 1;
            }
        }

        private void CalculateBigramOccurences(List<Tokenizer.WordTag> wordsInput)
        {
            this.BigramTransitionFrequence = new Dictionary<Tuple<string, string>, int>();
            bool firstFileChecked = false;
            for (int i = -1; i < wordsInput.Count - 1; i++)
            {
                if (!firstFileChecked)
                {
                    this.BigramTransitionFrequence.Add(new Tuple<string, string>(".", wordsInput[i + 1].tag), 1);
                    firstFileChecked = true;
                    continue;
                }

                var tuple = new Tuple<string, string>(wordsInput[i].tag, wordsInput[i + 1].tag);
                var tag = this.BigramTransitionFrequence.FirstOrDefault(x => x.Key.Equals(tuple));
                if (tag.Key == null)
                {
                    this.BigramTransitionFrequence.Add(tuple, 1);
                }
                else
                {
                    this.BigramTransitionFrequence[tag.Key] += 1;
                }
            }
        }

        private void CalculateTrigramOccurences(List<Tokenizer.WordTag> wordsInput)
        {
            this.TrigramTransitionFrequence = new Dictionary<Tuple<string, string, string>, int>();
            bool firstFileChecked = false;
            for (int i = -1; i < wordsInput.Count - 2; i++)
            {
                if (!firstFileChecked)
                {
                    this.TrigramTransitionFrequence.Add(new Tuple<string, string, string>(".", wordsInput[i + 1].tag, wordsInput[i + 2].tag), 1);
                    firstFileChecked = true;
                    continue;
                }

                var tuple = new Tuple<string, string, string>(wordsInput[i].tag, wordsInput[i + 1].tag, wordsInput[i + 2].tag);

                if (tuple.Item2.Equals("."))
                    continue;
                
                var tag = this.TrigramTransitionFrequence.FirstOrDefault(x => x.Key.Equals(tuple));
                if (tag.Key == null)
                {
                    this.TrigramTransitionFrequence.Add(tuple, 1);
                }
                else
                {
                    this.TrigramTransitionFrequence[tag.Key] += 1;
                }
            }
        }

    }
}