using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NLP
{
    public class GrammarTagger
    {
        public List<WordModel> Models;
        private Stopwatch TrainingTime;

        public class WordModel
        {
            public string Word;
            public Dictionary<string, int> TagFreq;
            public WordModel() 
            {
                this.TagFreq = new Dictionary<string, int>();
            }
        }

        public GrammarTagger(List<Tokenizer.WordTag> wordsInput)
        {
            TrainingTime = new Stopwatch();
            TrainingTime.Start();

            Models = new List<WordModel>();
            foreach(var w in wordsInput)
            {
                WordModel wmFind = Models.Find(x => x.Word == w.word);
                if (wmFind == null)
                {
                    WordModel wModel = new WordModel();
                    wModel.Word = w.word;
                    wModel.TagFreq.Add(w.tag, 1);
                    Models.Add(wModel);
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

            TrainingTime.Stop();
        }

        public long GetTrainingTimeMs()
        {
            return this.TrainingTime.ElapsedMilliseconds;
        }

    }
}