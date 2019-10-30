using System;
using System.Collections.Generic;
using System.Text;

namespace Nlp
{
    namespace PosTagger
    {
        public class Corpus
        {
            /// <summary>
            /// Returns the genre of the file (file name), method works only for Brown Text-Corpus (default in PosTagger)
            /// </summary>
            /// <param name="fileName">File name of the file corpus (ex. ca01)</param>
            /// <returns></returns>
            public static string GetTextGenreFromFileName(string fileName)
            {
                char secondLetter = fileName[1];
                switch (secondLetter)
                {
                    case 'a': return "PRESS: REPORTAGE";
                    case 'b': return "PRESS: EDITORIAL";
                    case 'c': return "PRESS: REVIEWS";
                    case 'd': return "RELIGION";
                    case 'e': return "SKILL AND HOBBIES";
                    case 'f': return "POPULAR LORE";
                    case 'g': return "BELLES-LETTRES";
                    case 'h': return "MISCELLANEOUS: GOVERNMENT & HOUSE ORGANS";
                    case 'j': return "LEARNED";
                    case 'k': return "FICTION: GENERAL";
                    case 'l': return "FICTION: MYSTERY";
                    case 'm': return "FICTION: SCIENCE";
                    case 'n': return "FICTION: ADVENTURE";
                    case 'p': return "FICTION: ROMANCE";
                    case 'r': return "HUMOR";
                    default: return "NO GENRE FOUND";
                }
            }
        }
    }
}
