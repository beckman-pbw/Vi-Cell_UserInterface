using AutoMapper;
using GrpcServer;
using GrpcService;
using Ninject;
using NUnit.Framework;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutServices.Ninject;
using System;
using SampleStatus = GrpcService.SampleStatusEnum;
using SubstrateType = ScoutUtilities.Enums.SubstrateType;

namespace ScoutOpcUaTests
{
    public class SampleObjectAutoMapperTest
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
        public void SampleObjectAutoMapperTest_CellType()
        {
            var cellType = new CellType();
            cellType.CellTypeName = "Insect";

            var sampleObject = new SampleConfig();
            sampleObject.CellType = cellType;
            var map = new SampleEswDomain();

            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(false, map.IsQualityControl);
            Assert.AreEqual(cellType.CellTypeName, map.CellTypeQcName);

            cellType.CellTypeName = "Yeast";

            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(false, map.IsQualityControl);
            Assert.AreEqual(cellType.CellTypeName, map.CellTypeQcName);
        }

        [Test]
        public void SampleObjectAutoMapperTest_QualityControl()
        {
            // test when sample only has a QC attached (QC does not includes cell type subobject)
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            var cellType = new CellType();
            cellType.CellTypeName = "Insect";

            var qc = new QualityControl();
            qc.QualityControlName = "QC Name";
            qc.CellTypeName = cellType.CellTypeName;
            
            sampleObject.QualityControl = qc;

            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(true, map.IsQualityControl);
            Assert.AreEqual(qc.QualityControlName, map.CellTypeQcName);

            // test when sample only has a QC attached (QC includes cell type subobject)
            qc.CellTypeName = cellType.CellTypeName;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(true, map.IsQualityControl);
            Assert.AreEqual(qc.QualityControlName, map.CellTypeQcName);

            cellType.CellTypeName = "Yeast";
            qc.CellTypeName = cellType.CellTypeName;
            qc.QualityControlName = "QC Name AGAIN";
            qc.CellTypeName = cellType.CellTypeName;

            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(true, map.IsQualityControl);
            Assert.AreEqual(qc.QualityControlName, map.CellTypeQcName);

            // test when sample has a QC AND celltype attached (QC includes cell type subobject)
            sampleObject.QualityControl = qc;
            sampleObject.CellType = cellType;
            
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(true, map.IsQualityControl);
            Assert.AreEqual(qc.QualityControlName, map.CellTypeQcName);

            cellType.CellTypeName = "Yeast";
            qc.CellTypeName = cellType.CellTypeName;
            qc.QualityControlName = "QC Name AGAIN";
            qc.CellTypeName = cellType.CellTypeName;

            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(true, map.IsQualityControl);
            Assert.AreEqual(qc.QualityControlName, map.CellTypeQcName);
        }

        [Test]
        public void SampleObjectAutoMapperTest_Dilution()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();
            
