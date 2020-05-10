using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLP
{
    public partial class HMMTagger
    {
        public double GetValueWeightForUnknownWord(string testWord, string currentTag)
        {
            double proc = 0.0d;
            const double maxVal = 2.5d, minVal = 1.5d; // 2.5 , 1.5
            const double zeroProbabilityDifferenceToMinProbability = 0.01d; // 0.01d / 10^-2

            bool testWordIsCapitalized = false;
            if (char.IsUpper(testWord[0]))
                testWordIsCapitalized = true;
            string lowerWord = testWord.ToLower();

            double suffixVal = 0.0d, preffixVal = 0.0d;

            double minSuffix = 1.0d, minPrefix = 1.0d;


            if (testWordIsCapitalized)
            {
                // founding capitalized prefix min value
                foreach (var pfx in this.PrefixCapitalizedWordEmissionProbabilities)
                {
                    foreach (var pf in pfx.TagFreq)
                    {
                        if (pf.Value < minPrefix)
                            minPrefix = pf.Value;
                    }
                }

                // founding capitalized suffix min value
                foreach (var sfx in this.SuffixCapitalizedWordEmissionProbabilities)
                {
                    foreach (var sf in sfx.TagFreq)
                    {
                        if (sf.Value < minSuffix)
                            minSuffix = sf.Value;
                    }
                }

                foreach (var pfx in this.PrefixCapitalizedWordEmissionProbabilities)
                {
                    if (lowerWord.StartsWith(pfx.Word))
                    {
                        if (pfx.TagFreq.ContainsKey(currentTag))
                        {
                            preffixVal = pfx.TagFreq[currentTag];

                            break;
                        }
                    }
                }

                foreach (var sfx in this.SuffixCapitalizedWordEmissionProbabilities)
                {
                    if (lowerWord.EndsWith(sfx.Word))
                    {
                        if (sfx.TagFreq.ContainsKey(currentTag))
                        {
                            suffixVal = sfx.TagFreq[currentTag];

                            break;
                        }
                    }
                }
            }

            if (minPrefix == 1.0d)
            {
                // founding prefix min value
                foreach (var pfx in this.PrefixEmissionProbabilities)
                {
                    foreach (var pf in pfx.TagFreq)
                    {
                        if (pf.Value < minPrefix)
                            minPrefix = pf.Value;
                    }
                }
            }

            if (minSuffix == 1.0d)
            {
                // founding capitalized suffix min value
                foreach (var sfx in this.SuffixEmissionProbabilities)
                {
                    foreach (var sf in sfx.TagFreq)
                    {
                        if (sf.Value < minSuffix)
                            minSuffix = sf.Value;
                    }
                }
            }

            if (preffixVal == 0.0d)
            {
                foreach (var pfx in this.PrefixEmissionProbabilities)
                {
                    if (lowerWord.StartsWith(pfx.Word))
                    {
                        if (pfx.TagFreq.ContainsKey(currentTag))
                        {
                            preffixVal = pfx.TagFreq[currentTag];

                            break;
                        }
                    }
                }
            }

            if (suffixVal == 0.0d)
            {
                foreach (var sfx in this.SuffixEmissionProbabilities)
                {
                    if (lowerWord.EndsWith(sfx.Word))
                    {
                        if (sfx.TagFreq.ContainsKey(currentTag))
                        {
                            suffixVal = sfx.TagFreq[currentTag];

                            break;
                        }
                    }
                }
            }

            double sum = (double)preffixVal + suffixVal;
            double minSum = (double)(minPrefix + minSuffix);

            if (sum == 0.0d)
            {
                double minProbabilityForZero = TextNormalization.MinMaxNormalization(minSum, 0.0d, 2.0d) * zeroProbabilityDifferenceToMinProbability;
                proc += minProbabilityForZero;
            }
            else
                proc += (double)TextNormalization.MinMaxNormalization(sum, 0.0d, 2.0d);


            const double maxValPossible = maxVal, minValPossible = minVal;
            double occurenceAdder = 0.0d;

            if (testWordIsCapitalized && currentTag == "NN")
                occurenceAdder += (double)maxVal; // max value to be a NN
            if ((lowerWord.EndsWith("\'s") || lowerWord.EndsWith("s\'") || lowerWord.EndsWith("s")) && currentTag == "NN")
                occurenceAdder += (double)maxVal;
            if (lowerWord.Contains(".") && currentTag == "NN")
                occurenceAdder += (double)minVal / 2;
            if ((lowerWord.Contains("-") || lowerWord.Contains("/")) && currentTag == "NN")
                occurenceAdder += (double)minVal / 2;// NN
            if ((lowerWord.Contains("-") || lowerWord.Contains("/")) && currentTag == "JJ")
                occurenceAdder += (double)minVal / 2; // JJ
            if ((lowerWord.Contains("-") && lowerWord.Count(x => x == '-') > 2) && currentTag == "OT")
                occurenceAdder += (double)minVal / 2; // OT (e.g.: At-the-central-library)
            if (lowerWord.Contains("/") && currentTag == "OT")
                occurenceAdder += (double)minVal / 2; // OT
            if (lowerWord.EndsWith("\'t") && currentTag == "VB")
                occurenceAdder += (double)maxVal;
            if ((lowerWord.EndsWith("\'ve") || lowerWord.EndsWith("\'ll")) && currentTag == "PN")
                occurenceAdder += (double)maxVal;

            if (occurenceAdder == 0.0d)
            {
                double minProbabilityForZero = TextNormalization.MinMaxNormalization(minValPossible, 0, maxValPossible) * zeroProbabilityDifferenceToMinProbability;
                proc += minProbabilityForZero;
            }
            else
                proc += TextNormalization.MinMaxNormalization(occurenceAdder, 0, maxValPossible);

            //Console.WriteLine("adder: " + occurenceAdder);

            proc = TextNormalization.BoundProbability(proc);

            //Console.WriteLine("final proc: " + proc + " - current word: " + testWord + " - current tag: " + currentTag);
            //Console.WriteLine();

            return proc;
        }
    }
}
