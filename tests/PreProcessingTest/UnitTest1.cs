using NUnit.Framework;
using NLP;
using System.Collections.Generic;

namespace PreProcessingTest
{
    public class TokenizerTest
    {
        string simpleInput = "Simple/nn Test/nn sentence/nn for/cc testing/vb ./.";
        string complicatedInput = "\tSolo/nn\r\n\r\ncompound/word/nn compound\\word2/nn compound-word3/nn ;/; for/cc testing/vb ./." +
            "\r\n\r\n\tShould/vb also/cc get/vb this/at ./.";
        List<string> simpleListOfWords = new List<string>()
            {
                "Simple/nn",
                "Test/nn",
                "sentence/nn",
                "for/cc",
                "testing/vb",
                "./."
            };
        List<string> complicatedListOfWords = new List<string>()
            {
                "Solo/nn",
                "compound/word/nn",
                "compound\\word2/nn",
                "compound-word3/nn",
                ";/;",
                "for/cc",
                "testing/vb",
                "./.",
                "Should/vb",
                "also/cc",
                "get/vb",
                "this/at",
                "./."
            };
    

        [Test]
        public void SimpleWordTokenizeCorpusTest()
        {
            List<string> expected = simpleListOfWords;
            List<string> res = Tokenizer.TokenizePennTreebank(simpleInput);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void ComplicatedWordTokenizeCorpusTest()
        {
            List<string> expected = complicatedListOfWords;
            List<string> res = Tokenizer.TokenizePennTreebank(complicatedInput);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void SimpleSeparateTagFromWordTest()
        {
            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("Simple", "nn"),
                new Tokenizer.WordTag("Test", "nn"),
                new Tokenizer.WordTag("sentence", "nn"),
                new Tokenizer.WordTag("for", "cc"),
                new Tokenizer.WordTag("testing", "vb"),
                new Tokenizer.WordTag(".", "."),
            };

            List<Tokenizer.WordTag> res = Tokenizer.SeparateTagFromWord(simpleListOfWords);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void ComplicatedSeparateTagFromWordTest()
        {
            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("Solo", "nn"),
                new Tokenizer.WordTag("compound/word", "nn"),
                new Tokenizer.WordTag("compound\\word2", "nn"),
                new Tokenizer.WordTag("compound-word3", "nn"),
                new Tokenizer.WordTag(";", ";"),
                new Tokenizer.WordTag("for", "cc"),
                new Tokenizer.WordTag("testing", "vb"),
                new Tokenizer.WordTag(".", "."),
                new Tokenizer.WordTag("Should", "vb"),
                new Tokenizer.WordTag("also", "cc"),
                new Tokenizer.WordTag("get", "vb"),
                new Tokenizer.WordTag("this", "at"),
                new Tokenizer.WordTag(".", "."),
            };

            List<Tokenizer.WordTag> res = Tokenizer.SeparateTagFromWord(complicatedListOfWords);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void SpecialCases()
        {
            List<string> input = new List<string>()
            { 
                "word//////////test///////////nn", 
                "word2\\\\\\\\\\\\\\\\2\\\\\\/////////ddii///vb\\\\\\////vb", 
                "word3=nn==vb===---pn------\\\\----\"33/////3[[]';.[]])(*&^%$@@/////pn",
            };
            List<Tokenizer.WordTag> res = Tokenizer.SeparateTagFromWord(input);

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("word//////////test//////////", "nn"),
                new Tokenizer.WordTag("word2\\\\\\\\\\\\\\\\2\\\\\\/////////ddii///vb\\\\\\///", "vb"),
                new Tokenizer.WordTag("word3=nn==vb===---pn------\\\\----\"33/////3[[]';.[]])(*&^%$@@////", "pn"),
            };
            Assert.AreEqual(expected, res);
        }
    }

    public class SpeechPartTest
    {
        [Test]
        public void UniformDistributedSpeechPartFrequenceTest()
        {
            List<Tokenizer.WordTag> input = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "PN"),
                new Tokenizer.WordTag("test", "RB"),
                new Tokenizer.WordTag("test", "JJ"),
                new Tokenizer.WordTag("test", "PP"),
                new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "."),
            };

