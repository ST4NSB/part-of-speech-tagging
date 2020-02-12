using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NLP
{
    public class TextNormalization
    {
        public static List<Tokenizer.WordTag> Pipeline(List<Tokenizer.WordTag> words)
        {
            List<Tokenizer.WordTag> newWords = new List<Tokenizer.WordTag>();
            foreach (var word in words)
            {
                var sw = word.word;
                // var splittedWords = word.word.Split(new Char[] { '-', '/' }).ToList();
                //foreach (var sw in splittedWords)
                //{
                if (!IsStopWord(sw))
                {
                    string tsw = EliminateDigitsFromWord(sw);
                    if (!string.IsNullOrEmpty(tsw))
                    {
                        tsw = ToLowerCaseNormalization(tsw);
                        tsw = EliminateApostrophe(tsw);
                        newWords.Add(new Tokenizer.WordTag(tsw, word.tag));
                    }
                }
                //}
            }
            return newWords;
        }

        private static bool IsStopWord(string word)
        {
            string[] stopWords = { "``", "\"", "\'", "''", "(", ")", "[", "]", "{", "}" };
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
                //int count = Regex.Matches(output, @"[a-zA-z]").Count;
                var count = output.Count(char.IsLetter);
                if (count >= 3) // verifies if has at least 3 letters left
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
