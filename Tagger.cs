using System;
using System.Collections.Generic;

namespace NLP
{
    public class GrammarTagger
    {
        public List<WordModel> Models;

        public class WordModel
        {
            public string Word;
            public Dictionary<string, int> TagFreq;
            public WordModel()
            {
                TagFreq = new Dictionary<string, int>();
            }
        }

        public GrammarTagger(List<Tokenizer.WordTag> wordsInput)
        {
            Models = new List<WordModel>();
            foreach(var w in wordsInput)
            {
                bool wordFound = false;
                foreach (var model in Models)
                {
                    if (model.Word == w.word)
                    {
                        wordFound = true;
                        bool tagFound = false;
                        foreach (var tag in model.TagFreq)
                        {
                            if (tag.Key == w.tag) 
                            {
                                tagFound = true;
                                model.TagFreq[tag.Key] += 1;
                                break;
                            }
                        }
                        if (!tagFound)
                        {
                            model.TagFreq.Add(w.tag, 1);
                        }
                    }
                }
                if (!wordFound) 
                {
                    WordModel wModel = new WordModel();
                    wModel.Word = w.word;
                    wModel.TagFreq.Add(w.tag, 1);
                    Models.Add(wModel);
                }
            }
        }

    }
}