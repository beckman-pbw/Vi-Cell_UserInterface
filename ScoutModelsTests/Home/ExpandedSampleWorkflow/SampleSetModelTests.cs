using NUnit.Framework;
using ScoutModels.Home.ExpandedSampleWorkflow;

namespace ScoutModelsTests.Home.ExpandedSampleWorkflow
{
    [TestFixture]
    public class SampleSetModelTests
    {
        [Test]
        public void CleanSequentialNamingDisplayNameTests()
        {
            Assert.AreEqual(string.Empty, SampleSetModel.CleanSequentialNamingDisplayName(null));
            Assert.AreEqual(string.Empty, SampleSetModel.CleanSequentialNamingDisplayName(string.Empty));
            
            Assert.AreEqual("Test", SampleSetModel.CleanSequentialNamingDisplayName("Test"));
            Assert.AreEqual("Test", SampleSetModel.CleanSequentialNamingDisplayName("Test[1]"));
            Assert.AreEqual("Test", SampleSetModel.CleanSequentialNamingDisplayName("[1]Test"));
            Assert.AreEqual("Test", SampleSetModel.CleanSequentialNamingDisplayName("[001]Test"));
            Assert.AreEqual("Test", SampleSetModel.CleanSequentialNamingDisplayName("Test[001]"));
            
            Assert.AreEqual("", SampleSetModel.CleanSequentialNamingDisplayName("[001]"));
            Assert.AreEqual("", SampleSetModel.CleanSequentialNamingDisplayName("[01]"));
            Assert.AreEqual("", SampleSetModel.CleanSequentialNamingDisplayName("[1]"));
            Assert.AreEqual("[a]", SampleSetModel.CleanSequentialNamingDisplayName("[a]"));

            Assert.AreEqual("[1a]Test", SampleSetModel.CleanSequentialNamingDisplayName("[1a]Test"));
            Assert.AreEqual("Test[1a]", SampleSetModel.CleanSequentialNamingDisplayName("Test[1a]"));
        }
    }
}