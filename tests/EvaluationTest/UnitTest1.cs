using NUnit.Framework;
using NLP;
using System.Collections.Generic;

namespace EvaluationTest
{
    public class EvaluatorTest
    {
        Evaluation eval;
        List<Tokenizer.WordTag> realtarget = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("", "A"),
                new Tokenizer.WordTag("", "A"),
                new Tokenizer.WordTag("u1", "B"),
                new Tokenizer.WordTag("", "C"),
                new Tokenizer.WordTag("", "B"),
                new Tokenizer.WordTag("u2", "A"),
                new Tokenizer.WordTag("", "B"),
                new Tokenizer.WordTag("", "A"),
                new Tokenizer.WordTag("", "C"),
                new Tokenizer.WordTag("u3", "B"),
                new Tokenizer.WordTag("", "B"),
                new Tokenizer.WordTag("u4", "C"),
                new Tokenizer.WordTag("", "B"),
                new Tokenizer.WordTag("", "C"),
            };

        List<string> predictedtarget = new List<string>()
            {
                "A", "A", "A", "C", "B", "A", "A", "B", "C", "B", "B", "B", "B", "C"
            };

        [SetUp]
        public void Setup()
        {
            eval = new Evaluation();
            eval.CreateSupervizedEvaluationsMatrix(realtarget, predictedtarget, null);
        }

        [Test]
        public void GetTagsTest()
        {
            var tagsList = eval.GetClassTags();
            HashSet<string> exp = new HashSet<string>() { "A", "B", "C" };
            Assert.AreEqual(exp, tagsList);
        }

        [Test]
        public void GetMatrixTest()
        {
            var res = eval.GetClassificationMatrix();
            List<List<float>> exp = new List<List<float>>()
            {
                new List<float>() { 11f/14, 3f/5, 3f/4, (2 * (3f/5 * 3f/4)) / (3f/5 + 3f/4), 8f/10 }, // A
                new List<float>() { 10f/14, 4f/6, 4f/6, 2f/3, 6f/8 }, // B            ----------         2f/3 = (2 * (4f/6 * 4f/6)) / (4f/6 + 4f/6)
                new List<float>() { 13f/14, 3f/3, 3f/4, (2 * (3f/3 * 3f/4)) / (3f/3 + 3f/4), 10f/10 }, // C
            };
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void GetAllWordsNaiveAccuracyTest()
        {
            float res = eval.GetNaiveAccuracy(realtarget, predictedtarget, new HashSet<string>(), evalMode: "k+u");
            float exp = 10f / 14;
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void GetUnknownWordsNaiveAccuracyTest()
        {
            HashSet<string> unkwords = new HashSet<string>() { "u1", "u2", "u3", "u4" };
            float resUnk = eval.GetNaiveAccuracy(realtarget, predictedtarget, unkwords, evalMode: "u");
            float resK = eval.GetNaiveAccuracy(realtarget, predictedtarget, unkwords, evalMode: "k");

            float expUnk = 2f / 4;
            float expK = 8f / 10;

            Assert.AreEqual(expUnk, resUnk);
            Assert.AreEqual(expK, resK);
        }
    }
}