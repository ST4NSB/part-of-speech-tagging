using System;
using System.Collections.Generic;
using System.Text;

namespace NLP
{
    public class CrossValidation
    {
        /// <summary>
        /// Public members to load training & test files
        /// </summary>
        public string[] TestFile, TrainFile; // length = fold

        /// <summary>
        /// Public constructor to create a structure for evaluating data on cross validation.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fold"></param>
        /// <param name="shuffle"></param>
        public CrossValidation(string filePath, int fold = 10, bool shuffle = false) 
        {
            this.SetFilesForCrossValidation(filePath, fold, shuffle);
        }

        /// <summary>
        /// Private method to divide files into training & test files
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fold"></param>
        /// <param name="shuffle"></param>
        private void SetFilesForCrossValidation(string filePath, int fold = 10, bool shuffle = false) 
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

        /// <summary>
        /// Private method to shuffle list by Fisher-Yates algorithm (Durstenfeld's version)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
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
