using System;
using GrpcService;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Structs;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class SampleEswDomainToSampleResultTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void SampleEswDomainToSampleResult_SampleDataUuid()
        {
            var sampleDomain = new SampleEswDomain { SampleDataUuid = new uuidDLL(Guid.NewGuid()) };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleDataUuid.ToString(), map.SampleDataUuid);
        }

        [Test]
        public void SampleEswDomainToSampleResult_SampleId()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain { SampleIdentifier = "TestSample!!" };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SampleIdentifier, map.SampleId);
        }

        [Test]
        public void SampleEswDomainToSampleResult_SampleStatus()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain { SampleCompletionStatus = ScoutUtilities.Enums.sample_completion_status.sample_not_run };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(SampleStatusEnum.NotProcessed, map.Status);

            sampleDomain.SampleRecord.SampleCompletionStatus = ScoutUtilities.Enums.sample_completion_status.sample_completed;
            map = Mapper.Map<SampleResult>(sampleDomain);
            Assert.AreEqual(SampleStatusEnum.Completed, map.Status);
        }

        [Test]
        public void SampleEswDomainToSampleResult_CellCount()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { TotalCells = 9999 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.TotalCells, map.CellCount);
        }

        [Test]
        public void SampleEswDomainToSampleResult_ViableCells()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { ViableCells = 590 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.ViableCells, map.ViableCells);
        }

        [Test]
        public void SampleEswDomainToSampleResult_TotalCellsPerML()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { ConcentrationML = 590 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.ConcentrationML, map.TotalCellsPerMilliliter);
        }

        [Test]
        public void SampleEswDomainToSampleResult_ViableCellsPerML()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { ViableConcentration = 700 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.ViableConcentration, map.ViableCellsPerMilliliter);
        }

        [Test]
        public void SampleEswDomainToSampleResult_ViabilityPercent()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { Viability = 99 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.Viability, map.ViabilityPercent);
        }

        [Test]
        public void SampleEswDomainToSampleResult_AverageDiameter()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { Size = 70 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.Size, map.AverageDiameter);
        }

        [Test]
        public void SampleEswDomainToSampleResult_AverageViableDiameter()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { ViableSize = 79 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.ViableSize, map.AverageViableDiameter);
        }

        [Test]
        public void SampleEswDomainToSampleResult_AverageCircularity()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { Circularity = .8 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.Circularity, map.AverageCircularity);
        }

        [Test]
        public void SampleEswDomainToSampleResult_AverageViableCircularity()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { ViableCircularity = .99 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.ViableCircularity, map.AverageViableCircularity);
        }

        [Test]
        public void SampleEswDomainToSampleResult_AverageCellsPerImage()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { AverageCellsPerImage = 256 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.AverageCellsPerImage, map.AverageCellsPerImage);
        }

        [Test]
        public void SampleEswDomainToSampleResult_AverageBackgroundIntensity()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { AvgBackground = 7 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.AvgBackground, map.AverageBackgroundIntensity);
        }

        [Test]
        public void SampleEswDomainToSampleResult_BubbleCount()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { Bubble = 3 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.Bubble, map.BubbleCount);
        }

        [Test]
        public void SampleEswDomainToSampleResult_ClusterCount()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult = new ScoutDomains.BasicResultDomain { ClusterCount = 109 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CumulativeResult.ClusterCount, map.ClusterCount);
        }

        [Test]
        public void SampleEswDomainToSampleResult_ImagesForAnalysis()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.ImageSetIds = new uuidDLL [] { new uuidDLL() };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(1, map.ImagesForAnalysis);
            Assert.AreEqual(sampleDomain.SampleRecord.ImageSetIds.Length, map.ImagesForAnalysis);
        }

        [Test]
        public void SampleEswDomainToSampleResult_QualityControlName()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(String.Empty, map.QualityControlName);

            sampleDomain.SampleRecord.BpQcName = "I am a QC!";
            map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.BpQcName, map.QualityControlName);
        }

        [Test]
        public void SampleEswDomainToSampleResult_CellType()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain { CellTypeName = "Test Cell" };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.CellTypeName, map.CellType);
        }

        [Test]
        public void SampleEswDomainToSampleResult_Diameter()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain { MinimumDiameter = 12, MaximumDiameter = 99 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.MinimumDiameter, map.MinimumDiameter);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.MaximumDiameter, map.MaximumDiameter);
        }

        [Test]
        public void SampleEswDomainToSampleResult_Images()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain { Images = 100 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.Images, map.Images);
        }

        [Test]
        public void SampleEswDomainToSampleResult_CellSharpness()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain { CellSharpness = 7.2f };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.CellSharpness, map.CellSharpness);
        }

        [Test]
        public void SampleEswDomainToSampleResult_MinimumCircularity()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain { MinimumCircularity = .7f };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.MinimumCircularity, map.MinimumCircularity);
        }

        [Test]
        public void SampleEswDomainToSampleResult_DeclusterDegree()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain { DeclusterDegree = ScoutUtilities.Enums.eCellDeclusterSetting.eDCHigh};
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual((DeclusterDegreeEnum)sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.DeclusterDegree, map.DeclusterDegree);
        }

        [Test]
        public void SampleEswDomainToSampleResult_AspirationCycles()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain { AspirationCycles = 2 };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AspirationCycles, map.AspirationCycles);
        }

        [Test]
        public void SampleEswDomainToSampleResult_ViableSpotBrightnessAndArea()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain = new ScoutDomains.Analysis.AnalysisDomain();

            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter = new System.Collections.Generic.List<ScoutDomains.AnalysisParameterDomain>();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = 30 });
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter.Add(new ScoutDomains.AnalysisParameterDomain { ThresholdValue = 30 });
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter[1].ThresholdValue, map.ViableSpotBrightness);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.AnalysisParameter[0].ThresholdValue, map.ViableSpotArea);
        }

        [Test]
        public void SampleEswDomainToSampleResult_MixingCycle()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain = new ScoutDomains.Analysis.AnalysisDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.MixingCycle = 3;
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.AnalysisDomain.MixingCycle, map.MixingCycles);
        }
        [Test]
        public void SampleEswDomainToSampleResult_ConcentrationAdjustmentFactor()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            sampleDomain.SampleRecord.SelectedResultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain = new ScoutDomains.Analysis.CellTypeDomain();
            sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.CalculationAdjustmentFactor = 4.5f;
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.CellTypeDomain.CalculationAdjustmentFactor, map.ConcentrationAdjustmentFactor);
        }

        [Test]
        public void SampleEswDomainToSampleResult_Analysis()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            ScoutDomains.RunResult.ResultSummaryDomain resultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            resultSummary.RetrieveDate = DateTime.Now;
            resultSummary.UserId = "Fake_User";
            sampleDomain.SampleRecord.ResultSummaryList.Add(resultSummary);
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.RetrieveDate, map.AnalysisDateTime.ToDateTime());
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.UserId, map.AnalysisBy);
        }

        [Test]
        public void SampleEswDomainToSampleResult_Reanalysis()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain();
            ScoutDomains.RunResult.ResultSummaryDomain resultSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            resultSummary.RetrieveDate = DateTime.Now.AddDays(-5);
            resultSummary.UUID = new uuidDLL(Guid.NewGuid());

            ScoutDomains.RunResult.ResultSummaryDomain reanalyzedSummary = new ScoutDomains.RunResult.ResultSummaryDomain();
            reanalyzedSummary.RetrieveDate = DateTime.Now;
            reanalyzedSummary.UUID = new uuidDLL(Guid.NewGuid());
            reanalyzedSummary.UserId = "Fake_User";

            sampleDomain.SampleRecord.ResultSummaryList.Add(resultSummary);
            sampleDomain.SampleRecord.ResultSummaryList.Add(reanalyzedSummary);

            sampleDomain.SampleRecord.SelectedResultSummary = sampleDomain.SampleRecord.ResultSummaryList[1];
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.RetrieveDate, map.ReanalysisDateTime.ToDateTime());
            Assert.AreEqual(sampleDomain.SampleRecord.SelectedResultSummary.UserId, map.ReanalysisBy);
        }

        [Test]
        public void SampleEswDomainToSampleResult_Dilution()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain() { DilutionName = "5" };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(int.Parse(sampleDomain.SampleRecord.DilutionName), map.Dilution);
        }

        [Test]
        public void SampleEswDomainToSampleResult_WashType()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain { WashName = ScoutUtilities.Enums.SamplePostWash.FastWash };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual((WashTypeEnum)sampleDomain.SampleRecord.WashName, map.WashType);

            sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain { WashName = ScoutUtilities.Enums.SamplePostWash.FastWash };
            map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual((WashTypeEnum)sampleDomain.SampleRecord.WashName, map.WashType);
        }

        [Test]
        public void SampleEswDomainToSampleResult_Tag()
        {
            var sampleDomain = new SampleEswDomain();
            sampleDomain.SampleRecord = new ScoutDomains.SampleRecordDomain { Tag = "I'm a tag!" };
            var map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleDomain.SampleRecord.Tag, map.Tag);

            sampleDomain = new SampleEswDomain() { SampleTag = null };
            map = Mapper.Map<SampleResult>(sampleDomain);

            Assert.IsNotNull(map);
            Assert.AreEqual(String.Empty, map.Tag);
        }

        [Test]
        public void LookingForNullsTest()
        {
            var jsonSettings = new JsonSerializerSettings { Converters = new JsonConverter[] { new KeyValuePairConverter() } };
            var json = @"{'$type':'ScoutDomains.EnhancedSampleWorkflow.SampleEswDomain, ScoutDomains','Index':0,'SampleSetIndex':1,'TimeStamp':1617905826,'Uuid':{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'T8vBAC07TyaIRP74P+Qifw=='}},'SampleDataUuid':{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'keRdI5eAT4yiN6DhjyOeKg=='}},'SampleSetUid':{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'fHS0FP6FRkCvEVaZas7pgQ=='}},'SampleStatus':6,'SubstrateType':2,'PlatePrecession':0,'SampleName':'SampleJson','SampleTag':'TestJson','SamplePosition':{'$type':'ScoutUtilities.Common.SamplePosition, ScoutUtilities','Row':'A','Column':1},'IsQualityControl':false,'CellTypeIndex':3,'CellTypeQcName':'Yeast','Dilution':1,'WashType':0,'Username':'Vi-CELL','SaveEveryNthImage':1,'SampleRecord':{'$type':'ScoutDomains.SampleRecordDomain, ScoutDomains','IsSingleExportStatus':true,'UUID':{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'keRdI5eAT4yiN6DhjyOeKg=='}},'NumOfResultRecord':1,'ImageSet':null,'ImageSetIds':{'$type':'ScoutUtilities.Structs.uuidDLL[], ScoutUtilities','$values':[{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'wR/vNvRPShWxbSmF93JjOQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'ZOzAMi5pTWasOwa+GDfg8Q=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'fZ0VXtXrQaamiOfBytsb9w=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'/9YZzFjqS/2e+5N91PrJJw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'75ff4iBGTHChcSVqxrFMMw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'b5qW8uLgREqfe4g2E1wpWw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'CNVZg/XZRwCAICuurI+6fw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'vjwR0OIVSBi/bUjbBIF1HQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'o2zSM3W8TeOnqPWdc9ujNA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'8UWooPOOTGCyaZozI3zEdQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'9olWKUG1S0qhcQ94n6cIeA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'dd5r6oedQr+jscWckFalEQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'v/6pGWOmQ8Keow5U5bJHmA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'+1vOiEMBSOe7mPaXfhaH6w=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'GLNVv9UcTm6pK6HRMv+1Yg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'QuzmzMKsT82X8aQE9LQBMg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'pu58itwwQkW7ni8PWEfNHA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'FbfAwEYwT4WTBGN1pVBsSg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'qgd+gQygS3G49i6HKq8pMg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'wKIVoYMpQhybiUI11q6KxQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'+JZ58rtSQxGrlto4/Kt6Sw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'RPemBaqESJuavfjnGgJH2g=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'4IUN5BQeR2qLzqSTXWNF+g=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'6Fhws1Z3RJ+YRbkBtuk/Cg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'EAeK8MXyRHG2nDFRrdHuMA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'nfhx3+fQR2yqVdYJ2Jxt/A=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'RlQ0786ITnWbZUcLfnyDSg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'/16ww4nCT5Od6K7uJCEgEA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'9rAu180RR6KiOMZ0Wm+WMA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'vwDUI0eYQrih4+UVQ5IfVA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'Wil4yZPKT+KinL8jYV8BEg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'JdTRUtGGSYibDxKwToYGCw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'oH/bU1sNT1Kos2d8gMyXpw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'paK0FgOCRjOvbZ1tFjEiAw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'9CBnAF8MSOKQct5ZnkP6NA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'zMVjOFK0RPGx3MVzIPQCFQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'vxVOOa2KRAuBALQwTDWe4A=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'25i5OHqrSmKPde8nc59JrA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'SRB4xGltT3eZ8X6p3ywldg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'xX2xw1hKQTuN0ljraa2GIg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'2ttpwYRbQoGgVeOF/GWTrw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'kUaimuXSQbq4IGvdqraGIQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'Yr+51UBQQkaJFpUaCA4EBQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'Q+2C0B7ASbaGuuddfPus+A=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'Nz4EFSjcQ12Eb3tfgm2aoA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'NfI37YEMSw6/ujgBNVrBEw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'yDho0omyTCCpd1DPkUFnLA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'mC1fMbVQQTOiZSRPKAP/xg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'URgN+pzWSFKc0NERGUfwug=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'8thFe/wAR4WKi5eo9KcXFA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'g8RAiKsDQdy4FBcAMlCe+g=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'BqTiFMZmRhuay47AH7KBtA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'4+ibXzM6Ru22bug76ilTyw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'EAeQpmgGTOCJRC/nvZZQ5Q=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'rvM3nyD4T4ipuKdai5Q7HA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'J6amtAvbQWGIZO7yp//j0w=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'fhCE4OcZQ1yQ8/kZG7nusA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'v/VMm7MQRC2/+yFmlk6vMg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'MGXrrJrkRiuWnBYasoLuHQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'RSVEe7OfSdGVuw3Os7F2Ig=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'7O2pORCnRgydw5anGnOgnA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'WhzDutXDSb6cX9aDsVw/mg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'5THxUhI6RE6WDGMoq8/0qA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'kHSbXXsjRGuruKoaVOPyVw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'n7OpEaHFTf6JBsaZQ7/kHg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'9XTFaOY8TjGL+/eraLPuKQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'Jn3ab/cgQKyP2DUjeF/vbA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'dtci5ncERgi3Wj3vB75tog=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'HEQOiLX5Rua9qqjaR81vUA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'aRGhM6MSR62qmuqKTtQFwg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'7t7upoPnS32Uh3ICdRCZVA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'EW/ToacAT0iA5USS80Lq9A=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'tELGWqTURr6iM4Mk5sGzuA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'mjH29p4bSPySRp4xcGsiyw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'MnmnF+FFRDi02DtvheOd4A=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'Bm4W9RMiS4Wd59UliW3Urw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'rnFtMU+MRuizgTRdU/PUvw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'NwWp83AGSxe28Magp4ML0g=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'DxPJxlbuQsqQFMOQHy1hEQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'ZsCtunjwS9CX4TMBLyZOYw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'bX77CAwDRTKQVJzo8CmRTg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'NvjUx+3pSZmr7Fob6hdczg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'iO1ocyxLTRSa31RaVWAkhA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'4s0s2jAZSY2PRzdUG1r54g=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'l/lLbno6S5eHFVqESN2YZQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'XTHT/F+TQy2/FZdx4m4+Hg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'s0sVN3HTTqOlcTmYgh/xpg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'j9RJ1KzSTTuJAYakvCZ+UQ=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'0PeBH+KJRqi4msE3Hchp3A=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'vLeRzwuCR7y+WntLLV2Y5A=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'dVqBc2QhQLSu52tpM5bZuA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'WSkooDjwT/O9HXB3LAKEvg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'7p0SinWDQEq6LVsKd/oxbg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'OfKBNhOnS4eBFQ4tKtIRCw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'R4MlKeOkQKaAB17Zq78IIA=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'zvD7t0FhQm2MpVMxi7U0Zw=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'ek/pHSqASnan2qWBeUFkKg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'JSqBZMApQYK7/NBfmJTdTg=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'bBWFpRV5TIaZ7nhpHjnZ/g=='}},{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'cREeoUK4RTO86eXpoYq2Tw=='}}]},'ResultRecordIds':null,'SampleHierarchy':0,'IsSampleCompleted':true,'SelectedImageIndex':{'Key':0,'Value':null},'Tag':'TestJson','IsSelected':false,'NumImageSets':100,'HasValue':false,'Position':{'$type':'ScoutUtilities.Common.SamplePosition, ScoutUtilities','Row':'\u0000','Column':0},'SampleIdentifier':'SampleJson','UserId':'Vi-CELL','TimeStamp':1617905994,'RetrieveDate':'2021-04-08T12:19:54','BpQcName':'','DilutionName':'1','WashName':0,'SampleImageList':{'$type':'System.Collections.ObjectModel.ObservableCollection`1[[ScoutDomains.SampleImageRecordDomain, ScoutDomains]], System','$values':[]},'ResultSummaryList':{'$type':'System.Collections.ObjectModel.ObservableCollection`1[[ScoutDomains.RunResult.ResultSummaryDomain, ScoutDomains]], System','$values':[{'$type':'ScoutDomains.RunResult.ResultSummaryDomain, ScoutDomains','UUID':{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'DXoG4w7URlKYQBm6omm4hw=='}},'UserId':'Vi-CELL','TimeStamp':1617905994,'CellTypeDomain':{'$type':'ScoutDomains.Analysis.CellTypeDomain, ScoutDomains','ListItemLabel':'Yeast','CellTypeIndex':3,'CellTypeName':'Yeast','AnalysisDomain':null,'AspirationCycles':3,'IsCellEnable':false,'IsCellSelected':false,'TempCellName':'Yeast','IsUserDefineCellType':false,'IsQualityControlCellType':false,'MinimumDiameter':3.0,'MaximumDiameter':20.0,'Images':100,'CellSharpness':4.0,'DeclusterDegree':3,'MinimumCircularity':0.1,'CalculationAdjustmentFactor':0.0},'AppDomain':{'$type':'ScoutDomains.Analysis.AnalysisDomain, ScoutDomains','AnalysisIndex':0,'Label':'Viable (TB)','MixingCycle':3,'AnalysisParameter':{'$type':'System.Collections.Generic.List`1[[ScoutDomains.AnalysisParameterDomain, ScoutDomains]], mscorlib','$values':[{'$type':'ScoutDomains.AnalysisParameterDomain, ScoutDomains','Label':'Cell Spot Area','Characteristic':{'$type':'ScoutUtilities.Structs.Characteristic_t, ScoutUtilities','key':20,'s_key':0,'s_s_key':0},'ThresholdValue':2.0,'AboveThreshold':true},{'$type':'ScoutDomains.AnalysisParameterDomain, ScoutDomains','Label':'Average Spot Brightness','Characteristic':{'$type':'ScoutUtilities.Structs.Characteristic_t, ScoutUtilities','key':21,'s_key':0,'s_s_key':0},'ThresholdValue':45.0,'AboveThreshold':true}]},'IsAnalysisEnable':false},'CumulativeResult':{'$type':'ScoutDomains.BasicResultDomain, ScoutDomains','ProcessedStatus':5,'TotalCumulativeImage':100,'TotalCells':588,'ViableCells':0,'ConcentrationML':216384.0,'ViableConcentration':0.0,'Viability':0.0,'Size':10.19,'ViableSize':0.0,'Circularity':0.98,'ViableCircularity':0.0,'IsConcentrationValid':false,'IsViableConcentrationValid':false,'AverageCellsPerImage':5.0,'Bubble':0,'AvgBackground':131,'ClusterCount':0},'SignatureList':{'$type':'System.Collections.ObjectModel.ObservableCollection`1[[ScoutDomains.RunResult.SignatureInstanceDomain, ScoutDomains]], System','$values':[]},'SelectedSignature':null,'RetrieveDate':'2021-04-08T12:19:54'}]},'SelectedResultSummary':{'$type':'ScoutDomains.RunResult.ResultSummaryDomain, ScoutDomains','UUID':{'$type':'ScoutUtilities.Structs.uuidDLL, ScoutUtilities','u':{'$type':'System.Byte[], mscorlib','$value':'DXoG4w7URlKYQBm6omm4hw=='}},'UserId':'Vi-CELL','TimeStamp':1617905994,'CellTypeDomain':{'$type':'ScoutDomains.Analysis.CellTypeDomain, ScoutDomains','ListItemLabel':'Yeast','CellTypeIndex':3,'CellTypeName':'Yeast','AnalysisDomain':null,'AspirationCycles':3,'IsCellEnable':false,'IsCellSelected':false,'TempCellName':'Yeast','IsUserDefineCellType':false,'IsQualityControlCellType':false,'MinimumDiameter':3.0,'MaximumDiameter':20.0,'Images':100,'CellSharpness':4.0,'DeclusterDegree':3,'MinimumCircularity':0.1,'CalculationAdjustmentFactor':0.0},'AppDomain':{'$type':'ScoutDomains.Analysis.AnalysisDomain, ScoutDomains','AnalysisIndex':0,'ApplicationName':'Viable (TB)','MixingCycle':3,'AnalysisParameter':{'$type':'System.Collections.Generic.List`1[[ScoutDomains.AnalysisParameterDomain, ScoutDomains]], mscorlib','$values':[{'$type':'ScoutDomains.AnalysisParameterDomain, ScoutDomains','Label':'Cell Spot Area','Characteristic':{'$type':'ScoutUtilities.Structs.Characteristic_t, ScoutUtilities','key':20,'s_key':0,'s_s_key':0},'ThresholdValue':2.0,'AboveThreshold':true},{'$type':'ScoutDomains.AnalysisParameterDomain, ScoutDomains','Label':'Average Spot Brightness','Characteristic':{'$type':'ScoutUtilities.Structs.Characteristic_t, ScoutUtilities','key':21,'s_key':0,'s_s_key':0},'ThresholdValue':45.0,'AboveThreshold':true}]},'IsAnalysisEnable':false},'CumulativeResult':{'$type':'ScoutDomains.BasicResultDomain, ScoutDomains','ProcessedStatus':5,'TotalCumulativeImage':100,'TotalCells':588,'ViableCells':0,'ConcentrationML':216384.0,'ViableConcentration':0.0,'Viability':0.0,'Size':10.19,'ViableSize':0.0,'Circularity':0.98,'ViableCircularity':0.0,'IsConcentrationValid':false,'IsViableConcentrationValid':false,'AverageCellsPerImage':5.0,'Bubble':0,'AvgBackground':131,'ClusterCount':0},'SignatureList':{'$type':'System.Collections.ObjectModel.ObservableCollection`1[[ScoutDomains.RunResult.SignatureInstanceDomain, ScoutDomains]], System','$values':[]},'SelectedSignature':null,'RetrieveDate':'2021-04-08T12:19:54'},'SelectedSampleImageRecord':null,'ImageIndexList':{'$type':'System.Collections.ObjectModel.ObservableCollection`1[[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib],[System.String, mscorlib]], mscorlib]], System','$values':[]},'ShowParameterList':{'$type':'System.Collections.Generic.List`1[[System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib]], mscorlib','$values':[]},'IsSampleHasBubble':1,'SampleCompletionStatus':0,'RegentInfoRecordList':{'$type':'System.Collections.Generic.List`1[[ScoutDomains.Reagent.ReagentInfoRecordDomain, ScoutDomains]], mscorlib','$values':[{'$type':'ScoutDomains.Reagent.ReagentInfoRecordDomain, ScoutDomains','PackNumber':'C06019','LotNumber':654321,'ReagentName':'Cleaning Agent','ExpirationDate':'2021-05-08T00:00:00-06:00','InServiceDate':'2021-04-08T00:00:00-06:00','EffectiveExpirationDate':'2021-05-08T00:00:00-06:00'},{'$type':'ScoutDomains.Reagent.ReagentInfoRecordDomain, ScoutDomains','PackNumber':'C06019','LotNumber':654321,'ReagentName':'Conditioning Solution','ExpirationDate':'2021-05-08T00:00:00-06:00','InServiceDate':'2021-04-08T00:00:00-06:00','EffectiveExpirationDate':'2021-05-08T00:00:00-06:00'},{'$type':'ScoutDomains.Reagent.ReagentInfoRecordDomain, ScoutDomains','PackNumber':'C06019','LotNumber':654321,'ReagentName':'Buffer Solution','ExpirationDate':'2021-05-08T00:00:00-06:00','InServiceDate':'2021-04-08T00:00:00-06:00','EffectiveExpirationDate':'2021-05-08T00:00:00-06:00'},{'$type':'ScoutDomains.Reagent.ReagentInfoRecordDomain, ScoutDomains','PackNumber':'C06019','LotNumber':654321,'ReagentName':'Trypan Blue','ExpirationDate':'2021-05-08T00:00:00-06:00','InServiceDate':'2021-04-08T00:00:00-06:00','EffectiveExpirationDate':'2021-05-08T00:00:00-06:00'}]}}}";
            var obj = JsonConvert.DeserializeObject<SampleEswDomain>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            var sampleResult = Mapper.Map<SampleResult>(obj);
            Assert.IsNotNull(sampleResult);
        }

	}
}
