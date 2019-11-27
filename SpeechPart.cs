using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class SpeechPart
    {
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
    }
}
