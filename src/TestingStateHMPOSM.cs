using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP
{
    public partial class PartOfSpeechModel
    {
        /// <summary>
        /// Calculates & assigns HMM network probabilities just to the test words.
        /// </summary>
        /// <param name="testWords"></param>
        /// <param name="model"></param>
        public void CalculateHiddenMarkovModelProbabilitiesForTestCorpus(List<Tokenizer.WordTag> testWords, string model = "bigram")
        {
            // emission stage
            Task taskEmission = Task.Factory.StartNew(() => this.calculateEmissionTestCorpus(testWords));

            // transition stage
            // unigram
            Task taskUnigram = Task.Factory.StartNew(() => this.calculateUnigramTestCorpus());

            // bigram
            Task taskBigram = Task.Factory.StartNew(() => this.calculateBigramTestCorpus());


            if (model.Equals("trigram")) // trigram
            {
                Task taskTrigram = Task.Factory.StartNew(() => this.calculateTrigramTestCorpus());
                Task.WaitAll(taskEmission, taskUnigram, taskBigram, taskTrigram);

                Task taskBiInterp = Task.Factory.StartNew(() => this.DeletedInterpolationBigram());
                Task taskTriInterp = Task.Factory.StartNew(() => this.DeletedInterpolationTrigram());
                Task.WaitAll(taskBiInterp, taskTriInterp);
            }
            else
            {
                Task.WaitAll(taskEmission, taskUnigram, taskBigram);
                this.DeletedInterpolationBigram();
            }
        }

        /// <summary>
        /// Calculates & assigns HMM network probabilities just to the test words. (version 2 for list of strings)
        /// </summary>
        /// <param name="testWords"></param>
        /// <param name="model"></param>
        public void CalculateHiddenMarkovModelProbabilitiesForTestCorpus(List<string> testWords, string model = "bigram")
        {
            // emission stage
            this.calculateEmissionTestCorpus(testWords);

            // transition stage
            // unigram
            if (UnigramProbabilities.Count == 0)
            {
                Task taskUnigram = Task.Factory.StartNew(() => this.calculateUnigramTestCorpus());

                // bigram
                Task taskBigram = Task.Factory.StartNew(() => this.calculateBigramTestCorpus());


                if (model.Equals("trigram")) // trigram
                {
                    Task taskTrigram = Task.Factory.StartNew(() => this.calculateTrigramTestCorpus());
                    Task.WaitAll(taskUnigram, taskBigram, taskTrigram);

                    Task taskBiInterp = Task.Factory.StartNew(() => this.DeletedInterpolationBigram());
                    Task taskTriInterp = Task.Factory.StartNew(() => this.DeletedInterpolationTrigram());
                    Task.WaitAll(taskBiInterp, taskTriInterp);
                }
                else
                {
                    Task.WaitAll(taskUnigram, taskBigram);
                    this.DeletedInterpolationBigram();
                }
            }
        }

        private void calculateEmissionTestCorpus(List<Tokenizer.WordTag> testWords)
        {
            foreach (var tw in testWords)
            {
                if (!char.IsUpper(tw.word[0])) continue;

                string sWord = tw.word;
                PartOfSpeechModel.EmissionModel wmFind = WordCapitalizedTagsEmissionFrequence.Find(x => x.Word == sWord);
                EmissionProbabilisticModel wFind = WordCapitalizedTagsEmissionProbabilities.Find(x => x.Word == sWord);
                if (wmFind != null && wFind == null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach (var tf in wmFind.TagFreq)
                    {
                        int cti = this.UnigramFrequence.FirstOrDefault(x => x.Key == tf.Key).Value;
                        double pwiti = (double)tf.Value / cti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti) 
                        epModel.TagFreq.Add(tf.Key, pwiti);
                    }
                    this.WordCapitalizedTagsEmissionProbabilities.Add(epModel);
                }
            }

            foreach (var tw in testWords)
            {
                string sWord = tw.word.ToLower();

                PartOfSpeechModel.EmissionModel wmFind = WordTagsEmissionFrequence.Find(x => x.Word == sWord);
                EmissionProbabilisticModel wFind = WordTagsEmissionProbabilities.Find(x => x.Word == sWord);
                if (wmFind != null && wFind == null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach (var tf in wmFind.TagFreq)
                    {
                        int cti = this.UnigramFrequence.FirstOrDefault(x => x.Key == tf.Key).Value;
                        double pwiti = (double)tf.Value / cti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti)
                        epModel.TagFreq.Add(tf.Key, pwiti);
                    }
                    this.WordTagsEmissionProbabilities.Add(epModel);
                }
            }
        }

        private void calculateEmissionTestCorpus(List<string> testWords)
        {
            foreach (var tw in testWords)
            {
                if (!char.IsUpper(tw[0])) continue;

                string sWord = tw;
                PartOfSpeechModel.EmissionModel wmFind = WordCapitalizedTagsEmissionFrequence.Find(x => x.Word == sWord);
                EmissionProbabilisticModel wFind = WordCapitalizedTagsEmissionProbabilities.Find(x => x.Word == sWord);
                if (wmFind != null && wFind == null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach (var tf in wmFind.TagFreq)
                    {
                        int cti = this.UnigramFrequence.FirstOrDefault(x => x.Key == tf.Key).Value;
                        double pwiti = (double)tf.Value / cti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti) 
                        epModel.TagFreq.Add(tf.Key, pwiti);
                    }
                    this.WordCapitalizedTagsEmissionProbabilities.Add(epModel);
                }
            }

            foreach (var tw in testWords)
            {
                string sWord = tw.ToLower();

                PartOfSpeechModel.EmissionModel wmFind = WordTagsEmissionFrequence.Find(x => x.Word == sWord);
                EmissionProbabilisticModel wFind = WordTagsEmissionProbabilities.Find(x => x.Word == sWord);
                if (wmFind != null && wFind == null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach (var tf in wmFind.TagFreq)
                    {
                        int cti = this.UnigramFrequence.FirstOrDefault(x => x.Key == tf.Key).Value;
                        double pwiti = (double)tf.Value / cti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti)
                        epModel.TagFreq.Add(tf.Key, pwiti);
                    }
                    this.WordTagsEmissionProbabilities.Add(epModel);
                }
            }
        }

        private void calculateUnigramTestCorpus()
        {
            foreach (var uni in this.UnigramFrequence)
            {
                double pi = (double)(uni.Value - 1)  / (this.N - 1);
                this.UnigramProbabilities.Add(uni.Key, pi);
            }
        }

        private void calculateBigramTestCorpus()
        {
            foreach (var bi in this.BigramTransitionFrequence)
            {
                var cti = this.UnigramFrequence.FirstOrDefault(x => x.Key.Equals(bi.Key.Item1)).Value;
                double pti = (double)(bi.Value - 1) / (cti - 1); // Transition probability: p(ti|ti-1) = C(ti-1, ti) / C(ti-1)
                this.BigramTransitionProbabilities.Add(bi.Key, pti);
            }
        }

        private void calculateTrigramTestCorpus()
        {
            foreach (var tri in this.TrigramTransitionFrequence)
            {
                Tuple<string, string> tuple = new Tuple<string, string>(tri.Key.Item1, tri.Key.Item2);
                var cti = this.BigramTransitionFrequence.FirstOrDefault(x => x.Key.Equals(tuple)).Value;
                double pti = (double)(tri.Value - 1) / (cti - 1); // Transition probability: p(ti|ti-1, ti-2) = C(ti-2, ti-1, ti) / C(ti-2, ti-1)
                this.TrigramTransitionProbabilities.Add(tri.Key, pti);
            }
        }
    }
}