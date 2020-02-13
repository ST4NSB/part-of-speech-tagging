using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class CrossValidation
    {
        private List<string> TrainFiles, TestFiles;
        public string TestFile, TrainFile;

        public CrossValidation() { }

        public void SetFilesForCrossValidation(string filePath, int fold = 10)
        {
            List<string> files = FileReader.GetAllTextFromDirectoryAsList(filePath);
            int filesPerFold = files.Count / fold;

            int crossIndex = 0;


            TrainFiles = new List<string>();
            TestFiles = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                if (i >= filesPerFold)
                {
                    TrainFiles.Add(files[i]);
                }
                else
                {
                    TestFiles.Add(files[i]);
                }
            }

            TestFile = String.Join(" ", TestFiles);
            TrainFile = String.Join(" ", TrainFiles);


            //Console.WriteLine("cross: " + files.Count);



        }
    }
}
