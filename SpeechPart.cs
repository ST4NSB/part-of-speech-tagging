using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class SpeechPart
    {
        /// <summary>
        /// Static Method to count the frequency of every individual Part of Speech in the Corpus
        /// </summary>
        /// <param name="Words"></param>
        /// <returns>A dictionary of strings and ints</returns>
        public static Dictionary<string, int> SpeechPartFrequence(List<Tokenizer.WordTag> Words)
        {
            Dictionary<string, int> speechCount = new Dictionary<string, int>();
            foreach(var item in Words)
            {
                if (speechCount.ContainsKey(item.tag))
                    speechCount[item.tag] += 1;
                else speechCount.Add(item.tag, 1);
            }
            return speechCount;
        }

        public static List<Tokenizer.WordTag> GetNewAbstractTags(List<Tokenizer.WordTag> Words)
        {
            List<Tokenizer.WordTag> newWords = new List<Tokenizer.WordTag>();
            foreach (var w in Words)
            {
                int tagIndex = GetTagIndexForConversion(w);
                string newTag = ConvertBrownTagToAbstractTag(tagIndex);
                Tokenizer.WordTag newWord = new Tokenizer.WordTag();
                newWord.word = w.word;
                newWord.tag = newTag;
                newWords.Add(newWord);
            }
            return newWords;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagIndex"></param>
        /// <returns></returns>
        private static string ConvertBrownTagToAbstractTag(int tagIndex)
        {
            string tag = "Tag NOT found! Something went wrong!";
            if (tagIndex >= 0 && tagIndex <= 8)
                tag = "NN";
            else if (tagIndex >= 9 && tagIndex <= 20)
                tag = "PN";
            else if (tagIndex >= 21 && tagIndex <= 40)
                tag = "VB";
            else if (tagIndex >= 41 && tagIndex <= 44)
                tag = "JJ";
            else if (tagIndex >= 45 && tagIndex <= 52)
                tag = "RB";
            else if (tagIndex >= 53 && tagIndex <= 54)
                tag = "PP";
            else if (tagIndex >= 55 && tagIndex <= 57)
                tag = "CC";
            else if (tagIndex >= 58 && tagIndex <= 70)
                tag = "AT/DT";
            else if (tagIndex >= 71 && tagIndex <= 72)
                tag = ".";
            else
                tag = "OT";
            return tag;
        }

        private static int GetTagIndexForConversion(Tokenizer.WordTag Word)
        {
            int tagIndex = -1;
            List<string> BrownCorpusTags = new List<string>()
            {
                "nn", "nns", "nns$", "np", "np$", "nps", "nps$", "nr", "nrs",
                "pn", "pn$", "pp$", "pp$$", "ppl", "ppls", "ppo", "pps", "ppss", "wp$", "wpo", "wps",
                "vb", "vbd", "vbg", "vbn", "vbz", "bem", "ber", "bez", "bed", "bedz", "ben", "do", "dod", "doz", "hv",
                "hvd", "hvg", "hvn", "hvz", "md",
                "jj", "jjr", "jjs", "jjt", 
                "rb", "rbr", "rbt", "rn", "rp", "wrb", "ql", "qlp",
                "in", "to", 
                "cc", "cs", "wql", 
                "at", "ap", "abl", "abn", "abx", "dt", "dti", "dts", "dtx", "be", "beg", "ex", "wdt",
                ".", "hl"
            };

            for (int i = 0; i < BrownCorpusTags.Count; i++)
            {
                string[] splittedWord = Word.tag.Split(new Char[] { '+', '-' });
                foreach (string w in splittedWord)
                {
                    if (Word.tag.Equals("wql") || Word.tag.Equals("wql-tl")) // special case where wql is found in RB at ql
                    {
                        tagIndex = 57;
                        return tagIndex;
                    }
                    else if (Word.tag.Contains(BrownCorpusTags[i]))
                    {
                        tagIndex = i;
                        return tagIndex;
                    }
                }
            }
            return tagIndex;
        }


    }
}
