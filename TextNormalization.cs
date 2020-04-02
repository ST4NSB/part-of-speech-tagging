using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NLP
{
    public static class TextNormalization
    {
        //public static double MinMaxNormalization(double value, double newMax, double newMin, double oldMax = 1.0d, double oldMin = 0.0d)
        //{
        //    return (double)(((value - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        //}

        public static double BoundProbability(double x)
        {
            if (x > 1.0d)
                return 1.0d;
            else if (x < 0.0d)
                return 0.0d;
            else return x;
        }

        public static double MinMaxNormalization(double x, double min, double max)
        {
            return (double)(x - min) / (max - min);
        }

        public static List<Tokenizer.WordTag> PreProcessingPipeline(List<Tokenizer.WordTag> words, bool toLowerOption = false, bool keepOnlyCapitalizedWords = false)
        {
            List<Tokenizer.WordTag> newWords = new List<Tokenizer.WordTag>();
            foreach (var sw in words)
            {
                if (IsStopWord(sw.word)) continue;
                string tsw = EliminateDigitsFromWord(sw.word);
                if (string.IsNullOrEmpty(tsw)) continue;
                if (toLowerOption)
                    tsw = ToLowerCaseNormalization(tsw);
                if(keepOnlyCapitalizedWords)
                    if (!char.IsUpper(tsw[0]))
                        continue;

                newWords.Add(new Tokenizer.WordTag(tsw, sw.tag));
            }
            return newWords;
        }

        private static bool IsStopWord(string word)
        {
            string[] stopWords = { "(", ")", "[", "]", "{", "}" };
            foreach (var sword in stopWords)
                if (word.Equals(sword))
                    return true;
            return false;
        }

        private static string EliminateDotFromWord(string word)
        {
            string newstring = word.Replace(".", string.Empty);
            return newstring;
        }

        private static string EliminateDigitsFromWord(string word)
        {
            if (!word.Any(char.IsDigit))
                return word;
            else
            {
                string output = Regex.Replace(word, @"[\d-]", string.Empty);
                var count = output.Count(char.IsLetter);

                const int x = 3;
                if (count >= x) // verifies if has at least x letters left
                    return output;
                return string.Empty;
            }
        }

        private static string ToLowerCaseNormalization(string word)
        {
            return word.ToLower();
        }

        private static string EliminateApostrophe(string word)
        {
            var output = word.Replace("\'", string.Empty);
            return output;
        }
    }
}
