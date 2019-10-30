using System;
using System.Collections.Generic;
using System.Text;

namespace File
{
    class FileReader
    {
        private string fileName;
        public FileReader(string fileName)
        {
            this.fileName = fileName;
        }
        public string getStringFromFile()
        {
            string rFile = System.IO.File.ReadAllText(this.fileName);
            return rFile;
        }

    }
}
