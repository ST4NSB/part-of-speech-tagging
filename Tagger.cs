using System;
using System.Collections.Generic;

namespace NLP
{
    public class Tagger
    {
        public class SMH { }

        private string trainFile;
        public string TrainFilePath
        {
            get => trainFile;
            set => trainFile = value;
        }
        /// <summary>
        /// Loads Brown Corpus file path automatically.
        /// </summary>
        public Tagger()
        {
            trainFile = "brown corpus to be added..";
        }
        

    }
    
    
}