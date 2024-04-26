using System;
using GrpcService;
using NUnit.Framework;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Common;
using ScoutUtilities.Structs;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class SampleEswDomainToSampleConfigTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void SampleEswDomainToSampleConfigTests_SampleName()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            PropertyReflectionTest(sample, map, "my sample name", 
                nameof(SampleEswDomain.SampleName), nameof(SampleConfig.SampleName));
            PropertyReflectionTest(sample, map, string.Empty, 
                nameof(SampleEswDomain.SampleName), nameof(SampleConfig.SampleName));
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_SampleTag()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            PropertyReflectionTest(sample, map, "my sample tag",
                nameof(SampleEswDomain.SampleTag), nameof(SampleConfig.Tag));
            PropertyReflectionTest(sample, map, string.Empty,
                nameof(SampleEswDomain.SampleTag), nameof(SampleConfig.Tag));
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_Uuid()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            sample = new SampleEswDomain {Uuid = new uuidDLL(Guid.NewGuid())};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(sample.Uuid.ToString(), map.SampleUuid);

            sample = new SampleEswDomain {Uuid = new uuidDLL(Guid.Empty)};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(Guid.Empty.ToString(), map.SampleUuid);
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_SamplePosition()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            sample = new SampleEswDomain {SamplePosition = new ScoutUtilities.Common.SamplePosition('Y', 1)};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(ApplicationConstants.ACupLabel, map.SamplePosition.Row);
            Assert.AreEqual((uint) sample.SamplePosition.Column, map.SamplePosition.Column);

            sample = new SampleEswDomain { SamplePosition = new ScoutUtilities.Common.SamplePosition('Z', 13) };
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(ApplicationConstants.CarouselLabel, map.SamplePosition.Row);
            Assert.AreEqual((uint)sample.SamplePosition.Column, map.SamplePosition.Column);

            sample = new SampleEswDomain { SamplePosition = new ScoutUtilities.Common.SamplePosition() };
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(char).ToString(), map.SamplePosition.Row);
            Assert.AreEqual(default(uint), map.SamplePosition.Column);
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_CellType()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            sample = new SampleEswDomain {CellTypeQcName = "Cell Type Name", IsQualityControl = false};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(sample.CellTypeQcName, map.CellType.CellTypeName);

            sample = new SampleEswDomain { CellTypeQcName = string.Empty, CellTypeIndex = 32, IsQualityControl = false };
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.CellType.CellTypeName);

            sample = new SampleEswDomain{ IsQualityControl = true };
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.CellType.CellTypeName);

            sample = new SampleEswDomain();
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.CellType.CellTypeName);

            sample = new SampleEswDomain{ CellTypeIndex = 2 };
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.CellType.CellTypeName);
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_QualityControl()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            sample = new SampleEswDomain {CellTypeQcName = "Cell Type Name", IsQualityControl = true};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(sample.CellTypeQcName, map.QualityControl.QualityControlName);

            sample = new SampleEswDomain { CellTypeQcName = string.Empty, CellTypeIndex = 32, IsQualityControl = true };
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.QualityControl.QualityControlName);

            sample = new SampleEswDomain {IsQualityControl = false};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(null, map.QualityControl);

            sample = new SampleEswDomain();
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(null, map.QualityControl);

            sample = new SampleEswDomain {IsQualityControl = true};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.QualityControl.QualityControlName);

            sample = new SampleEswDomain{ CellTypeIndex = 2, IsQualityControl = true};
            map = Mapper.Map<SampleConfig>(sample);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.QualityControl.QualityControlName);
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_Dilution()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            PropertyReflectionTest(sample, map, (uint)123,
                nameof(SampleEswDomain.Dilution), nameof(SampleConfig.Dilution));
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_WashType()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SamplePostWash.FastWash,
                nameof(SampleEswDomain.WashType), nameof(SampleConfig.WashType));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SamplePostWash.NormalWash,
                nameof(SampleEswDomain.WashType), nameof(SampleConfig.WashType));
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_SaveEveryNthImage()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            PropertyReflectionTest(sample, map, (uint)3,
                nameof(SampleEswDomain.SaveEveryNthImage), nameof(SampleConfig.SaveEveryNthImage));
            PropertyReflectionTest(sample, map, (uint)45,
                nameof(SampleEswDomain.SaveEveryNthImage), nameof(SampleConfig.SaveEveryNthImage));
        }

        [Test]
        public void SampleEswDomainToSampleConfigTests_SampleStatus()
        {
            var sample = new SampleEswDomain();
            var map = new SampleConfig();

            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.SkipError,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.AcquisitionComplete,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.Completed,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.InProcessAspirating,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.InProcessCleaning,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.InProcessImageAcquisition,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.InProcessMixing,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.NotProcessed,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
            PropertyReflectionTest(sample, map, ScoutUtilities.Enums.SampleStatus.SkipManual,
                nameof(SampleEswDomain.SampleStatus), nameof(SampleConfig.SampleStatus));
        }
    }
}