            Dictionary<string, int> expected = new Dictionary<string, int>()
            {
                {"NN", 1},
                {"VB", 1},
                {"AT/DT", 1},
                {"PN", 1},
                {"RB", 1},
                {"JJ", 1},
                {"PP", 1},
                {"CC", 1},
                {"OT", 1},
                {".", 1},
            };

            Dictionary<string, int> res = SpeechPartClassifier.TagsFrequence(input);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void NonUniformSpeechPartFrequenceTest()
        {
            List<Tokenizer.WordTag> input = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "."),
                new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "PN"),
                new Tokenizer.WordTag("test", "RB"),
                new Tokenizer.WordTag("test", "JJ"),
                new Tokenizer.WordTag("test", "JJ"),
                new Tokenizer.WordTag("test", "PP"),
                new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "."),
                new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "CC"),
            };

            Dictionary<string, int> expected = new Dictionary<string, int>()
            {
                {"AT/DT", 4},
                {"NN", 6},
                {"CC", 5},
                {"VB", 1},
                {".", 2},
                {"PN", 1},
                {"RB", 1},
                {"JJ", 2},
                {"PP", 1},
                {"OT", 2},
            };

            Dictionary<string, int> res = SpeechPartClassifier.TagsFrequence(input);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void MissingTagsSpeechPartFrequenceTest()
        {
            List<Tokenizer.WordTag> input = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "CC"),
            };

            Dictionary<string, int> expected = new Dictionary<string, int>()
            {
                {"CC", 3},
                {"AT/DT", 2},
                {"NN", 5},
            };

            Dictionary<string, int> res = SpeechPartClassifier.TagsFrequence(input);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void SimpleGetNewHierarchicTagsTest()
        {
            List<Tokenizer.WordTag> input = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("test", "wps"), new Tokenizer.WordTag("test", "vbd"),
            };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>
            {
                new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "VB"),
            };

            List<Tokenizer.WordTag> res = SpeechPartClassifier.GetNewHierarchicTags(input);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void GetNewHierarchicTagsWithoutOTTest()
        {
            List<Tokenizer.WordTag> input = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("test", "nn"), new Tokenizer.WordTag("test", "nns"), new Tokenizer.WordTag("test", "nns$"),
                new Tokenizer.WordTag("test", "np"), new Tokenizer.WordTag("test", "np$"), new Tokenizer.WordTag("test", "nps"),
                new Tokenizer.WordTag("test", "nps$"), new Tokenizer.WordTag("test", "nr"), new Tokenizer.WordTag("test", "nrs"),

                new Tokenizer.WordTag("test", "pn"), new Tokenizer.WordTag("test", "pn$"), new Tokenizer.WordTag("test", "pp$"),
                new Tokenizer.WordTag("test", "pp$$"), new Tokenizer.WordTag("test", "ppl"), new Tokenizer.WordTag("test", "ppls"),
                new Tokenizer.WordTag("test", "ppo"), new Tokenizer.WordTag("test", "pps"), new Tokenizer.WordTag("test", "ppss"),
                new Tokenizer.WordTag("test", "wp$"), new Tokenizer.WordTag("test", "wpo"), new Tokenizer.WordTag("test", "wps"),

                new Tokenizer.WordTag("test", "vb"), new Tokenizer.WordTag("test", "vbd"), new Tokenizer.WordTag("test", "vbg"),
                new Tokenizer.WordTag("test", "vbn"), new Tokenizer.WordTag("test", "vbz"), new Tokenizer.WordTag("test", "bem"),
                new Tokenizer.WordTag("test", "ber"), new Tokenizer.WordTag("test", "bez"), new Tokenizer.WordTag("test", "bed"),
                new Tokenizer.WordTag("test", "bedz"), new Tokenizer.WordTag("test", "ben"), new Tokenizer.WordTag("test", "do"),
                new Tokenizer.WordTag("test", "dod"), new Tokenizer.WordTag("test", "doz"), new Tokenizer.WordTag("test", "hv"),
                new Tokenizer.WordTag("test", "hvd"), new Tokenizer.WordTag("test", "hvg"), new Tokenizer.WordTag("test", "hvn"),
                new Tokenizer.WordTag("test", "hvz"), new Tokenizer.WordTag("test", "md"),
                
                new Tokenizer.WordTag("test", "jj"), new Tokenizer.WordTag("test", "jjr"), new Tokenizer.WordTag("test", "jjs"),
                new Tokenizer.WordTag("test", "jjt"),

                new Tokenizer.WordTag("test", "rb"), new Tokenizer.WordTag("test", "rbr"), new Tokenizer.WordTag("test", "rbt"),
                new Tokenizer.WordTag("test", "rn"), new Tokenizer.WordTag("test", "rp"), new Tokenizer.WordTag("test", "wrb"),
                new Tokenizer.WordTag("test", "ql"), new Tokenizer.WordTag("test", "qlp"),

                new Tokenizer.WordTag("test", "in"), new Tokenizer.WordTag("test", "to"),

                new Tokenizer.WordTag("test", "cc"), new Tokenizer.WordTag("test", "cs"), new Tokenizer.WordTag("test", "wql"),
                new Tokenizer.WordTag("test", "wql-tl"),

                new Tokenizer.WordTag("test", "at"), new Tokenizer.WordTag("test", "ap"), new Tokenizer.WordTag("test", "abl"),
                new Tokenizer.WordTag("test", "abn"), new Tokenizer.WordTag("test", "abn"), new Tokenizer.WordTag("test", "abx"),
                new Tokenizer.WordTag("test", "dt"), new Tokenizer.WordTag("test", "dti"), new Tokenizer.WordTag("test", "dts"),
                new Tokenizer.WordTag("test", "dtx"), new Tokenizer.WordTag("test", "be"), new Tokenizer.WordTag("test", "beg"),
                new Tokenizer.WordTag("test", "ex"), new Tokenizer.WordTag("test", "wdt"),

                new Tokenizer.WordTag("test", "."),
            };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>
            {
                new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "NN"),

                new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"),
                new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"),
                new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"),
                new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PN"),

                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "VB"),

                new Tokenizer.WordTag("test", "JJ"), new Tokenizer.WordTag("test", "JJ"), new Tokenizer.WordTag("test", "JJ"),
                new Tokenizer.WordTag("test", "JJ"),

                new Tokenizer.WordTag("test", "RB"), new Tokenizer.WordTag("test", "RB"), new Tokenizer.WordTag("test", "RB"),
                new Tokenizer.WordTag("test", "RB"), new Tokenizer.WordTag("test", "RB"), new Tokenizer.WordTag("test", "RB"),
                new Tokenizer.WordTag("test", "RB"), new Tokenizer.WordTag("test", "RB"),

                new Tokenizer.WordTag("test", "PP"), new Tokenizer.WordTag("test", "PP"),

                new Tokenizer.WordTag("test", "CC"), new Tokenizer.WordTag("test", "CC"), new Tokenizer.WordTag("test", "CC"),
                new Tokenizer.WordTag("test", "CC"),

                new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"),
                new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "AT/DT"),

                new Tokenizer.WordTag("test", "."),
            };

            List<Tokenizer.WordTag> res = SpeechPartClassifier.GetNewHierarchicTags(input);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void GetNewHierarchicTagsWithOTTest()
        {
            List<Tokenizer.WordTag> input = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("test", "dod*"), new Tokenizer.WordTag("test", "uh"), new Tokenizer.WordTag("test", "\'"),
                new Tokenizer.WordTag("test", "do*"), new Tokenizer.WordTag("test", "ap$"), new Tokenizer.WordTag("test", "cd"),
                new Tokenizer.WordTag("test", "cd$"), new Tokenizer.WordTag("test", "ber*"), new Tokenizer.WordTag("test", ","),
                new Tokenizer.WordTag("test", "*"), new Tokenizer.WordTag("test", "\'\'"), new Tokenizer.WordTag("test", "``"),
                new Tokenizer.WordTag("test", "("), new Tokenizer.WordTag("test", ")"), new Tokenizer.WordTag("test", ":"),
                new Tokenizer.WordTag("test", "nil"),
            };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>
            {
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "OT"), new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "AT/DT"), new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "OT"), new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "OT"), new Tokenizer.WordTag("test", "OT"), new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "OT"), new Tokenizer.WordTag("test", "OT"), new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "OT"), 
            };

            List<Tokenizer.WordTag> res = SpeechPartClassifier.GetNewHierarchicTags(input);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void GetNewHierarchicCompoundTagsTest()
        {
            List<Tokenizer.WordTag> input = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("test", "nns$-tl"), new Tokenizer.WordTag("test", "pps+hvz"), new Tokenizer.WordTag("test", "fw-rb"),
                new Tokenizer.WordTag("test", "pp$-tl"), new Tokenizer.WordTag("test", "---hl"), new Tokenizer.WordTag("test", ":-tl"),
                new Tokenizer.WordTag("test", "np+bez"), new Tokenizer.WordTag("test", "wps+bez"), new Tokenizer.WordTag("test", "fw-in+at-tl"),
                new Tokenizer.WordTag("test", "vbg+to"), new Tokenizer.WordTag("test", "fw-at+nn-tl"), new Tokenizer.WordTag("test", "fw-nn$-tl"),
                new Tokenizer.WordTag("test", "fw-vb-nc"),
            };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>
            {
                new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "RB"),
                new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "OT"), new Tokenizer.WordTag("test", "OT"),
                new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "PN"), new Tokenizer.WordTag("test", "PP"),
                new Tokenizer.WordTag("test", "VB"), new Tokenizer.WordTag("test", "NN"), new Tokenizer.WordTag("test", "NN"),
                new Tokenizer.WordTag("test", "VB"),
            };

            List<Tokenizer.WordTag> res = SpeechPartClassifier.GetNewHierarchicTags(input);
            Assert.AreEqual(expected, res);
        }
    }

    public class DataCleaningAndNormalizationTest
    {
        [Test]
        public void BoundProbabilityTest()
        {
            double x = -1.0d, y = -0.000001d;
            double t = 2.0d, z = 1.00000000001;
            double n = 0.483d;

            Assert.AreEqual(0.0d, TextNormalization.BoundProbability(x));
            Assert.AreEqual(0.0d, TextNormalization.BoundProbability(y));
            Assert.AreEqual(1.0d, TextNormalization.BoundProbability(t));
            Assert.AreEqual(1.0d, TextNormalization.BoundProbability(z));
            Assert.AreEqual(0.483d, TextNormalization.BoundProbability(n));
        }

        [Test]
        public void MinMaxNormalization()
        {
            Assert.AreEqual(4.0d, TextNormalization.MinMaxNormalization(x: 5.0d, min: 1.0d, max: 2.0d));
            Assert.AreEqual(3.0d, TextNormalization.MinMaxNormalization(x: 20.0d, min: 5.0d, max: 10.0d));
        }

        [Test]
        public void PreProcessingPipelineStopWordsTest()
        {
            List<Tokenizer.WordTag> inputSw = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("(", "("), new Tokenizer.WordTag(")", ")"), new Tokenizer.WordTag("[", "["),
                    new Tokenizer.WordTag("]", "]"), new Tokenizer.WordTag("{", "nil"), new Tokenizer.WordTag("}", "nil"),
                };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>();
            List<Tokenizer.WordTag> res = TextNormalization.PreProcessingPipeline(inputSw);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void PreProcessingPipelineDigitsTest()
        {
            List<Tokenizer.WordTag> inputSw = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("123.03", "cd"), new Tokenizer.WordTag("9780", "cd"), new Tokenizer.WordTag("9780-965", "cd"),
                    new Tokenizer.WordTag("abc", "cd"), new Tokenizer.WordTag("123d", "cd"), new Tokenizer.WordTag("123de", "cd"),
                    new Tokenizer.WordTag("123def", "cd"), new Tokenizer.WordTag("123456defg", "cd"), new Tokenizer.WordTag("abc-123", "cd"),
                    new Tokenizer.WordTag("123++-", "cd"), new Tokenizer.WordTag("9A78B087C", "cd"), new Tokenizer.WordTag("9A78B087", "cd"),
                };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
            {
                new Tokenizer.WordTag("abc", "cd"), new Tokenizer.WordTag("def", "cd"), new Tokenizer.WordTag("defg", "cd"),
                new Tokenizer.WordTag("abc", "cd"), new Tokenizer.WordTag("ABC", "cd")
            };
            List<Tokenizer.WordTag> res = TextNormalization.PreProcessingPipeline(inputSw);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void PreProcessingPipelineAllWordsToLowerTest()
        {
            List<Tokenizer.WordTag> inputSw = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag("hello", "nn"),
                    new Tokenizer.WordTag("HeLLo", "nn"),
                    new Tokenizer.WordTag("hELLO", "nn"),
                    new Tokenizer.WordTag("HELLO", "nn"),
                };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
            {
                    new Tokenizer.WordTag("hello", "nn"),
                    new Tokenizer.WordTag("hello", "nn"),
                    new Tokenizer.WordTag("hello", "nn"),
                    new Tokenizer.WordTag("hello", "nn"),
                    new Tokenizer.WordTag("hello", "nn"),
            };
            List<Tokenizer.WordTag> res = TextNormalization.PreProcessingPipeline(inputSw, toLowerOption: true);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void PreProcessingPipelineKeepCapitalWordsTest()
        {
            List<Tokenizer.WordTag> inputSw = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag("hello", "nn"),
                    new Tokenizer.WordTag("HELLO", "nn"),
                    new Tokenizer.WordTag("hELLO", "nn"),
                };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
            {
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag("HELLO", "nn"),
            };
            List<Tokenizer.WordTag> res = TextNormalization.PreProcessingPipeline(inputSw, keepOnlyCapitalizedWords: true);
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void EliminateDuplicatesEOSTest()
        {
            List<Tokenizer.WordTag> inputSw = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag("sup", "nn"),
                    new Tokenizer.WordTag("?", "."),
                    new Tokenizer.WordTag("?", "."),
                    new Tokenizer.WordTag("!", "."),
                    new Tokenizer.WordTag("!", "."),
                    new Tokenizer.WordTag("test", "nn"),
                    new Tokenizer.WordTag("!", "."),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag("\\", "."),
                    new Tokenizer.WordTag("/", "."),
                    new Tokenizer.WordTag("~", "."),
                    new Tokenizer.WordTag("?", "."),
                    new Tokenizer.WordTag(".", "."),
                };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag("sup", "nn"),
                    new Tokenizer.WordTag("?", "."),
                    new Tokenizer.WordTag("test", "nn"),
                    new Tokenizer.WordTag("!", "."),
                };
            var result = TextNormalization.EliminateDuplicateSequenceOfEndOfSentenceTags(inputSw);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void EliminateAllEOSTest()
        {
            List<Tokenizer.WordTag> inputSw = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag("What", "nn"),
                    new Tokenizer.WordTag("is", "nn"),
                    new Tokenizer.WordTag("this", "nn"),
                    new Tokenizer.WordTag("?", "."),
                    new Tokenizer.WordTag("good", "nn"),
                    new Tokenizer.WordTag("stuff", "nn"),
                    new Tokenizer.WordTag(".", "."),
                    new Tokenizer.WordTag(".", "."),
                };

            List<Tokenizer.WordTag> expected = new List<Tokenizer.WordTag>()
                {
                    new Tokenizer.WordTag("Hello", "nn"),
                    new Tokenizer.WordTag("What", "nn"),
                    new Tokenizer.WordTag("is", "nn"),
                    new Tokenizer.WordTag("this", "nn"),
                    new Tokenizer.WordTag("good", "nn"),
                    new Tokenizer.WordTag("stuff", "nn"),
                };
            TextNormalization.EliminateAllEndOfSentenceTags(ref inputSw);
            Assert.AreEqual(expected, inputSw);
        }
    }
}