using GrpcService;
using NUnit.Framework;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;

namespace ScoutOpcUaTests
{
    [TestFixture]
    [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
    public class SampleProgressEventArgsToDeleteSampleResultsArgs : BaseAutoMapperUnitTest
    {
        [Test]
        public void Test1()
        {
            var sampleArgs = new SampleProgressEventArgs(HawkeyeError.eSuccess)
            {
                OperationIsComplete = false,
                PercentComplete = 11,
                ProgressMessage = "test string"
            };

            var map = Mapper.Map<DeleteSampleResultsArgs>(sampleArgs);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleArgs.PercentComplete, map.PercentComplete);
            // @todo update test
            //Assert.AreEqual(sampleArgs.OperationIsComplete, map.DeleteStatus);
            //Assert.AreEqual(sampleArgs.Error == HawkeyeError.eSuccess, map.Success);
        }

        [Test]
        public void Test2()
        {
            var sampleArgs = new SampleProgressEventArgs(HawkeyeError.eReagentError)
            {
                OperationIsComplete = false,
                PercentComplete = 13,
                ProgressMessage = "test string"
            };

            var map = Mapper.Map<DeleteSampleResultsArgs>(sampleArgs);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleArgs.PercentComplete, map.PercentComplete);
            //Assert.AreEqual(sampleArgs.OperationIsComplete, map.OperationIsComplete);
            //Assert.AreEqual(sampleArgs.Error == HawkeyeError.eSuccess, map.Success);
        }

        [Test]
        public void Test3()
        {
            var sampleArgs = new SampleProgressEventArgs(HawkeyeError.eSuccess)
            {
                OperationIsComplete = true,
                PercentComplete = 100,
                ProgressMessage = "test string"
            };

            var map = Mapper.Map<DeleteSampleResultsArgs>(sampleArgs);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleArgs.PercentComplete, map.PercentComplete);
            //Assert.AreEqual(sampleArgs.OperationIsComplete, map.OperationIsComplete);
            //Assert.AreEqual(sampleArgs.Error == HawkeyeError.eSuccess, map.Success);
        }

        [Test]
        public void Test4()
        {
            var sampleArgs = new SampleProgressEventArgs(HawkeyeError.eEntryInvalid)
            {
                OperationIsComplete = true,
                PercentComplete = 100,
                ProgressMessage = "test string"
            };

            var map = Mapper.Map<DeleteSampleResultsArgs>(sampleArgs);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleArgs.PercentComplete, map.PercentComplete);
            //Assert.AreEqual(sampleArgs.OperationIsComplete, map.OperationIsComplete);
            //Assert.AreEqual(sampleArgs.Error == HawkeyeError.eSuccess, map.Success);
        }

        [Test]
        public void Test5()
        {
            var sampleArgs = new SampleProgressEventArgs(HawkeyeError.eEntryInvalid);

            var map = Mapper.Map<DeleteSampleResultsArgs>(sampleArgs);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleArgs.PercentComplete, map.PercentComplete);
            //Assert.AreEqual(sampleArgs.OperationIsComplete, map.OperationIsComplete);
            //Assert.AreEqual(sampleArgs.Error == HawkeyeError.eSuccess, map.Success);
        }

        [Test]
        public void Test6()
        {
            var sampleArgs = new SampleProgressEventArgs(68, false, HawkeyeError.eSuccess);

            var map = Mapper.Map<DeleteSampleResultsArgs>(sampleArgs);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleArgs.PercentComplete, map.PercentComplete);
            //Assert.AreEqual(sampleArgs.OperationIsComplete, map.OperationIsComplete);
            //Assert.AreEqual(sampleArgs.Error == HawkeyeError.eSuccess, map.Success);
        }
    }
}