using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class Evaluation
    {
        HashSet<string> ClassTags;
        List<List<float>> finalMatrix;

        public List<List<List<float>>> CrossValEval; // TOTO ADD LOGIC HERE

        public Evaluation()
        {
            this.CrossValEval = new List<List<List<float>>>();
        }

        public float GetSimpleAccuracy(List<Tokenizer.WordTag> realTags, List<string> predictedTags)
        {
            int wordsHit = 0;
            for (int i = 0; i < realTags.Count; i++)
                if (realTags[i].tag == predictedTags[i])
                    wordsHit++;
            return (float)wordsHit / realTags.Count;
        }

        public void CreateSupervizedEvaluationsMatrix(List<Tokenizer.WordTag> realTags, List<string> predictedTags, int fbeta = 1)
        {
            ClassTags = new HashSet<string>();
            finalMatrix = new List<List<float>>();

            foreach (var item in realTags)
                this.ClassTags.Add(item.tag);

            foreach (string item in predictedTags)
                this.ClassTags.Add(item);

            foreach(var tag in this.ClassTags)
            {
                int tp = 0, fp = 0, fn = 0, tn = 0; 
                for (int i = 0; i < realTags.Count; i++)
                {
                    if (realTags[i].tag != tag && predictedTags[i] != tag)
                        tn++;
                    else if (realTags[i].tag == tag && predictedTags[i] == tag)
                        tp++;
                    else if (realTags[i].tag == tag && predictedTags[i] != tag)
                        fn++;
                    else if (realTags[i].tag != tag && predictedTags[i] == tag)
                        fp++;
                }
                float accuracy = (float)(tp + tn) / (tp + tn + fn + fp);
                if  (float.IsNaN(accuracy) || float.IsInfinity(accuracy))
                    accuracy = 0.0f;
                float precision = (float)tp / (tp + fp);
                if (float.IsNaN(precision) || float.IsInfinity(precision))
                    precision = 0.0f;
                float recall = (float)tp / (tp + fn);
                if (float.IsNaN(recall) || float.IsInfinity(recall))
                    recall = 0.0f;
                float fmeasure = (float) ((fbeta * fbeta + 1) * precision * recall) / ((fbeta * fbeta) * precision + recall);
                if (float.IsNaN(fmeasure) || float.IsInfinity(fmeasure))
                    fmeasure = 0.0f;
                finalMatrix.Add(new List<float>() { accuracy, precision, recall, fmeasure });
            }
            this.CrossValEval.Add(finalMatrix);
        }

        public HashSet<string> GetClassTags()
        {
            return this.ClassTags;
        }

        public List<List<float>> GetClassificationMatrix()
        {
            return this.finalMatrix;
        }

        public int GetFullMatrixLineLength()
        {
            return this.ClassTags.Count + 1;
        }
        
        public int GetFullMatrixColLength()
        {
            return 5;
        }

        public List<List<string>> GetFullClassificationMatrix()
        {
            List<List<string>> matrix = new List<List<string>>();
            int i = 0;
            foreach (string hashTag in this.ClassTags)
            {
                matrix.Add(new List<string>()
                {
                    hashTag,
                    this.finalMatrix[i][0].ToString(),
                    this.finalMatrix[i][1].ToString(),
                    this.finalMatrix[i][2].ToString(),
                    this.finalMatrix[i][3].ToString()
                });
                i++;
            }

            float totalAccuracy = 0.0f, totalPrecision = 0.0f, totalRecall = 0.0f, totalFmeasure = 0.0f;
            for (int j = 0; j < ClassTags.Count; j++)
            {
                totalAccuracy += finalMatrix[j][0];
                totalPrecision += finalMatrix[j][1];
                totalRecall += finalMatrix[j][2];
                totalFmeasure += finalMatrix[j][3];
            }
            totalAccuracy = (float)totalAccuracy / ClassTags.Count;
            totalPrecision = (float)totalPrecision / ClassTags.Count;
            totalRecall = (float)totalRecall / ClassTags.Count;
            totalFmeasure = (float)totalFmeasure / ClassTags.Count;

            matrix.Add(new List<string>()
            {
                "TOTAL",
                totalAccuracy.ToString(),
                totalPrecision.ToString(),
                totalRecall.ToString(),
                totalFmeasure.ToString()
            });

            return matrix;
        }
    }
}
