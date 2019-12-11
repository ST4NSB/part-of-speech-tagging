using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NLP
{
    public class Tagger
    {
        public List<WordModel> Models;
        private Stopwatch TrainingTime;

        /// <summary>
        /// The Model struct definition (Word - Dic[Tag, Tag_Frequency]), eg. (The, [at, 1]) 
        /// </summary>
        public class WordModel
        {
            public string Word;
            public Dictionary<string, int> TagFreq;
            public WordModel() 
            {
                this.TagFreq = new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Constructor that creates the list of models (SVM) for every individual word with a dictionary of grammar tags
        /// </summary>
        /// <param name="wordsInput">List of words - tag, eg. The - at)</param>
        public Tagger(List<Tokenizer.WordTag> wordsInput)
        {
            this.TrainingTime = new Stopwatch();
            this.TrainingTime.Start();

            this.Models = new List<WordModel>();
            foreach(var w in wordsInput)
            {
                WordModel wmFind = Models.Find(x => x.Word == w.word);
                if (wmFind == null)
                {
                    WordModel wModel = new WordModel();
                    wModel.Word = w.word;
                    wModel.TagFreq.Add(w.tag, 1);
                    this.Models.Add(wModel);
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

            this.TrainingTime.Stop();
        }

        //public Dictionary<string, string> EasyWordTag(List<string> inputWords)
        //{
        //    Dictionary<string, string> output = new Dictionary<string, string>();
        //    foreach(string word in inputWords)
        //    {
        //        WordModel wordModelFinder = this.Models.Find(x => x.Word == word);
        //        var maxValueTag = wordModelFinder.TagFreq.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        //        output.Add(word, maxValueTag);
        //    }
        //    return output;
        //} 

        /// <summary>
        /// Method that returns the elapsed time, loading SVM (ms)
        /// </summary>
        public long GetTrainingTimeMs()
        {
            return this.TrainingTime.ElapsedMilliseconds;
        }

    }
}