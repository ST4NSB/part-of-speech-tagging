using System;
using System.Collections.Generic;

namespace Nlp
{
    namespace PosTagger
    {
        public class Tagger
        {
            private string trainFile;
            public string TrainFilePath 
            {
                get => trainFile;
                set => trainFile = value;
            }
            /// <summary>
            /// Loads Brown Corpus automatically.
            /// </summary>
            public Tagger()
            {
                trainFile = "brown corpus to be added..";
            }
            public Dictionary<string, string> readTest(string file)
            {
                var files = FileLogic.FileReader.GetAllTextFromFileAsList(file);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach(var ifile in files)
                {
                    dic.Add(ifile.name, ifile.text);
                }
                return dic;
            }
            
        }
    }
    
}
