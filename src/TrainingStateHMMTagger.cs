using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP
{
    public partial class HMMTagger
    {
        /// <summary>
        /// The main function to create a Hidden Markov Model Network.
        /// </summary>
        /// <param name="uncapitalizedWords"></param>
        /// <param name="capitalizedWords"></param>
        /// <param name="smoothingCoef"></param>
        public void CreateHiddenMarkovModel(List<Tokenizer.WordTag> uncapitalizedWords, List<Tokenizer.WordTag> capitalizedWords, int smoothingCoef = 0)
        {
            this.N = uncapitalizedWords.Count;

            // > .NET 4.0 for task-ing
            Task taskSuffixPrefixEmission = Task.Factory.StartNew(() => this.GetEmissionProbabilitiesForSuffixesAndPrefixes(uncapitalizedWords, capitalizedWords, smoothingCoef));
            Task taskEmissionWords = Task.Factory.StartNew(() => this.CalculateEmissionForWordTags(uncapitalizedWords, capitalizedWords));
            Task taskBigram = Task.Factory.StartNew(() => this.CalculateBigramOccurences(uncapitalizedWords));
            Task taskTrigram = Task.Factory.StartNew(() => this.CalculateTrigramOccurences(uncapitalizedWords));
            Task.WaitAll(taskSuffixPrefixEmission, taskEmissionWords, taskBigram, taskTrigram);

            //this.GetEmissionProbabilitiesForSuffixesAndPrefixes(uncapitalizedWords, capitalizedWords);
            //this.CalculateEmissionForWordTags(uncapitalizedWords, capitalizedWords);
            //this.CalculateBigramOccurences(uncapitalizedWords);
            //this.CalculateTrigramOccurences(uncapitalizedWords);
        }

        private void CalculateEmissionForWordTags(List<Tokenizer.WordTag> uncapitalizedWords, List<Tokenizer.WordTag> capitalizedWords)
        {
            this.WordCapitalizedTagsEmissionFrequence = new List<EmissionModel>();
            this.WordTagsEmissionFrequence = new List<EmissionModel>();

            foreach (var w in capitalizedWords)
            {
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
