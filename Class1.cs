using System;
using System.Collections.Generic;

namespace Nlp
{
    namespace PosTagger
    {
        public class Test
        {
            public Dictionary<string, string> readTest(string file)
            {
                var files = FileLogic.FileReader.GetAllTextFromFileAsList(file);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach(var ifile in files)
                {
                    dic.Add(ifile.Name, ifile.Text);
                }
                return dic;
            }
            
        }
    }
    
}
