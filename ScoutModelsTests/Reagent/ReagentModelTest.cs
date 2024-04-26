using System.Runtime.InteropServices;
using ApiProxies;
using Moq;
using NUnit.Framework;
using ScoutModels;
using ScoutUtilities.Enums;

namespace ScoutModelsTests.Reagent
{
    [TestFixture]
    public class ReagentModelTest
    {
        private ApiEventBroker _broker;

        [SetUp]
        public void SetUp()
        {
            _broker = new ApiEventBroker();
        }

        [TearDown]
        public void TearDown()
        {
            _broker = null;
        }
        
        [Test]
        public void Constructor_ShouldInitializeAnInstance()
        {
            //Arrange //Act
            var reagentModel = new ReagentModel();

            //Assert
            Assert.IsNotNull(reagentModel);
        }

        [Test]
        [TestCase(ReagentContainerStatus.eEmpty)]
        [TestCase(ReagentContainerStatus.eExpired)]
        [TestCase(ReagentContainerStatus.eFaulted)]
        [TestCase(ReagentContainerStatus.eInvalid)]
        [TestCase(ReagentContainerStatus.eLoading)]
        [TestCase(ReagentContainerStatus.eNotDetected)]
        [TestCase(ReagentContainerStatus.eOK)]
        [TestCase(ReagentContainerStatus.eUnloaded)]
        [TestCase(ReagentContainerStatus.eUnloading)]
        public void ReagentHealth_ShouldSetAndGetReagentHealth(ReagentContainerStatus reagentStatus)
        {
            //Arrange
            var reagentModel = new ReagentModel();

            //Act
            reagentModel.ReagentHealth = reagentStatus;

            //Assert
            Assert.AreEqual(reagentStatus, reagentModel.ReagentHealth);
        }

        [Test]
        public void PartNumbers_ShouldSetAndGetPartNumbers()
        {
            //Arrange 
            var reagentModel = new ReagentModel();
            var expectedPartNumbers = "93jdh3";

            //Act
            reagentModel.PartNumber = expectedPartNumbers;

            //Assert
            Assert.AreEqual(expectedPartNumbers, reagentModel.PartNumber);
        }

        [Test]
        public void ProgressIndicator_ShouldSetAndGetProgressIndicator()
        {
            //Arrange 
            var reagentModel = new ReagentModel();
            var expectedProgressIndicator = "Test in progress";

            //Act
            reagentModel.ProgressIndicator = expectedProgressIndicator;

            //Assert
            Assert.AreEqual(expectedProgressIndicator, reagentModel.ProgressIndicator);
        }

        [Test]
        public void EventsRemaining_ShouldSetAndGetEventsRemaining()
        {
            //Arrange 
            var reagentModel = new ReagentModel();
            var expectedEventsRemaining = "30 events remaining";

            //Act
            reagentModel.EventsRemaining = expectedEventsRemaining;

            //Assert
            Assert.AreEqual(expectedEventsRemaining, reagentModel.EventsRemaining);
        }

        [Test]
        public void ReagentContainerStatusAsStr_ShouldSetAndGetReagentContainerStatusAsStr()
        {
            //Arrange 
            var reagentModel = new ReagentModel();
            var expectedReagentContainerStatus = "Empty";

            //Act
            reagentModel.ReagentContainerStatusAsStr = expectedReagentContainerStatus;

            //Assert
            Assert.AreEqual(expectedReagentContainerStatus, reagentModel.ReagentContainerStatusAsStr);
        }

        [Test]
        public void ReagentContainers_shouldGetNewInstanceOfReagentContainersIfReagentContainersIsNull()
        {
            //Arrange
            var reagentModel = new ReagentModel();

            //Act Assert
            Assert.IsNotNull(reagentModel.ReagentContainers);
        }
    }
}
