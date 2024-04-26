using AutoMapper;
using GrpcServer;
using GrpcService;
using Ninject;
using NUnit.Framework;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutServices.Ninject;
using ScoutUI;
using ScoutUtilities.Common;
using SubstrateType = ScoutUtilities.Enums.SubstrateType;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class SamplePositionAutoMapperTests
    {
        private IKernel _kernel;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel(new ScoutServiceModule(), new OpcUaGrpcModule());
            _mapper = _kernel.Get<IMapper>();
            Assert.IsNotNull(_mapper);
        }

        [Test]
        public void SamplePositionAutoMapperTest()
        {
            var samplePosition = new ScoutUtilities.Common.SamplePosition('Y', 1);
            var map = new GrpcService.SamplePosition();

            map = _mapper.Map<GrpcService.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(ApplicationConstants.ACupLabel, map.Row);
            Assert.AreEqual(samplePosition.Column, map.Column);

            samplePosition = new ScoutUtilities.Common.SamplePosition('Z', 22);
            map = _mapper.Map<GrpcService.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(ApplicationConstants.CarouselLabel, map.Row);
            Assert.AreEqual(samplePosition.Column, map.Column);

            samplePosition = new ScoutUtilities.Common.SamplePosition();
            map = _mapper.Map<GrpcService.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(default(char).ToString(), map.Row);
            Assert.AreEqual(default(uint), map.Column);
        }

        [Test]
        public void SamplePositionAutoMapperTest2()
        {
            var samplePosition = new GrpcService.SamplePosition {Row = "Y", Column = 1};
            var map = new ScoutUtilities.Common.SamplePosition();

            map = _mapper.Map<ScoutUtilities.Common.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(samplePosition.Row, map.Row.ToString());
            Assert.AreEqual(samplePosition.Column, map.Column);

            samplePosition = new GrpcService.SamplePosition { Row = "F", Column = 12 };
            map = _mapper.Map<ScoutUtilities.Common.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(samplePosition.Row, map.Row.ToString());
            Assert.AreEqual(samplePosition.Column, map.Column);

            samplePosition = new GrpcService.SamplePosition();
            map = _mapper.Map<ScoutUtilities.Common.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(default(char).ToString(), map.Row.ToString());
            Assert.AreEqual(default(uint), map.Column);
        }

        [Test]
        public void SamplePositionAutoMapperTest_Cases_1()
        {
            var samplePosition = new ScoutUtilities.Common.SamplePosition('y', 1);
            var map = new GrpcService.SamplePosition();

            map = _mapper.Map<GrpcService.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(samplePosition.Row.ToString().ToUpper(), map.Row);
            Assert.AreEqual(samplePosition.Column, map.Column);

            samplePosition = new ScoutUtilities.Common.SamplePosition('z', 22);
            map = _mapper.Map<GrpcService.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(samplePosition.Row.ToString().ToUpper(), map.Row);
            Assert.AreEqual(samplePosition.Column, map.Column);

            samplePosition = new ScoutUtilities.Common.SamplePosition();
            map = _mapper.Map<GrpcService.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(default(char).ToString(), map.Row);
            Assert.AreEqual(default(uint), map.Column);
        }

        [Test]
        public void SamplePositionAutoMapperTest_Cases_2()
        {
            var samplePosition = new GrpcService.SamplePosition {Row = "x", Column = 1};
            var map = new ScoutUtilities.Common.SamplePosition();

            map = _mapper.Map<ScoutUtilities.Common.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(samplePosition.Column, map.Column);
            Assert.AreEqual(samplePosition.Row.ToUpper(), map.Row.ToString());

            samplePosition = new GrpcService.SamplePosition { Row = "f", Column = 12 };
            map = _mapper.Map<ScoutUtilities.Common.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(samplePosition.Row.ToUpper(), map.Row.ToString());
            Assert.AreEqual(samplePosition.Column, map.Column);

            samplePosition = new GrpcService.SamplePosition();
            map = _mapper.Map<ScoutUtilities.Common.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(default(char).ToString(), map.Row.ToString());
            Assert.AreEqual(default(uint), map.Column);
        }

        [TestCase("A", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("A", (uint) 12, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("A", (uint) 13, ExpectedResult = SubstrateType.NoType)]
        [TestCase("B", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("C", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("D", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("E", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("f", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("g", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("h", (uint) 1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("h", (uint) 12, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("h", (uint) 13, ExpectedResult = SubstrateType.NoType)]
        [TestCase("i", (uint) 1, ExpectedResult = SubstrateType.NoType)]
        [TestCase("j", (uint) 1, ExpectedResult = SubstrateType.NoType)]
        [TestCase("x", (uint) 1, ExpectedResult = SubstrateType.NoType)]
        [TestCase("y", (uint) 1, ExpectedResult = SubstrateType.AutomationCup)]
        [TestCase("y", (uint) 2, ExpectedResult = SubstrateType.NoType)]
        [TestCase("z", (uint) 1, ExpectedResult = SubstrateType.Carousel)]
        [TestCase("z", (uint) 24, ExpectedResult = SubstrateType.Carousel)]
        [TestCase("z", (uint) 25, ExpectedResult = SubstrateType.NoType)]
        public SubstrateType SamplePositionToSubstrateTests(string row, uint column)
        {
            var samplePosition = new GrpcService.SamplePosition {Row = row, Column = column};
            var map = _mapper.Map<ScoutUtilities.Common.SamplePosition>(samplePosition);

            Assert.IsNotNull(map);
            Assert.AreEqual(column, map.Column);
            Assert.AreEqual(row.ToUpper(), map.Row.ToString());
            return map.GetSubstrateType();
        }
    }
}