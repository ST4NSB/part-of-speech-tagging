using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NLP
{
    public class Decoder
    {
        private List<Tagger.EmissionModel> EmissionFreq;
        private Dictionary<string, int> UnigramFreq;
        private Dictionary<Tuple<string, string>, int> BigramFreq;
        private Dictionary<Tuple<string, string, string>, int> TrigramFreq;

        public List<EmissionProbabilisticModel> EmissionProbabilities;
        public Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities;

        public List<List<ViterbiNode>> ViterbiMatrix;

        private Stopwatch ViterbiDecodeTime;

        public Decoder(
            List<Tagger.EmissionModel> EmissionFreq,
            Dictionary<string, int> UnigramFreq,
            Dictionary<Tuple<string, string>, int> BigramFreq)
        {
            this.EmissionFreq = EmissionFreq;
            this.UnigramFreq = UnigramFreq;
            this.BigramFreq = BigramFreq;
        }

        public Decoder(
            List<Tagger.EmissionModel> EmissionFreq, 
            Dictionary<string, int> UnigramFreq, 
            Dictionary<Tuple<string, string>, int> BigramFreq,
            Dictionary<Tuple<string, string, string>, int> TrigramFreq)
        {
            this.EmissionFreq = EmissionFreq;
            this.UnigramFreq = UnigramFreq;
            this.BigramFreq = BigramFreq;
            this.TrigramFreq = TrigramFreq;
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

        public struct ViterbiNode
        {
            public double value;
            public string CurrentNodeTag;
            public string PreviousNodeTag;
            public string NextNodeTag; // + bidirectionality
            public ViterbiNode(double value, string CurrentNodeTag, string PreviousNodeTag = null, string NextNodeTag = null)
            {
                this.value = value;
                this.CurrentNodeTag = CurrentNodeTag;
                this.PreviousNodeTag = PreviousNodeTag;
                this.NextNodeTag = NextNodeTag;
            }
        }

        public void CalculateProbabilitiesForTestFiles(List<Tokenizer.WordTag> testWords, string model = "bigram")
        {
            this.EmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.BigramTransitionProbabilities = new Dictionary<Tuple<string, string>, double>();

            // emission stage
            foreach(var tw in testWords)
            {
                Tagger.EmissionModel wmFind = EmissionFreq.Find(x => x.Word == tw.word);
                EmissionProbabilisticModel wFind = EmissionProbabilities.Find(x => x.Word == tw.word);
                if(wmFind != null && wFind == null)
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
            foreach(var tuple in this.BigramFreq)
            {
                var cti = this.UnigramFreq.FirstOrDefault(x => x.Key.Equals(tuple.Key.Item1)).Value;
                float pti = (float)tuple.Value / cti; // Transition probability: p(ti|ti-1) = C(ti-1, ti) / C(ti-1)
                this.BigramTransitionProbabilities.Add(tuple.Key, pti);
                
            }

            if(model.Equals("trigram")) 
            {
                // TODO: add condition later for tri-gram
            }
        }

        public void ViterbiDecoding(List<Tokenizer.WordTag> testWords, string model = "bigram")
        {
            this.ViterbiDecodeTime = new Stopwatch();
            this.ViterbiDecodeTime.Start();

            this.ViterbiMatrix = new List<List<ViterbiNode>>();



            if(model.Equals("trigram"))
            {
                // TODO: add condition later for tri-gram
            }

            this.ViterbiDecodeTime.Stop();
        }

        public long GetViterbiDecodingTime()
        {
            return this.ViterbiDecodeTime.ElapsedMilliseconds;
        }
    }
}
