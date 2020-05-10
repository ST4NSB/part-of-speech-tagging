using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NLP
{
    public partial class PartOfSpeechModel
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

        /// <summary>
        /// Public constructor to initialize the POSM.
        /// </summary>
        public PartOfSpeechModel()
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

        /// <summary>
        /// Public constructor to initialize the POSM with trained vectors.
        /// </summary>
        /// <param name="WordTagsEmissionProbabilities"></param>
        /// <param name="WordCapitalizedTagsEmissionProbabilities"></param>
        /// <param name="UnigramProbabilities"></param>
        /// <param name="BigramTransitionProbabilities"></param>
        /// <param name="TrigramTransitionProbabilities"></param>
        /// <param name="SuffixEmissionProbabilities"></param>
        /// <param name="PrefixEmissionProbabilities"></param>
        /// <param name="SuffixCapitalizedWordEmissionProbabilities"></param>
        /// <param name="PrefixCapitalizedWordEmissionProbabilities"></param>
        public PartOfSpeechModel(
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
        /// The Emission Model struct definition (Word - Dic[Tag, Tag_Frequency]), eg. (The, [at, 1]).
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

        /// <summary>
        /// The Emission Probabilistic Model struct definition (Word - Dic[Tag, Tag_Probability]), eg. (good, [jj, 0.85]).
        /// </summary>
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
        /// Calculates smoothing interpolation for BIGRAM.
        /// </summary>
        private void DeletedInterpolationBigram()
        {
            //if (this.TrigramTransitionProbabilities == null)
            //    this.TrigramTransitionProbabilities = new Dictionary<Tuple<string, string, string>, double>();

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

        /// <summary>
        /// Calculates smoothing interpolation for TRIGRAM.
        /// </summary>
        private void DeletedInterpolationTrigram()
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
    }
}