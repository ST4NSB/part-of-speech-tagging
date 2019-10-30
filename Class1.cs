using System;

namespace Nlp
{
    namespace PosTagger
    {
        public class Test
        {
            public string readTest(string file)
            {
                string files = FileLogic.FileReader.GetTextFromFileAsString(file);
                return files;
            }
            
        }
    }
    
}
