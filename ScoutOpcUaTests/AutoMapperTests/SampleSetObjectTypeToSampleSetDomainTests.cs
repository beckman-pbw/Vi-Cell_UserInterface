using System;
using GrpcService;
using NUnit.Framework;
using ScoutDomains.EnhancedSampleWorkflow;

namespace ScoutOpcUaTests
{
    public class SampleSetConfigToSampleSetDomainTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void SampleSetTest_SetName()
        {
            var source = new SampleSetConfig();
            var map = new SampleSetDomain();

            source = new SampleSetConfig
            {
                SampleSetName = "My Set name goes here"
            };
            map = Mapper.Map<SampleSetDomain>(source);

            Assert.IsNotNull(map);
            Assert.AreEqual(source.SampleSetName, map.SampleSetName);

            source = new SampleSetConfig
            {
                SampleSetName = string.Empty
            };
            map = Mapper.Map<SampleSetDomain>(source);

            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.SampleSetName);
        }

        [Test]
        public void SampleSetTest_SetUuid()
        {
            var source = new SampleSetConfig();
            var map = new SampleSetDomain();

            source = new SampleSetConfig
            {
                SampleSetUuid = Guid.NewGuid().ToString()
            };
            map = Mapper.Map<SampleSetDomain>(source);

            Assert.IsNotNull(map);
            Assert.AreEqual(source.SampleSetUuid.ToUpper(), map.Uuid.ToString());

            source = new SampleSetConfig
            {
                SampleSetUuid = string.Empty
            };
            map = Mapper.Map<SampleSetDomain>(source);

            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.SampleSetName);
        }

        [Test]
        public void SampleSetTest_SetPlatePrecession()
        {
            var source = new SampleSetConfig();
            var map = new SampleSetDomain();

            source = new SampleSetConfig
            {
                PlatePrecession = PrecessionEnum.ColumnMajor
            };
            map = Mapper.Map<SampleSetDomain>(source);

            Assert.IsNotNull(map);
            Assert.AreEqual(ScoutUtilities.Enums.Precession.ColumnMajor, map.PlatePrecession);
        }

        [Test]
        public void SampleSetTest_Samples()
        {
            var source = new SampleSetConfig();
            var map = new SampleSetDomain();

            source = new SampleSetConfig
            {
                Samples =
                {
                    new SampleConfig
                    {
                        SampleName = "sample 1",
                        CellType = new CellType{CellTypeName = "Yeast"},
                        Dilution = 123,
                        SamplePosition = new SamplePosition{Row = "B", Column = 2},
                        SaveEveryNthImage = 5,
                        Tag = "my tag 1"
                    },
                    new SampleConfig
                    {
                        SampleName = "sample 2",
                        QualityControl = new QualityControl
                        {
                            QualityControlName = "My Cool QC",
                            CellTypeName = "Insect"
                        },
                        Dilution = 321,
                        SamplePosition = new SamplePosition{Row = "G", Column = 10},
                        SaveEveryNthImage = 14,
                        Tag = "my tag 2"
                    }
                }
            };
            map = Mapper.Map<SampleSetDomain>(source);

            Assert.IsNotNull(map);
            Assert.AreEqual(2, source.Samples.Count);
            Assert.AreEqual((uint)SubstrateTypeEnum.Plate96, (uint)map.Carrier);

            Assert.AreEqual(source.Samples[0].SampleName, map.Samples[0].SampleName);
            Assert.AreEqual(false, map.Samples[0].IsQualityControl);
            Assert.AreEqual(source.Samples[0].CellType.CellTypeName, map.Samples[0].CellTypeQcName);
            Assert.AreEqual(source.Samples[0].Dilution, map.Samples[0].Dilution);
            Assert.AreEqual(source.Samples[0].SamplePosition.Row, map.Samples[0].SamplePosition.Row.ToString());
            Assert.AreEqual(source.Samples[0].SamplePosition.Column, map.Samples[0].SamplePosition.Column);
            Assert.AreEqual(source.Samples[0].SaveEveryNthImage, map.Samples[0].SaveEveryNthImage);
            Assert.AreEqual(source.Samples[0].Tag, map.Samples[0].SampleTag);

            Assert.AreEqual(source.Samples[1].SampleName, map.Samples[1].SampleName);
            Assert.AreEqual(source.Samples[1].QualityControl.QualityControlName, map.Samples[1].CellTypeQcName);
            Assert.AreEqual(true, map.Samples[1].IsQualityControl);
            Assert.AreEqual(source.Samples[1].Dilution, map.Samples[1].Dilution);
            Assert.AreEqual(source.Samples[1].SamplePosition.Row, map.Samples[1].SamplePosition.Row.ToString());
            Assert.AreEqual(source.Samples[1].SamplePosition.Column, map.Samples[1].SamplePosition.Column);
            Assert.AreEqual(source.Samples[1].SaveEveryNthImage, map.Samples[1].SaveEveryNthImage);
            Assert.AreEqual(source.Samples[1].Tag, map.Samples[1].SampleTag);
        }
    }
}