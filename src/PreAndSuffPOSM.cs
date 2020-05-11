using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLP
{
    public partial class PartOfSpeechModel
    {
        /// <summary>
        /// Calculates emission for suffixes & prefixes.
        /// </summary>
        /// <param name="uncapitalizedWords"></param>
        /// <param name="capitalizedWords"></param>
        /// <param name="smoothing"></param>
        private void GetEmissionProbabilitiesForSuffixesAndPrefixes(List<Tokenizer.WordTag> uncapitalizedWords, List<Tokenizer.WordTag> capitalizedWords, int smoothing)
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
                                                    "penta", "tech"}; // starts with   

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
            CalculateSuffixPrefixProbabilities(capitalSuff, capitalPref, suffxem, preffxem, smoothing, pref.Count, suff.Count);
        }

        /// <summary>
        /// Calculates Emission Frequence for suffix & preffix training capitalized & uncapitalized words.
        /// </summary>
        /// <param name="uncapitalizedWords"></param>
        /// <param name="capitalizedWords"></param>
        /// <param name="pref"></param>
        /// <param name="suff"></param>
        /// <param name="capitalSuff"></param>
        /// <param name="capitalPref"></param>
        /// <param name="suffxem"></param>
        /// <param name="preffxem"></param>
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

        /// <summary>
        /// Calculates Emission Probability for suffix & preffix training capitalized & uncapitalized words.
        /// </summary>
        /// <param name="capitalSuff"></param>
        /// <param name="capitalPref"></param>
        /// <param name="suffxem"></param>
        /// <param name="preffxem"></param>
        /// <param name="smoothing"></param>
        /// <param name="prefSize"></param>
        /// <param name="suffSize"></param>
        private void CalculateSuffixPrefixProbabilities(
            List<EmissionModel> capitalSuff,
            List<EmissionModel> capitalPref,
            List<EmissionModel> suffxem,
            List<EmissionModel> preffxem,
            int smoothing,
            int prefSize,
            int suffSize)
        {
            foreach (var sfx in capitalSuff)
            {
                var tagSum = sfx.TagFreq.Sum(x => x.Value);
                Dictionary<string, double> tgfreq = new Dictionary<string, double>();
                foreach (var tg in sfx.TagFreq)
                {
                    tgfreq.Add(tg.Key, (double)(tg.Value + smoothing) / (tagSum + (smoothing * suffSize)));
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
                    tgfreq.Add(tg.Key, (double)(tg.Value + smoothing) / (tagSum + (smoothing * prefSize)));
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
                    tgfreq.Add(tg.Key, (double)(tg.Value + smoothing) / (tagSum + (smoothing * suffSize)));
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
                    tgfreq.Add(tg.Key, (double)(tg.Value + smoothing) / (tagSum + (smoothing * prefSize)));
                }

                var em = new EmissionProbabilisticModel();
                em.Word = pfx.Word;
                em.TagFreq = tgfreq;
                this.PrefixEmissionProbabilities.Add(em);
            }
        }
    }
}