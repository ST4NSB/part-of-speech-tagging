using System;
using System.Collections.Generic;

namespace NLP
{
    public class Evaluation
    {
        HashSet<string> ClassTags;
        List<List<float>> finalMatrix;

        public Evaluation() {}

        // k+u - known + unknown words
        public float GetNaiveAccuracy(List<Tokenizer.WordTag> testData, List<string> predictedTags, HashSet<string> unknownWords, string evalMode = "k+u")
        {
            int wordsHit = 0;
            int nrOfWords = 0;
            for (int i = 0; i < testData.Count; i++)
            {
                if (evalMode != "k+u")
                {
                    if (unknownWords.Contains(testData[i].word))
                    {
                        if (evalMode == "k")
                            continue;
                    }
                    else
                    {
                        if (evalMode == "u")
                            continue;
                    }
                    
                }
                if (testData[i].tag == predictedTags[i])
                    wordsHit++;
                nrOfWords++;
            }

            float accuracy = (float)wordsHit / nrOfWords;
            return accuracy;
        }

        public void CreateSupervizedEvaluationsMatrix(List<Tokenizer.WordTag> testData, List<string> predictedTags, HashSet<string> unknownWords, string evalMode = "k+u", int fbeta = 1)
        {
            ClassTags = new HashSet<string>();
            finalMatrix = new List<List<float>>();

            foreach (var item in testData)
                this.ClassTags.Add(item.tag);

            foreach (string item in predictedTags)
                this.ClassTags.Add(item);

            foreach(var tag in this.ClassTags)
            {
                int tp = 0, fp = 0, fn = 0, tn = 0; 
                for (int i = 0; i < testData.Count; i++)
                {
                    if (evalMode != "k+u")
                    {
                        if (unknownWords.Contains(testData[i].word))
                        {
                            if (evalMode == "k")
                                continue;
                        }
                        else
                        {
                            if (evalMode == "u")
                                continue;
                        }
                    }
                    

                    if (testData[i].tag != tag && predictedTags[i] != tag)
                        tn++;
                    else if (testData[i].tag == tag && predictedTags[i] == tag)
                        tp++;
                    else if (testData[i].tag == tag && predictedTags[i] != tag)
                        fn++;
                    else if (testData[i].tag != tag && predictedTags[i] == tag)
                        fp++;
                }
                float accuracy = (float)(tp + tn) / (tp + tn + fn + fp);
                if  (float.IsNaN(accuracy) || float.IsInfinity(accuracy))
                    accuracy = 0.0f;
                float precision = (float)tp / (tp + fp);
                if (float.IsNaN(precision) || float.IsInfinity(precision))
                    precision = 0.0f;
                float recall = (float)tp / (tp + fn); // true positive rate
                if (float.IsNaN(recall) || float.IsInfinity(recall))
                    recall = 0.0f;
                float fmeasure = (float) ((fbeta * fbeta + 1) * precision * recall) / ((fbeta * fbeta) * precision + recall);
                if (float.IsNaN(fmeasure) || float.IsInfinity(fmeasure))
                    fmeasure = 0.0f;
                float specificity = (float)tn / (tn + fp); // true negative rate
                if (float.IsNaN(specificity) || float.IsInfinity(specificity))
                    specificity = 0.0f;
                finalMatrix.Add(new List<float>() { accuracy, precision, recall, fmeasure, specificity });
            }
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
            // tag, acc, prec, recall, f1-score, specificity
            return 6;
        }

        public List<List<string>> PrintClassificationResultsMatrix()
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
                    this.finalMatrix[i][3].ToString(), 
                    this.finalMatrix[i][4].ToString()
                });
                i++;
            }

            float totalAccuracy = 0.0f, totalPrecision = 0.0f, totalRecall = 0.0f, totalFmeasure = 0.0f, totalSpecificity = 0.0f;
            for (int j = 0; j < ClassTags.Count; j++)
            {
                totalAccuracy += finalMatrix[j][0];
                totalPrecision += finalMatrix[j][1];
                totalRecall += finalMatrix[j][2];
                totalFmeasure += finalMatrix[j][3];
                totalSpecificity += finalMatrix[j][4];
            }
            totalAccuracy = (float)totalAccuracy / ClassTags.Count;
            totalPrecision = (float)totalPrecision / ClassTags.Count;
            totalRecall = (float)totalRecall / ClassTags.Count;
            totalFmeasure = (float)totalFmeasure / ClassTags.Count;
            totalSpecificity = (float)totalSpecificity / ClassTags.Count;

            matrix.Add(new List<string>()
            {
                "TOTAL",
                totalAccuracy.ToString(),
                totalPrecision.ToString(),
                totalRecall.ToString(),
                totalFmeasure.ToString(),
                totalSpecificity.ToString()
            });

            return matrix;
        }
    }
}