            sampleObject.Dilution = 14;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.Dilution, map.Dilution);

            sampleObject.Dilution = 351;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.Dilution, map.Dilution);

            sampleObject.Dilution = 998;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.Dilution, map.Dilution);
        }

        [Test]
        public void SampleObjectAutoMapperTest_SampleName()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            sampleObject.SampleName = "My Sample Name";
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SampleName, map.SampleName);

            sampleObject.SampleName = "My Sample Name 2: Name Harder";
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SampleName, map.SampleName);
        }

        [Test]
        public void SampleObjectAutoMapperTest_SamplePosition()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            sampleObject.SamplePosition = new GrpcService.SamplePosition {Row = "A", Column = 1};
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SamplePosition.Row, map.SamplePosition.Row.ToString());
            Assert.AreEqual(sampleObject.SamplePosition.Column, map.SamplePosition.Column);

            sampleObject.SamplePosition = new GrpcService.SamplePosition { Row = "B", Column = 2 };
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SamplePosition.Row, map.SamplePosition.Row.ToString());
            Assert.AreEqual(sampleObject.SamplePosition.Column, map.SamplePosition.Column);

            sampleObject.SamplePosition = new GrpcService.SamplePosition { Row = "Y", Column = 1 };
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SamplePosition.Row, map.SamplePosition.Row.ToString());
            Assert.AreEqual(sampleObject.SamplePosition.Column, map.SamplePosition.Column);

            sampleObject.SamplePosition = new GrpcService.SamplePosition { Row = "Z", Column = 21 };
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SamplePosition.Row, map.SamplePosition.Row.ToString());
            Assert.AreEqual(sampleObject.SamplePosition.Column, map.SamplePosition.Column);
        }

        [TestCase("A", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("A", (uint)12, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("A", (uint)13, ExpectedResult = SubstrateType.NoType)]
        [TestCase("B", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("C", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("D", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("E", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("f", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("g", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("h", (uint)1, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("h", (uint)12, ExpectedResult = SubstrateType.Plate96)]
        [TestCase("h", (uint)13, ExpectedResult = SubstrateType.NoType)]
        [TestCase("i", (uint)1, ExpectedResult = SubstrateType.NoType)]
        [TestCase("j", (uint)1, ExpectedResult = SubstrateType.NoType)]
        [TestCase("x", (uint)1, ExpectedResult = SubstrateType.NoType)]
        [TestCase("y", (uint)1, ExpectedResult = SubstrateType.AutomationCup)]
        [TestCase("y", (uint)2, ExpectedResult = SubstrateType.NoType)]
        [TestCase("z", (uint)1, ExpectedResult = SubstrateType.Carousel)]
        [TestCase("z", (uint)24, ExpectedResult = SubstrateType.Carousel)]
        [TestCase("z", (uint)25, ExpectedResult = SubstrateType.NoType)]
        public SubstrateType SampleObjectAutoMapperTest_SubstrateType(string row, uint col)
        {
            var sampleObject = new SampleConfig
            {
                SamplePosition = new GrpcService.SamplePosition {Row = row, Column = col}
            };
            var map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(row.ToUpper(), map.SamplePosition.Row.ToString());
            Assert.AreEqual(col, (uint) map.SamplePosition.Column);
            return map.SubstrateType;
        }

        [Test]
        public void SampleObjectAutoMapperTest_SampleUuid()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            sampleObject.SampleUuid = Guid.NewGuid().ToString().ToUpper();
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SampleUuid, map.Uuid.ToString());

            sampleObject.SampleUuid = Guid.Empty.ToString().ToUpper();
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SampleUuid, map.Uuid.ToString());
        }

        [Test]
        public void SampleObjectAutoMapperTest_SaveEveryNthImage()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            sampleObject.SaveEveryNthImage = 1;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SaveEveryNthImage, map.SaveEveryNthImage);

            sampleObject.SaveEveryNthImage = 22;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.SaveEveryNthImage, map.SaveEveryNthImage);
        }

        [Test]
        public void SampleObjectAutoMapperTest_Tag()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            sampleObject.Tag = "My cool Tag!";
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.Tag, map.SampleTag);

            sampleObject.Tag = "Another cool Tag!";
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.Tag, map.SampleTag);
        }

        [Test]
        public void SampleObjectAutoMapperTest_WashType()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            sampleObject.WashType = WashTypeEnum.FastWashType;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual((ushort) sampleObject.WashType, (ushort) map.WashType);

            sampleObject.WashType = WashTypeEnum.NormalWashType;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual((ushort) sampleObject.WashType, (ushort) map.WashType);
        }

        [Test]
        public void SampleObjectAutoMapperTest_SampleStatus()
        {
            var sampleObject = new SampleConfig();
            var map = new SampleEswDomain();

            sampleObject.SampleStatus = SampleStatus.InProcessAspirating;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual((ushort) sampleObject.SampleStatus, (ushort) map.SampleStatus);

            sampleObject.SampleStatus = SampleStatus.SkipError;
            map = _mapper.Map<SampleEswDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual((ushort) sampleObject.SampleStatus, (ushort) map.SampleStatus);
        }

        [Test]
        public void SampleConfigToSampleEswTest_FailedOnceButShouldntHave()
        {
            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType
                {
                    CellTypeName = "Insect"
                },
                Dilution = 0, // the failing condition for this test
                SamplePosition = new SamplePosition {Row = "Y", Column = 1},
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WashType = WashTypeEnum.FastWashType
            };
            
            var map = _mapper.Map<SampleEswDomain>(sample);

            Assert.IsNotNull(map);
            Assert.AreEqual(sample.SampleName, map.SampleName);
            Assert.AreEqual(sample.CellType.CellTypeName, map.CellTypeQcName);
            Assert.AreEqual(sample.Dilution, map.Dilution);
            Assert.AreEqual(sample.SamplePosition.Column, map.SamplePosition.Column);
            Assert.AreEqual(sample.SamplePosition.Row, map.SamplePosition.Row.ToString());
            Assert.AreEqual(sample.SaveEveryNthImage, map.SaveEveryNthImage);
            Assert.AreEqual(sample.Tag, map.SampleTag);
            Assert.AreEqual((uint)sample.WashType, (uint)map.WashType);
        }
    }
}