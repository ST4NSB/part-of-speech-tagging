using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLP
{
    public class Decoder
    {
        private List<Tagger.EmissionModel> EmissionFreq;
        private Dictionary<string, int> UnigramFreq;
        private Dictionary<Tuple<string, string>, int> BigramFreq;

        public List<EmissionProbabilisticModel> EmissionProbabilities;
        public Dictionary<Tuple<string, string>, float> BigramProbabilities;

        public Decoder(
            List<Tagger.EmissionModel> EmissionFreq, 
            Dictionary<string, int> UnigramFreq, 
            Dictionary<Tuple<string, string>, int> BigramFreq)
        {
            this.EmissionFreq = EmissionFreq;
            this.UnigramFreq = UnigramFreq;
            this.BigramFreq = BigramFreq;
        }

        public class EmissionProbabilisticModel
        {
            public string Word;
            public Dictionary<string, float> TagFreq;
            public EmissionProbabilisticModel()
            {
                this.TagFreq = new Dictionary<string, float>();
            }
        }

        public void CalculateProbabilitiesForTestFiles(List<Tokenizer.WordTag> testWords, string model = "bigram")
        {
            this.EmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.BigramProbabilities = new Dictionary<Tuple<string, string>, float>();

            // emission stage
            foreach(var tw in testWords)
            {
                Tagger.EmissionModel wmFind = EmissionFreq.Find(x => x.Word == tw.word);
                if(wmFind != null)
                {
                    EmissionProbabilisticModel epModel = new EmissionProbabilisticModel();
                    epModel.Word = wmFind.Word;
                    foreach(var tf in wmFind.TagFreq)
                    {
                        int pti = this.UnigramFreq.FirstOrDefault(x => x.Key == tf.Key).Value;
                        float prob = (float)tf.Value / pti; // Emission probability: p(wi/ti) = C(ti, wi) / C(ti)
                        epModel.TagFreq.Add(tf.Key, prob);
                    }
                    this.EmissionProbabilities.Add(epModel);
                }
            }


            if(model.Equals("trigram")) // TODO: add condition later for tri-gram
            {

            }
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
    }
}
