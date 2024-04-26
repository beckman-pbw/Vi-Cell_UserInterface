using NUnit.Framework;
using ScoutDomains.Analysis;
using ScoutDomains.RunResult;
using ScoutUtilities.Common;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class SampleRecordDomainToSampleConfigTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void SampleRecordDomainToSampleConfigTests_Tag()
        {
            var sampleRecord = new ScoutDomains.SampleRecordDomain();
            var map = new GrpcService.SampleConfig();

            PropertyReflectionTest(sampleRecord, map, "My tag here",
                nameof(ScoutDomains.SampleRecordDomain.Tag), nameof(GrpcService.SampleConfig.Tag));
            PropertyReflectionTest(sampleRecord, map, string.Empty,
                nameof(ScoutDomains.SampleRecordDomain.Tag), nameof(GrpcService.SampleConfig.Tag));
        }

        [Test]
        public void SampleRecordDomainToSampleConfigTests_Position()
        {
            var sample = new ScoutDomains.SampleRecordDomain();
            var map = new GrpcService.SampleConfig();

            sample = new ScoutDomains.SampleRecordDomain { Position = new SamplePosition('Y', 1) };
            map = Mapper.Map<GrpcService.SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) sample.Position.Column, map.SamplePosition.Column);
            Assert.AreEqual(ApplicationConstants.ACupLabel, map.SamplePosition.Row);

            sample = new ScoutDomains.SampleRecordDomain { Position = new SamplePosition('Z', 21) };
            map = Mapper.Map<GrpcService.SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint)sample.Position.Column, map.SamplePosition.Column);
            Assert.AreEqual(ApplicationConstants.CarouselLabel, map.SamplePosition.Row);

            sample = new ScoutDomains.SampleRecordDomain();
            map = Mapper.Map<GrpcService.SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(uint), map.SamplePosition.Column);
            Assert.AreEqual(default(char).ToString(), map.SamplePosition.Row);
        }

        [Test]
        public void SampleRecordDomainToSampleConfigTests_CellType()
        {
            var sample = new ScoutDomains.SampleRecordDomain();
            var map = new GrpcService.SampleConfig();

            sample = new ScoutDomains.SampleRecordDomain
            {
                SelectedResultSummary = new ResultSummaryDomain
                {
                    CellTypeDomain = new CellTypeDomain
                    {
                        CellTypeIndex = 2,
                        CellTypeName = "Yeast"
                    }
                }
            };

            map = Mapper.Map<GrpcService.SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(sample.SelectedResultSummary.CellTypeDomain.CellTypeName,
                map.CellType.CellTypeName);

            sample = new ScoutDomains.SampleRecordDomain();
            map = Mapper.Map<GrpcService.SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(null, map.CellType);
        }
    }
}