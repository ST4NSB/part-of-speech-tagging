using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NLP
{
    public class Decoder
    {
        public List<HMMTagger.EmissionProbabilisticModel> EmissionProbabilities;
        public Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities;
        public Dictionary<Tuple<string, string, string>, double> TrigramTransitionProbabilities;

        public List<string> PredictedTags;

        public List<List<ViterbiNode>> ViterbiGraph;

        private Stopwatch ViterbiDecodeTime;

        private List<ViterbiNode> ForwardHistory, BackwardHistory;

        public Decoder() { }

        public Decoder(
            List<HMMTagger.EmissionProbabilisticModel> EmissionProbabilities,
            Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities)
        {
            this.EmissionProbabilities = EmissionProbabilities;
            this.BigramTransitionProbabilities = BigramTransitionProbabilities;
        }

        public Decoder(
            List<HMMTagger.EmissionProbabilisticModel> EmissionProbabilities,
            Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities,
            Dictionary<Tuple<string, string, string>, double> TrigramTransitionProbabilities)
        {
            this.EmissionProbabilities = EmissionProbabilities;
            this.BigramTransitionProbabilities = BigramTransitionProbabilities;
            this.TrigramTransitionProbabilities = TrigramTransitionProbabilities;
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

        public void ViterbiDecoding(List<Tokenizer.WordTag> testWords, string model = "bigram", string mode = "forward")
        {
            this.ViterbiDecodeTime = new Stopwatch();
            this.ViterbiDecodeTime.Start();

            this.ForwardHistory = new List<ViterbiNode>();
            this.BackwardHistory = new List<ViterbiNode>();

            this.PredictedTags = new List<string>();
            this.ViterbiGraph = new List<List<ViterbiNode>>();

            if(mode.Equals("forward") || mode.Equals("f+b"))
                this.ForwardAlgorithm(testWords, model, mode);
            if (mode.Equals("backward") || mode.Equals("f+b"))
                this.BackwardAlgorithm(testWords, model, mode);

            if(mode.Equals("f+b"))
                BiDirectionalModelTrace();

            this.ViterbiDecodeTime.Stop();
        }


        private void ForwardAlgorithm(List<Tokenizer.WordTag> testWords, string model, string mode)
        {
            // left to right encoding - forward approach
            bool startPoint = true;
            for (int i = 0; i < testWords.Count; i++)
            {
                if (testWords[i].tag == ".") // we can verify word instead of tag here
                {
                    Backtrace(method: "forward");
                    startPoint = true;
                    continue;
                }
                if (startPoint)
                {
                    HMMTagger.EmissionProbabilisticModel foundWord = this.EmissionProbabilities.Find(x => x.Word.Equals(testWords[i].word));
                    List<ViterbiNode> vList = new List<ViterbiNode>();

                    if(foundWord != null)
                        if (foundWord.TagFreq.Count == 1 && foundWord.TagFreq.ContainsKey("."))
                            foundWord = null;

                    if (foundWord == null)
                    {
                        // we take the best transition case where first item is "."
                        var orderedTransitions = BigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();
                        double product = 0.0d;
                        string nextTag = "NULL";

                        foreach (var item in orderedTransitions)
                            if (item.Key.Item1.Equals(".") && item.Key.Item2 != ".")
                            {
                                product = item.Value;
                                nextTag = item.Key.Item2;
                                break;
                            }
                        ViterbiNode node = new ViterbiNode(product, nextTag);
                        vList.Add(node);
                    }
                    else
                    {
                        foreach (var wt in foundWord.TagFreq)
                        {
                            if (wt.Key == ".")
                                continue;
                            double emissionFreqValue = wt.Value; // eg. Jane -> 0.1111 (NN)
                            Tuple<string, string> tuple = new Tuple<string, string>(".", wt.Key);
                            var biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)); // eg. NN->VB - 0.25
                            if (!biTransition.Equals(null))
                            {
                                double product = (double)emissionFreqValue * biTransition.Value;
                                ViterbiNode node = new ViterbiNode(product, wt.Key); 
                                vList.Add(node);
                            } 
                            else // case 1
                            {
                                ViterbiNode node = new ViterbiNode(0.0d, "NN"); 
                                vList.Add(node);
                            }
                        }
                    }
                    this.ViterbiGraph.Add(vList);
                    startPoint = false;
                }
                else
                {
                    HMMTagger.EmissionProbabilisticModel foundWord = this.EmissionProbabilities.Find(x => x.Word.Equals(testWords[i].word));
                    List<ViterbiNode> vList = new List<ViterbiNode>();

                    if (foundWord != null)
                        if (foundWord.TagFreq.Count == 1 && foundWord.TagFreq.ContainsKey("."))
                            foundWord = null;

                    if (foundWord == null)
                    {
                        ViterbiNode elem = this.ViterbiGraph[this.ViterbiGraph.Count - 1][0];
                        // we take the best transition case where first item is "."
                        var orderedTransitions = BigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();
                        // trigram cond.


                        ViterbiNode vGoodNode = new ViterbiNode(0.0d, "NULL");
                        double product = 0.0d;
                        string nextTag = "NULL";

                        foreach (var item in orderedTransitions)
                            if (item.Key.Item1.Equals(elem.CurrentTag) && item.Key.Item2 != ".")
                            {
                                product = (double)elem.value * item.Value;
                                nextTag = item.Key.Item2;
                                if (product >= vGoodNode.value)
                                {
                                    vGoodNode.value = product;
                                    vGoodNode.CurrentTag = nextTag;
                                    List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                    prevNodesGoodNode.Add(elem);
                                    vGoodNode.PrevNode = prevNodesGoodNode;
                                    break;
                                }
                            }
                        vList.Add(vGoodNode);
                    }
                    else
                    {
                        foreach (var tf in foundWord.TagFreq)
                        {
                            if (tf.Key == ".")
                                continue;
                            ViterbiNode vGoodNode = new ViterbiNode(0.0d, "NULL");
                            foreach (ViterbiNode vn in this.ViterbiGraph[this.ViterbiGraph.Count - 1])
                            {
                                Tuple<string, string> tuple = new Tuple<string, string>(vn.CurrentTag, tf.Key);
                                var biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)); // eg. NN->VB - 0.25
                                if (!biTransition.Equals(null))
                                {
                                    double product = (double)vn.value * biTransition.Value * tf.Value;
                                    if (product >= vGoodNode.value)
                                    {
                                        vGoodNode.value = product;
                                        vGoodNode.CurrentTag = tf.Key;
                                        List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                        prevNodesGoodNode.Add(vn);
                                        vGoodNode.PrevNode = prevNodesGoodNode;
                                    }
                                }
                                else // case 2
                                {
                                    vGoodNode.value = 0.0d;
                                    vGoodNode.CurrentTag = "NN";
                                    List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                    prevNodesGoodNode.Add(vn);
                                    vGoodNode.PrevNode = prevNodesGoodNode;
                                }
                            }
                            vList.Add(vGoodNode);
                        }
                    }
                    this.ViterbiGraph.Add(vList);
                }
                this.ViterbiGraph[this.ViterbiGraph.Count - 1] = this.ViterbiGraph[this.ViterbiGraph.Count - 1].OrderByDescending(x => x.value).ToList();
            }
        }

        private void BackwardAlgorithm(List<Tokenizer.WordTag> testWords, string model, string mode)
        {
            // left to right encoding - forward approach
            bool startPoint = true;
            for (int i = testWords.Count - 2; i >= -1; i--)
            {
                if(i == -1)
                {
                    Backtrace(method: "backward");
                    startPoint = true;
                    continue;
                }
                if (testWords[i].tag == ".")  
                {
                    Backtrace(method: "backward");
                    startPoint = true;
                    continue;
                }
                if (startPoint)
                {
                    HMMTagger.EmissionProbabilisticModel foundWord = this.EmissionProbabilities.Find(x => x.Word.Equals(testWords[i].word));
                    List<ViterbiNode> vList = new List<ViterbiNode>();

                    if (foundWord != null)
                        if (foundWord.TagFreq.Count == 1 && foundWord.TagFreq.ContainsKey("."))
                            foundWord = null;

                    if (foundWord == null)
                    {
                        // we take the best transition case where first item is "."
                        var orderedTransitions = BigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();
                        double product = 0.0d;
                        string nextTag = "NULL";

                        foreach (var item in orderedTransitions)
                            if (item.Key.Item2.Equals(".") && item.Key.Item1 != ".")
                            {
                                product = item.Value;
                                nextTag = item.Key.Item1;
                                break;
                            }
                        ViterbiNode node = new ViterbiNode(product, nextTag);
                        vList.Add(node);
                    }
                    else
                    {
                        foreach (var wt in foundWord.TagFreq)
                        {
                            if (wt.Key == ".")
                                continue;
                            double emissionFreqValue = wt.Value; // eg. Jane -> 0.1111 (NN)
                            Tuple<string, string> tuple = new Tuple<string, string>(wt.Key, ".");
                            var biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)); // eg. NN->VB - 0.25
                            if (!biTransition.Equals(null))
                            {
                                double product = (double)emissionFreqValue * biTransition.Value;
                                ViterbiNode node = new ViterbiNode(product, wt.Key); 
                                vList.Add(node);
                            }
                            else // case 1
                            {
                                ViterbiNode node = new ViterbiNode(0.0d, "NN"); 
                                vList.Add(node);
                            }
                        }
                    }
                    this.ViterbiGraph.Add(vList);
                    startPoint = false;
                }
                else
                {
                    HMMTagger.EmissionProbabilisticModel foundWord = this.EmissionProbabilities.Find(x => x.Word.Equals(testWords[i].word));
                    List<ViterbiNode> vList = new List<ViterbiNode>();

                    if (foundWord != null)
                        if (foundWord.TagFreq.Count == 1 && foundWord.TagFreq.ContainsKey("."))
                            foundWord = null;

                    if (foundWord == null)
                    {
                        ViterbiNode elem = this.ViterbiGraph[this.ViterbiGraph.Count - 1][0];
                        // we take the best transition case where first item is "."
                        var orderedTransitions = BigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();
                        // trigram cond.


                        ViterbiNode vGoodNode = new ViterbiNode(0.0d, "NULL");
                        double product = 0.0d;
                        string nextTag = "NULL";

                        foreach (var item in orderedTransitions)
                            if (item.Key.Item2.Equals(elem.CurrentTag) && item.Key.Item1 != ".")
                            {
                                product = (double)elem.value * item.Value;
                                nextTag = item.Key.Item1;
                                if (product >= vGoodNode.value)
                                {
                                    vGoodNode.value = product;
                                    vGoodNode.CurrentTag = nextTag;
                                    List<ViterbiNode> nextGoodNodes = new List<ViterbiNode>();
                                    nextGoodNodes.Add(elem);
                                    vGoodNode.NextNode = nextGoodNodes;
                                    break;
                                }
                            }
                        vList.Add(vGoodNode);
                    }
                    else
                    {
                        foreach (var tf in foundWord.TagFreq)
                        {
                            if (tf.Key == ".")
                                continue;
                            ViterbiNode vGoodNode = new ViterbiNode(0.0d, "NULL");
                            foreach (ViterbiNode vn in this.ViterbiGraph[this.ViterbiGraph.Count - 1])
                            {
                                Tuple<string, string> tuple = new Tuple<string, string>(tf.Key, vn.CurrentTag);
                                var biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)); // eg. NN->VB - 0.25
                                if (!biTransition.Equals(null))
                                {
                                    double product = (double)vn.value * biTransition.Value * tf.Value;
                                    if (product >= vGoodNode.value)
                                    {
                                        vGoodNode.value = product;
                                        vGoodNode.CurrentTag = tf.Key;
                                        List<ViterbiNode> nextGoodNode = new List<ViterbiNode>();
                                        nextGoodNode.Add(vn);
                                        vGoodNode.NextNode = nextGoodNode;
                                    }
                                }
                                else // case 2
                                {
                                    vGoodNode.value = 0.0d;
                                    vGoodNode.CurrentTag = "NN";
                                    List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                    prevNodesGoodNode.Add(vn);
                                    vGoodNode.PrevNode = prevNodesGoodNode;
                                }
                            }
                            vList.Add(vGoodNode);
                        }
                    }
                    this.ViterbiGraph.Add(vList);
                }
                this.ViterbiGraph[this.ViterbiGraph.Count - 1] = this.ViterbiGraph[this.ViterbiGraph.Count - 1].OrderByDescending(x => x.value).ToList();
            }

            if (mode == "backward")
            {
                this.PredictedTags = new List<string>();
                List<ViterbiNode> historyCopy = new List<ViterbiNode>(BackwardHistory);
                for (int i = 0; i < historyCopy.Count; i++)
                {
                    List<string> tagsViterbi = new List<string>();
                    while (true)
                    {
                        if (historyCopy[i].CurrentTag != ".")
                            tagsViterbi.Add(historyCopy[i].CurrentTag);
                        if (historyCopy[i].NextNode == null)
                            break;
                        historyCopy[i] = historyCopy[i].NextNode[0];
                    }
                    this.PredictedTags.AddRange(tagsViterbi);
                }
            }
        }

        private void Backtrace(string method)
        {
            ViterbiNode lastElement = this.ViterbiGraph[this.ViterbiGraph.Count - 1][0];
            List<string> tagsViterbi = new List<string>();
            if(method.Equals("forward"))
            {
                ForwardHistory.Add(lastElement);
                while (true)
                {
                    if (lastElement.CurrentTag != ".")
                        tagsViterbi.Insert(0, lastElement.CurrentTag);
                    if (lastElement.PrevNode == null)
                        break;
                    lastElement = lastElement.PrevNode[0];
                }
                this.PredictedTags.AddRange(tagsViterbi);

            }
            else if(method.Equals("backward"))
            {
                BackwardHistory.Insert(0, lastElement);
            }
            
            this.ViterbiGraph = new List<List<ViterbiNode>>(); // can be deleted, also saves ALL forward states and backwards states
        }

        private void BiDirectionalModelTrace()
        {
            this.PredictedTags = new List<string>();
            for(int i = 0; i < BackwardHistory.Count; i++)
            {
                if(BackwardHistory[i].value > ForwardHistory[i].value)
                {
                    //Console.WriteLine("backward!!!");
                    List<string> tagsViterbi = new List<string>();
                    while (true)
                    {
                        if (BackwardHistory[i].CurrentTag != ".")
                            tagsViterbi.Add(BackwardHistory[i].CurrentTag);
                        if (BackwardHistory[i].NextNode == null)
                            break;
                        BackwardHistory[i] = BackwardHistory[i].NextNode[0];
                    }
                    this.PredictedTags.AddRange(tagsViterbi);
                }
                else
                {
                    //Console.WriteLine("forward!!!");
                    List<string> tagsViterbi = new List<string>();
                    while (true)
                    {
                        if(ForwardHistory[i].CurrentTag != ".")
                            tagsViterbi.Insert(0, ForwardHistory[i].CurrentTag);
                        if (ForwardHistory[i].PrevNode == null)
                            break;
                        ForwardHistory[i] = ForwardHistory[i].PrevNode[0];
                    }
                    this.PredictedTags.AddRange(tagsViterbi);
                }
            }
        }

        public long GetViterbiDecodingTime()
        {
            return this.ViterbiDecodeTime.ElapsedMilliseconds;
        }
    }
}
