using System;
using System.Collections.Generic;
using GrpcService;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutUtilities.Enums;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class CellTypeDomainToCellTypeTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void CellTypeDomainToCellTypeTest_CellTypeName()
        {
            CellTypeDomain cellType = new CellTypeDomain();
            CellType map = new CellType();

            PropertyReflectionTest(cellType, map, "My cell type name", 
                nameof(CellTypeDomain.CellTypeName));
            PropertyReflectionTest(cellType, map, string.Empty, 
                nameof(CellTypeDomain.CellTypeName));
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_MinDiameter()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType = new CellTypeDomain { MinimumDiameter = 2654.7845 };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.MinimumDiameter, map.MinDiameter);

            cellType = new CellTypeDomain { MinimumDiameter = 56.123 };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.MinimumDiameter, map.MinDiameter);

            cellType = new CellTypeDomain { MinimumDiameter = null };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(double), map.MinDiameter);
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_MaxDiameter()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType = new CellTypeDomain { MaximumDiameter = 2654.7845 };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.MaximumDiameter, map.MaxDiameter);

            cellType = new CellTypeDomain { MaximumDiameter = 56.123 };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.MaximumDiameter, map.MaxDiameter);

            cellType = new CellTypeDomain { MaximumDiameter = null };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(double), map.MaxDiameter);
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_NumImages()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType = new CellTypeDomain {Images = 2};
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.Images, map.NumImages);

            cellType = new CellTypeDomain {Images = 56};
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.Images, map.NumImages);

            cellType = new CellTypeDomain {Images = null};
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(uint), map.NumImages);
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_CellSharpness()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType = new CellTypeDomain { CellSharpness = 2.2545f };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.CellSharpness, map.CellSharpness);

            cellType = new CellTypeDomain { CellSharpness = 987.654f };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.CellSharpness, map.CellSharpness);

            cellType = new CellTypeDomain { CellSharpness = null };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(float), map.CellSharpness);
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_MinimumCircularity()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType = new CellTypeDomain { MinimumCircularity = 2.2545d };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.MinimumCircularity, map.MinCircularity);

            cellType = new CellTypeDomain { MinimumCircularity = 987.654f };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.MinimumCircularity, map.MinCircularity);

            cellType = new CellTypeDomain { MinimumCircularity = null };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(float), map.MinCircularity);
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_DeclusterDegree()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            PropertyReflectionTest(cellType, map, eCellDeclusterSetting.eDCHigh,
                nameof(CellTypeDomain.DeclusterDegree), nameof(CellType.DeclusterDegree));
            PropertyReflectionTest(cellType, map, eCellDeclusterSetting.eDCLow,
                nameof(CellTypeDomain.DeclusterDegree), nameof(CellType.DeclusterDegree));
            PropertyReflectionTest(cellType, map, eCellDeclusterSetting.eDCMedium,
                nameof(CellTypeDomain.DeclusterDegree), nameof(CellType.DeclusterDegree));
            PropertyReflectionTest(cellType, map, eCellDeclusterSetting.eDCNone,
                nameof(CellTypeDomain.DeclusterDegree), nameof(CellType.DeclusterDegree));
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_AspirationCycles()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            PropertyReflectionTest(cellType, map, 0,
                nameof(CellTypeDomain.AspirationCycles), nameof(CellType.NumAspirationCycles));
            PropertyReflectionTest(cellType, map, 23,
                nameof(CellTypeDomain.AspirationCycles), nameof(CellType.NumAspirationCycles));
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_CalculationAdjustmentFactor()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType = new CellTypeDomain { CalculationAdjustmentFactor = 2.2545f };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.CalculationAdjustmentFactor, map.ConcentrationAdjustmentFactor);

            cellType = new CellTypeDomain { CalculationAdjustmentFactor = -9.654f };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.CalculationAdjustmentFactor, map.ConcentrationAdjustmentFactor);

            cellType = new CellTypeDomain { CalculationAdjustmentFactor = 0f };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.CalculationAdjustmentFactor, map.ConcentrationAdjustmentFactor);

            cellType = new CellTypeDomain { CalculationAdjustmentFactor = null };
            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(float), map.ConcentrationAdjustmentFactor);
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_ViableSpotArea()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType.AnalysisDomain = new AnalysisDomain();
            cellType.AnalysisDomain.AnalysisParameter = new List<AnalysisParameterDomain>();
            cellType.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = 1.99f });
            cellType.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = 2.01f });

            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.AnalysisDomain.AnalysisParameter[0].ThresholdValue, map.ViableSpotArea);
        }

        [Test]
        public void CellTypeDomainToCellTypeTest_ViableSpotBrightness()
        {
            var cellType = new CellTypeDomain();
            var map = new CellType();

            cellType.AnalysisDomain = new AnalysisDomain();
            cellType.AnalysisDomain.AnalysisParameter = new List<AnalysisParameterDomain>();
            cellType.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = 1.234f });
            cellType.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = 5.678f });

            map = Mapper.Map<CellType>(cellType);
            Assert.IsNotNull(map);
            Assert.AreEqual(cellType.AnalysisDomain.AnalysisParameter[1].ThresholdValue, map.ViableSpotBrightness);
        }
    }
}