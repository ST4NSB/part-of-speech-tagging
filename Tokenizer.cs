using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class Tokenizer
    {
        /// <summary>
        /// WordTag structure definition (Word - Tag), eg. "The\at" -> (The, at)
        /// </summary>
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
          
        /// <summary>
        /// Static method to tokenize every word from the corpus, eg. "The\at man\nn is\bez here\rb" -> (The\at, man\nn, is\bez, here\rb)
        /// </summary>
        /// <param name="Text"></param>
        /// <returns>List of strings</returns>
        public static List<string> WordTokenizeCorpus(string Text)
        {
            List<string> tokenizedText = new List<string>();
            string word = "";
            foreach(char c in Text)
            {
                if (!Char.IsWhiteSpace(c))
                    word += c;
                else if (!String.IsNullOrEmpty(word))
                {
                    tokenizedText.Add(word);
                    word = "";
                }
            }
            if (!String.IsNullOrEmpty(word))
                tokenizedText.Add(word);
            return tokenizedText;
        }

        ///// <summary>
        ///// Static method to tokenize every word from input, eg. "The man is here." -> (The, man, is, here, .) 
        ///// </summary>
        ///// <param name="Text"></param>
        ///// <returns></returns>
        //public static List<string> WordsOnlyTokenize(string Text)
        //{
        //    List<string> tokenized = new List<string>();
        //    string word = "";
        //    foreach (char c in Text) 
        //    {
        //        if (!Char.IsWhiteSpace(c) && !Char.IsPunctuation(c))
        //        {
        //            word += c;
        //        }
        //        else if (word.Length > 0)
        //        {
        //            if (c != '\'')
        //            {
        //                tokenized.Add(word);
        //                word = "";
        //                if (Char.IsPunctuation(c))
        //                    tokenized.Add(c.ToString());
        //            }
        //            else
        //            {
        //                word += c;
        //            }
        //        }
        //        else if(word.Length == 0)
        //        {
        //            if (c == '\'')
        //                tokenized.Add(c.ToString());
        //        }
        //    }
        //    if (word.Length > 0)
        //        tokenized.Add(word);
        //    return tokenized;
        //}

        /// <summary>
        /// Static method to separate the tag from the word, eg. "The/at" -> (The, at) 
        /// </summary>
        /// <param name="Words"></param>
        /// <returns>List of WordTags</returns>
        public static List<WordTag> SeparateTagFromWord(List<string> Words)
        {
            List<WordTag> wordTags = new List<WordTag>();
            int k = 0;
            foreach (var word in Words) 
            {
                string[] separated = word.Split('/');
                string separatedTag = separated[separated.Length - 1];

                string combWord = "";
                if (separated.Length > 2)
                {
                    for (int i = 0; i < separated.Length - 1; i++) 
                        combWord += separated[i] + "/";
                    combWord = combWord.Remove(combWord.Length - 1);
                }
                else combWord = separated[0];
                wordTags.Add(new WordTag(combWord, separatedTag));
                k++;
            }
            return wordTags;
        }
    }
}
