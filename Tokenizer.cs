using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class Tokenizer
    {
        public struct WordTag
        {
            public string word;
            public string tag;
            public WordTag(string word, string tag)
            {
                this.word = word;
                this.tag = tag;
            }
        }
          
        public static List<string> WordTokenize(string Text)
        {
            List<string> tokenizedText = new List<string>();
            string word = "";
            foreach(char c in Text)
            {
                if (!Char.IsWhiteSpace(c))
                    word += c;
                else if (word.Length > 0)
                {
                    tokenizedText.Add(word);
                    word = "";
                }
            }
            if (word.Length > 0)
                tokenizedText.Add(word);
            return tokenizedText;
        }

        public static List<WordTag> SeparateTagFromWord(List<string> Words)
        {
            List<WordTag> wordTags = new List<WordTag>();
            foreach (var word in Words) 
            {
                string[] separated = word.Split('/');
                wordTags.Add(new WordTag(separated[0], separated[1]));
            }
            return wordTags;
        }
    }
}
