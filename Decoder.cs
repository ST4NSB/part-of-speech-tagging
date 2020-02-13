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
        public Dictionary<string, double> UnigramProbabilities;
        public Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities;
        public Dictionary<Tuple<string, string, string>, double> TrigramTransitionProbabilities;

        public List<string> PredictedTags;

        public List<List<ViterbiNode>> ViterbiGraph;

        private Stopwatch ViterbiDecodeTime;

        private List<ViterbiNode> ForwardHistory, BackwardHistory;

        private double lambda1, lambda2, lambda3;

        public Decoder() { }

        public Decoder(
            List<HMMTagger.EmissionProbabilisticModel> EmissionProbabilities,
            Dictionary<string, double> UnigramProbabilities,
            Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities)
        {
            this.EmissionProbabilities = EmissionProbabilities;
            this.UnigramProbabilities = UnigramProbabilities;
            this.BigramTransitionProbabilities = BigramTransitionProbabilities;
        }

        public Decoder(
            List<HMMTagger.EmissionProbabilisticModel> EmissionProbabilities,
            Dictionary<string, double> UnigramProbabilities,
            Dictionary<Tuple<string, string>, double> BigramTransitionProbabilities,
            Dictionary<Tuple<string, string, string>, double> TrigramTransitionProbabilities)
        {
            this.EmissionProbabilities = EmissionProbabilities;
            this.UnigramProbabilities = UnigramProbabilities;
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

        public void SetLambdaValues(Tuple<double, double, double> lambdaTuple)
        {
            lambda1 = lambdaTuple.Item1;
            lambda2 = lambdaTuple.Item2;
            lambda3 = lambdaTuple.Item3;
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
            int triPoz = -1;
            for (int i = 0; i < testWords.Count; i++) // starting from left (0 index)
            {
                triPoz++;
                if (testWords[i].tag == ".") // we can verify word instead of tag here
                {
                    Backtrace(method: "forward"); // decompress method, going from right to left using prev nodes, applied only when '.' is met
                    startPoint = true;
                    continue;
                }
                if (startPoint) // first node (start)
                {
                    triPoz = 0;
                    HMMTagger.EmissionProbabilisticModel foundWord = this.EmissionProbabilities.Find(x => x.Word.Equals(testWords[i].word));
                    List<ViterbiNode> vList = new List<ViterbiNode>();

                    if(foundWord != null)
                        if (foundWord.TagFreq.Count == 1 && foundWord.TagFreq.ContainsKey(".")) // case where the only tag is '.'
                            foundWord = null;

                    if (foundWord == null)
                    {
                        // we take the best transition case where first item is "."
                        // case 2: all the transitions
                        var orderedTransitions = BigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();
                        double product = 0.0d;
                        string nodeTag = "NULL";

                        foreach (var item in orderedTransitions)
                            if (item.Key.Item1.Equals(".") && item.Key.Item2 != ".")
                            {
                                double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(item.Key.Item2)).Value;
                                double biTrans = (double)(uniVal * lambda1) + (item.Value * lambda2);
                                product = biTrans;
                                nodeTag = item.Key.Item2;
                                ViterbiNode node = new ViterbiNode(product, nodeTag);
                                vList.Add(node);
                            }
                    }
                    else
                    {
                        foreach (var wt in foundWord.TagFreq)
                        {
                            if (wt.Key == ".")
                                continue;
                            double emissionFreqValue = wt.Value; // eg. Jane -> 0.1111 (NN)
                            Tuple<string, string> tuple = new Tuple<string, string>(".", wt.Key);
                            double biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)).Value; // eg. NN->VB - 0.25

                            double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(wt.Key)).Value;
                            double biTrans = (double)(uniVal * lambda1) + (biTransition * lambda2);
                            double product = (double)emissionFreqValue * biTrans;
                            ViterbiNode node = new ViterbiNode(product, wt.Key);
                            vList.Add(node);
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
                        for (int j = 0; j < this.ViterbiGraph[this.ViterbiGraph.Count - 1].Count; j++)
                        {
                            ViterbiNode vGoodNode = new ViterbiNode(0.0d, "NULL");
                            ViterbiNode elem = this.ViterbiGraph[this.ViterbiGraph.Count - 1][j];
                            // we take the best transition case where first item is "."

                            var orderedTransitions = BigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();

                            if (model == "trigram" && triPoz >= 2)
                            {
                                if (elem.PrevNode == null)
                                    continue;
                                elem.PrevNode = elem.PrevNode.OrderByDescending(x => x.value).ToList();
                                ViterbiNode elem2 = elem.PrevNode[0];
                                var orderedTransitionsTri = TrigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();
                                
                                double product = 0.0d;
                                string nodeTag = "NULL_TRI";

                                foreach (var item in orderedTransitionsTri)
                                    if (item.Key.Item1.Equals(elem2.CurrentTag) && item.Key.Item2.Equals(elem.CurrentTag) && item.Key.Item3 != ".")
                                    {
                                        Tuple<string, string> biTuple = new Tuple<string, string>(elem.CurrentTag, item.Key.Item3);
                                        double biVal = this.BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(biTuple)).Value;

                                        double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(item.Key.Item3)).Value;

                                        double triTransition = (double)(lambda3 * item.Value) + (lambda2 * biVal) + (lambda1 * uniVal);

                                        product = (double)elem.value * triTransition;
                                        nodeTag = item.Key.Item3;
                                        if (product >= vGoodNode.value)
                                        {
                                            vGoodNode.value = product;
                                            vGoodNode.CurrentTag = nodeTag;
                                            List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                            prevNodesGoodNode.Add(elem);
                                            vGoodNode.PrevNode = prevNodesGoodNode;
                                            break;
                                        }
                                    }
                            }
                            else
                            {
                                double product = 0.0d;
                                string nodeTag = "NULL_BI";

                                foreach (var item in orderedTransitions)
                                    if (item.Key.Item1.Equals(elem.CurrentTag) && item.Key.Item2 != ".")
                                    {
                                        double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(item.Key.Item2)).Value;
                                        double biTrans = (double)(uniVal * lambda1) + (item.Value * lambda2);
                                        product = (double)elem.value * biTrans;
                                        nodeTag = item.Key.Item2;
                                        if (product >= vGoodNode.value)
                                        {
                                            vGoodNode.value = product;
                                            vGoodNode.CurrentTag = nodeTag;
                                            List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                            prevNodesGoodNode.Add(elem);
                                            vGoodNode.PrevNode = prevNodesGoodNode;
                                            break;
                                        }
                                    }
                            }
                            vList.Add(vGoodNode);
                        }
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
                                if(model == "trigram" && triPoz >= 2)
                                {
                                    if (vn.PrevNode == null)
                                        continue;
                                    vn.PrevNode = vn.PrevNode.OrderByDescending(x => x.value).ToList();
                                    Tuple<string, string, string> triTuple = new Tuple<string, string, string>(vn.PrevNode[0].CurrentTag, vn.CurrentTag, tf.Key);
                                    double triVal = this.TrigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(triTuple)).Value;

                                    Tuple<string, string> biTuple = new Tuple<string, string>(vn.CurrentTag, tf.Key);
                                    double biVal = this.BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(biTuple)).Value;

                                    double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(tf.Key)).Value;

                                    double triTransition = (double)(lambda3 * triVal) + (lambda2 * biVal) + (lambda1 * uniVal);
                                    double product = (double)vn.value * triTransition * tf.Value;
                                    if(product >= vGoodNode.value)
                                    {
                                        vGoodNode.value = product;
                                        vGoodNode.CurrentTag = tf.Key;
                                        List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                        prevNodesGoodNode.Add(vn);
                                        vGoodNode.PrevNode = prevNodesGoodNode;
                                    }
                                }
                                else
                                {
                                    Tuple<string, string> tuple = new Tuple<string, string>(vn.CurrentTag, tf.Key);
                                    double biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)).Value; // eg. NN->VB - 0.25

                                    double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(tf.Key)).Value;
                                    double biTrans = (double)(uniVal * lambda1) + (biTransition * lambda2);
                                    double product = (double)vn.value * biTrans * tf.Value;
                                    if (product >= vGoodNode.value)
                                    {
                                        vGoodNode.value = product;
                                        vGoodNode.CurrentTag = tf.Key;
                                        List<ViterbiNode> prevNodesGoodNode = new List<ViterbiNode>();
                                        prevNodesGoodNode.Add(vn);
                                        vGoodNode.PrevNode = prevNodesGoodNode;
                                    }
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
            // right to left encoding - backward approach
            bool startPoint = true;
            int triPoz = -1;
            for (int i = testWords.Count - 2; i >= -1; i--)
            {
                triPoz++;
                if (i == -1) // we first check to see if we got to index -1
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
                    triPoz = 0;
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
                        string nodeTag = "NULL";

                        foreach (var item in orderedTransitions)
                            if (item.Key.Item2.Equals(".") && item.Key.Item1 != ".")
                            {
                                double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(item.Key.Item1)).Value;
                                double biTrans = (double)(uniVal * lambda1) + (item.Value * lambda2);

                                product = biTrans;
                                nodeTag = item.Key.Item1;
                                ViterbiNode node = new ViterbiNode(product, nodeTag);
                                vList.Add(node);
                            }
                    }
                    else
                    {
                        foreach (var wt in foundWord.TagFreq)
                        {
                            if (wt.Key == ".")
                                continue;
                            double emissionFreqValue = wt.Value; // eg. Jane -> 0.1111 (NN)
                            Tuple<string, string> tuple = new Tuple<string, string>(wt.Key, ".");
                            double biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)).Value; // eg. NN->VB - 0.25

                            double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(wt.Key)).Value;
                            double biTrans = (double)(uniVal * lambda1) + (biTransition * lambda2);

                            double product = (double)emissionFreqValue * biTrans;
                            ViterbiNode node = new ViterbiNode(product, wt.Key);
                            vList.Add(node);
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
                        for (int j = 0; j < this.ViterbiGraph[this.ViterbiGraph.Count - 1].Count; j++)
                        {
                            ViterbiNode elem = this.ViterbiGraph[this.ViterbiGraph.Count - 1][j];
                            ViterbiNode vGoodNode = new ViterbiNode(0.0d, "NULL");
                            // we take the best transition case where first item is "."
                            var orderedTransitions = BigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();

                            if (model == "trigram" && triPoz >= 2)
                            {
                                if (elem.NextNode == null)
                                    continue;
                                elem.NextNode = elem.NextNode.OrderByDescending(x => x.value).ToList();
                                ViterbiNode elem2 = elem.NextNode[0];
                                var orderedTransitionsTri = TrigramTransitionProbabilities.OrderByDescending(x => x.Value).ToList();

                                double product = 0.0d;
                                string nodeTag = "NULL_TRI";

                                foreach (var item in orderedTransitionsTri)
                                    if (item.Key.Item3.Equals(elem2.CurrentTag) && item.Key.Item2.Equals(elem.CurrentTag) && item.Key.Item1 != ".")
                                    {
                                        Tuple<string, string> biTuple = new Tuple<string, string>(item.Key.Item1, elem.CurrentTag);
                                        double biVal = this.BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(biTuple)).Value;

                                        double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(item.Key.Item1)).Value;

                                        double triTransition = (double)(lambda3 * item.Value) + (lambda2 * biVal) + (lambda1 * uniVal);

                                        product = (double)elem.value * triTransition;
                                        nodeTag = item.Key.Item1;
                                        if (product >= vGoodNode.value)
                                        {
                                            vGoodNode.value = product;
                                            vGoodNode.CurrentTag = nodeTag;
                                            List<ViterbiNode> nextNodeTags = new List<ViterbiNode>();
                                            nextNodeTags.Add(elem);
                                            vGoodNode.NextNode = nextNodeTags;
                                            break;
                                        }
                                    }
                            }
                            else
                            {
                                double product = 0.0d;
                                string nodeTag = "NULL_BI";

                                foreach (var item in orderedTransitions)
                                    if (item.Key.Item2.Equals(elem.CurrentTag) && item.Key.Item1 != ".")
                                    {
                                        double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(item.Key.Item1)).Value;
                                        double biTrans = (double)(uniVal * lambda1) + (item.Value * lambda2);

                                        product = (double)elem.value * biTrans;
                                        nodeTag = item.Key.Item1;
                                        if (product >= vGoodNode.value)
                                        {
                                            vGoodNode.value = product;
                                            vGoodNode.CurrentTag = nodeTag;
                                            List<ViterbiNode> nextGoodNodes = new List<ViterbiNode>();
                                            nextGoodNodes.Add(elem);
                                            vGoodNode.NextNode = nextGoodNodes;
                                            break;
                                        }
                                    }
                            }
                            vList.Add(vGoodNode);
                        }
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
                                if (model == "trigram" && triPoz >= 2)
                                {
                                    if (vn.NextNode == null)
                                        continue;
                                    vn.NextNode = vn.NextNode.OrderByDescending(x => x.value).ToList();
                                    Tuple<string, string, string> triTuple = new Tuple<string, string, string>(tf.Key, vn.CurrentTag, vn.NextNode[0].CurrentTag);
                                    double triVal = this.TrigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(triTuple)).Value;

                                    Tuple<string, string> biTuple = new Tuple<string, string>(tf.Key, vn.CurrentTag);
                                    double biVal = this.BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(biTuple)).Value;

                                    double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(tf.Key)).Value;

                                    double triTransition = (double)(lambda3 * triVal) + (lambda2 * biVal) + (lambda1 * uniVal);
                                    double product = (double)vn.value * triTransition * tf.Value;

                                    if (product >= vGoodNode.value)
                                    {
                                        vGoodNode.value = product;
                                        vGoodNode.CurrentTag = tf.Key;
                                        List<ViterbiNode> nextGoodNodes = new List<ViterbiNode>();
                                        nextGoodNodes.Add(vn);
                                        vGoodNode.NextNode = nextGoodNodes;
                                    }
                                }
                                else
                                {
                                    Tuple<string, string> tuple = new Tuple<string, string>(tf.Key, vn.CurrentTag);
                                    double biTransition = BigramTransitionProbabilities.FirstOrDefault(x => x.Key.Equals(tuple)).Value; // eg. NN->VB - 0.25

                                    double uniVal = this.UnigramProbabilities.FirstOrDefault(x => x.Key.Equals(tf.Key)).Value;
                                    double biTrans = (double)(uniVal * lambda1) + (biTransition * lambda2);

                                    double product = (double)vn.value * biTrans * tf.Value;
                                    if (product >= vGoodNode.value)
                                    {
                                        vGoodNode.value = product;
                                        vGoodNode.CurrentTag = tf.Key;
                                        List<ViterbiNode> nextGoodNode = new List<ViterbiNode>();
                                        nextGoodNode.Add(vn);
                                        vGoodNode.NextNode = nextGoodNode;
                                    }
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
