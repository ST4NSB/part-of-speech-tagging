using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class CrossValidation
    {
        public string[] TestFile, TrainFile;

        public CrossValidation() { }

        public void SetFilesForCrossValidation(string filePath, int fold = 10, bool shuffle = false) 
        {
            List<string> files = FileReader.GetAllTextFromDirectoryAsList(filePath);
            int filesPerFold = files.Count / fold;

            TestFile = new string[fold];
            TrainFile = new string[fold];

            if (shuffle) 
                files = this.Shuffle(files);

            for (int crossIndex = 0; crossIndex < fold; crossIndex++)
            {
                var IndividualTrainFiles = new List<string>();
                var IndividualTestFiles = new List<string>();
                for (int i = 0; i < files.Count; i++)
                {
                    if (i >= (filesPerFold * crossIndex) && i < (filesPerFold * (crossIndex + 1)))
                    {
                        IndividualTestFiles.Add(files[i]);
                    }
                    else
                    {
                        IndividualTrainFiles.Add(files[i]);
                    }
                }
                string trainf = String.Join("  ", IndividualTrainFiles);
                string testf = String.Join("  ", IndividualTestFiles);
                this.TrainFile[crossIndex] = trainf;
                this.TestFile[crossIndex] = testf;
            }
        }

        private List<string> Shuffle(List<string> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1) 
            {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list; 
        }

    }
}
