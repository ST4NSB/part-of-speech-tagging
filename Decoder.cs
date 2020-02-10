using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NLP
{
    public class Decoder
    {
        private List<HMMTagger.EmissionModel> EmissionFreq;
        private Dictionary<string, int> UnigramFreq;
        private Dictionary<Tuple<string, string>, int> BigramFreq;
        private Dictionary<Tuple<string, string, string>, int> TrigramFreq;

        public List<EmissionProbabilisticModel> EmissionProbabilities;
        public Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities;


        public List<string> PredictedTags;

        public List<List<ViterbiNode>> ViterbiGraph;

        private Stopwatch ViterbiDecodeTime;

        public Decoder(
            List<HMMTagger.EmissionModel> EmissionFreq,
            Dictionary<string, int> UnigramFreq,
            Dictionary<Tuple<string, string>, int> BigramFreq)
        {
            this.EmissionFreq = EmissionFreq;
            this.UnigramFreq = UnigramFreq;
            this.BigramFreq = BigramFreq;
        }

        public Decoder(
            List<HMMTagger.EmissionModel> EmissionFreq, 
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

        public class ViterbiNode
        {
            public double value;
            public string CurrentTag;
            public List<ViterbiNode> PrevNode;
            public List<ViterbiNode> NextNode; // + bidirectionality
            public ViterbiNode(double value, string CurrentTag, List<ViterbiNode> PrevNode = null, List<ViterbiNode> NextNode = null)
            {
                this.value = value;
                this.CurrentTag = CurrentTag;
                this.PrevNode = PrevNode;
                this.NextNode = NextNode;
            }
        }

        public void CalculateProbabilitiesForTestFiles(List<Tokenizer.WordTag> testWords, string model = "bigram")
        {
            this.EmissionProbabilities = new List<EmissionProbabilisticModel>();
            this.BigramTransitionProbabilities = new Dictionary<Tuple<string, string>, double>();

            // emission stage
            foreach(var tw in testWords)
            {
                HMMTagger.EmissionModel wmFind = EmissionFreq.Find(x => x.Word == tw.word);
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

            this.ForwardAlgorithm(testWords, model);

            if(model.Equals("trigram"))
            {
                // TODO: add condition later for tri-gram
            }

            this.ViterbiDecodeTime.Stop();
        }


        private void ForwardAlgorithm(List<Tokenizer.WordTag> testWords, string model)
        {
            this.PredictedTags = new List<string>();
            this.ViterbiGraph = new List<List<ViterbiNode>>();

            // left to right encoding - forward approach
            bool startPoint = true;
            for (int i = 0; i < testWords.Count; i++)
            {
                if (startPoint)
                {
                   // ViterbiNode vnode = new ViterbiNode(0.0d, ".");
                   // this.ViterbiGraph.Add(new List<ViterbiNode>() { vnode });

                    EmissionProbabilisticModel foundWord = this.EmissionProbabilities.Find(x => x.Word.Equals(testWords[i].word));
                    if (foundWord == null)
                    {
                        Console.WriteLine("Error: word not found[start]");
                        return;
                    }

                    List<ViterbiNode> vList = new List<ViterbiNode>();
                    foreach (var wt in foundWord.TagFreq)
                    {
                        double emissionFreqValue = wt.Value; // eg. Jane -> 0.1111 (NN)
                        Tuple<string, string> tuple = new Tuple<string, string>(".", wt.Key);
                        var biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)); // eg. NN->VB - 0.25
                        if (biTransition.Equals(null))
                        {
                            Console.WriteLine("Error: transition not found[start]");
                            return;
                        }
                        double product = (double)emissionFreqValue * biTransition.Value;
                        ViterbiNode node = new ViterbiNode(product, testWords[i].tag); //,PrevNode: new List<ViterbiNode>() { vnode });
                        vList.Add(node);
                    }
                    this.ViterbiGraph.Add(vList);
                    startPoint = false;
                }
                else
                {
                    List<ViterbiNode> vList = new List<ViterbiNode>();

                    EmissionProbabilisticModel foundWord = this.EmissionProbabilities.Find(x => x.Word.Equals(testWords[i].word));
                    if (foundWord == null)
                    {
                        Console.WriteLine("Error: word not found");
                        return;
                    }

                    foreach (var tf in foundWord.TagFreq)
                    {
                        ViterbiNode vGoodNode = new ViterbiNode(0.0d, "NULL");
                        foreach(ViterbiNode vn in this.ViterbiGraph[this.ViterbiGraph.Count - 1])
                        {
                            Tuple<string, string> tuple = new Tuple<string, string>(vn.CurrentTag, tf.Key);
                            var biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)); // eg. NN->VB - 0.25
                            if (biTransition.Equals(null))
                            {
                                Console.WriteLine("Error: transition not found");
                                //return;
                                continue;
                            }

                            double product = (double)vn.value * biTransition.Value * tf.Value; 
                            if(product > vGoodNode.value)
                            {
                                vGoodNode.value = product;
                                vGoodNode.CurrentTag = tf.Key;
                                List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                prevNodesGoodNode.Add(vn);
                                vGoodNode.PrevNode = prevNodesGoodNode;
                            }
                        }
                        vList.Add(vGoodNode);
                    }
                    this.ViterbiGraph.Add(vList);
                    this.ViterbiGraph[this.ViterbiGraph.Count - 1] = this.ViterbiGraph[this.ViterbiGraph.Count - 1].OrderByDescending(x => x.value).ToList();
                    if (this.ViterbiGraph[this.ViterbiGraph.Count - 1][0].CurrentTag.Equals("."))
                    {
                        Backtrace(mode: "forward");
                        startPoint = true;
                        continue;
                    }
                }
            }
        }

        private void Backtrace(string mode = "forward")
        {
            ViterbiNode lastElement = this.ViterbiGraph[this.ViterbiGraph.Count - 1][0];
            List<string> tagsViterbi = new List<string>();
            if(mode.Equals("forward"))
            {
                while (true)
                {
                    tagsViterbi.Insert(0, lastElement.CurrentTag);
                    // lastElement.PrevNode = lastElement.PrevNode.OrderByDescending(x => x.value).ToList();
                    if (lastElement.PrevNode == null)
                        break;
                    lastElement = lastElement.PrevNode[0];
                }
            }

            this.PredictedTags.AddRange(tagsViterbi);
            this.ViterbiGraph = new List<List<ViterbiNode>>();
        }

        public long GetViterbiDecodingTime()
        {
            return this.ViterbiDecodeTime.ElapsedMilliseconds;
        }
    }
}
