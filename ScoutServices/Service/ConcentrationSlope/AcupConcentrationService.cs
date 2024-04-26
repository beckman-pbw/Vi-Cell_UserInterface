using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Service;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using DateTimeConversionHelper = ScoutUtilities.DateTimeConversionHelper;

namespace ScoutServices.Service.ConcentrationSlope
{
    public class AcupConcentrationService : IAcupConcentrationService
    {
        private readonly ICellTypeManager _cellTypeManager;
        private readonly ILogger _logger;

        public AcupConcentrationService(ILogger logger, ICellTypeManager cellTypeManager)
        {
	        _logger = logger;
            _cellTypeManager = cellTypeManager;
        }

        public List<AcupCalibrationConcentrationListDomain> GetDefaultACupConcentrationList()
        {
            var list = new List<AcupCalibrationConcentrationListDomain>();
            for (var i = 0; i < ApplicationConstants.NumberOfTubesInConcentration; i++)
            {
                var calibrationConcentrationListDomain = new AcupCalibrationConcentrationListDomain();
                calibrationConcentrationListDomain.Index = i;

                if (i < ApplicationConstants.EndPosition2M) //i < 10
                {
                    calibrationConcentrationListDomain.KnownConcentration = ApplicationConstants.KnownConcentration2M;
                    calibrationConcentrationListDomain.Status = CalibrationModel.GetSampleStatusFrom(ApplicationConstants.AssayValue2M);
                    calibrationConcentrationListDomain.StartPosition = ApplicationConstants.StartPosition2M;
                    calibrationConcentrationListDomain.EndPosition = ApplicationConstants.EndPosition2M;
                }
                if (i >= ApplicationConstants.EndPosition2M && i < ApplicationConstants.EndPosition4M) //i >= 10 && i < 15
                {
                    calibrationConcentrationListDomain.KnownConcentration = ApplicationConstants.KnownConcentration4M;
                    calibrationConcentrationListDomain.Status = CalibrationModel.GetSampleStatusFrom(ApplicationConstants.AssayValue4M);
                    calibrationConcentrationListDomain.StartPosition = ApplicationConstants.StartPosition4M;
                    calibrationConcentrationListDomain.EndPosition = ApplicationConstants.EndPosition4M;
                }
                if (i >= ApplicationConstants.EndPosition4M) //i >= 15
                {
                    calibrationConcentrationListDomain.KnownConcentration = ApplicationConstants.KnownConcentration10M;
                    calibrationConcentrationListDomain.Status = CalibrationModel.GetSampleStatusFrom(ApplicationConstants.AssayValue10M);
                    calibrationConcentrationListDomain.StartPosition = ApplicationConstants.StartPosition10M;
                    calibrationConcentrationListDomain.EndPosition = ApplicationConstants.EndPosition10M;
                }

                list.Add(calibrationConcentrationListDomain);
            }

            return list;
        }

        public SampleSetDomain GetACupConcentrationSampleSetDomain(string aCupConcentrationComment,
            AcupCalibrationConcentrationListDomain aCupConcDomain)
        {
            var sampleIndex = aCupConcDomain.Index;
            var setIndex = (ushort) ApplicationConstants.FirstNonOrphanSampleSetIndex;
            var username = LoggedInUser.CurrentUserId;

            var ssd = new SampleSetDomain();
            ssd.Carrier = SubstrateType.AutomationCup;
            ssd.Index = setIndex;
            ssd.Username = username;
            ssd.SampleSetName = LanguageResourceHelper.Get("LID_TabItem_CalibrationControl");
            ssd.SampleSetStatus = SampleSetStatus.Pending;
            ssd.Samples = GetSamplesForACupConcentration(aCupConcentrationComment, username, setIndex, aCupConcDomain, sampleIndex);
            
            return ssd;
        }

        public void UpdateACupConcentrationList(IList<ICalibrationConcentrationListDomain> aCupUserEnteredValuesList,
            IList<AcupCalibrationConcentrationListDomain> aCupConcentrationList)
        {
            for (var i = 0; i < ApplicationConstants.NumberOfTubesInConcentration; i++)
            {
                if (i < ApplicationConstants.EndPosition2M) //i < 10
                {
                    aCupConcentrationList[i].AssayValue = aCupUserEnteredValuesList[0].AssayValue;
                    aCupConcentrationList[i].Lot = aCupUserEnteredValuesList[0].Lot;
                    aCupConcentrationList[i].ExpiryDate = aCupUserEnteredValuesList[0].ExpiryDate;
                    aCupConcentrationList[i].AssayValueType = CalibrationModel.GetAssayValueEnumFrom(aCupUserEnteredValuesList[0].AssayValue);
                    aCupConcentrationList[i].IsCorrectAssayValue = aCupUserEnteredValuesList[0].IsCorrectAssayValue;
                }
                if (i >= ApplicationConstants.EndPosition2M && i < ApplicationConstants.EndPosition4M) //i >= 10 && i < 15
                {
                    aCupConcentrationList[i].AssayValue = aCupUserEnteredValuesList[1].AssayValue;
                    aCupConcentrationList[i].Lot = aCupUserEnteredValuesList[1].Lot;
                    aCupConcentrationList[i].ExpiryDate = aCupUserEnteredValuesList[1].ExpiryDate;
                    aCupConcentrationList[i].AssayValueType = CalibrationModel.GetAssayValueEnumFrom(aCupUserEnteredValuesList[1].AssayValue);
                    aCupConcentrationList[i].IsCorrectAssayValue = aCupUserEnteredValuesList[1].IsCorrectAssayValue;
                }
                if (i >= ApplicationConstants.EndPosition4M) //i >= 15
                {
                    aCupConcentrationList[i].AssayValue = aCupUserEnteredValuesList[2].AssayValue;
                    aCupConcentrationList[i].Lot = aCupUserEnteredValuesList[2].Lot;
                    aCupConcentrationList[i].ExpiryDate = aCupUserEnteredValuesList[2].ExpiryDate;
                    aCupConcentrationList[i].AssayValueType = CalibrationModel.GetAssayValueEnumFrom(aCupUserEnteredValuesList[2].AssayValue);
                    aCupConcentrationList[i].IsCorrectAssayValue = aCupConcentrationList[2].IsCorrectAssayValue;
                }
            }
        }
        
        public void CalculateAcupConcentration(IList<SampleRecordDomain> sampleRecords, 
            IList<AcupCalibrationConcentrationListDomain> concentrationSamples, 
            out CalibrationData totalCells, 
            out CalibrationData originalConcentration,
            out CalibrationData adjustedConcentration)
        {
            totalCells = new CalibrationData();
            originalConcentration = new CalibrationData();
            adjustedConcentration = new CalibrationData();

            if (sampleRecords == null) return;

            foreach (var record in sampleRecords)
            {
                //get the sample number from the end of the sample name
                var number = Regex.Match(record.SampleIdentifier, @"\d+$", RegexOptions.RightToLeft).Value;
                int.TryParse(number, out var sampleNumber);
                var concentrationList = concentrationSamples.Where(x =>
                    x.StartPosition <= sampleNumber && x.EndPosition >= sampleNumber);

                double assayValue = 0;
                var concentration = concentrationList.FirstOrDefault();
                if (concentration != null)
                {
                    var assayValueTemp = concentration.AssayValue;
                    switch (concentration.AssayValueType)
                    {
                        case AssayValueEnum.M2:
                            assayValue = assayValueTemp * 1000000;
                            break;
                        case AssayValueEnum.M4:
                            assayValue = assayValueTemp * 1000000;
                            break;
                        case AssayValueEnum.M10:
                            // If the provided value is >= 2, we assume the unit is 10^6/uL;
                            // otherwise, we assume 10^7/uL.
                            if (assayValueTemp >= 2.0)
                            {
                                assayValue = assayValueTemp * 1000000;
                            }
                            else
                            {
                                assayValue = assayValueTemp * 10000000;
                            }

                            break;
                    }
                }

                // Normalize the total count to handle invalid images.
                var adjustedCount = record.SelectedResultSummary.CumulativeResult.TotalCells *
                                    (100.0 / record.SelectedResultSummary.CumulativeResult.TotalCumulativeImage);

                // For CellHealth adjust the resultant concentration using the dilution used by the sample.
                bool result = Int32.TryParse(record.DilutionName, out int value);
                if (result)
                {
	                adjustedCount *= (double)value;
                }
                else
                {
	                _logger.Warn("Conversion of DilutionName failed, concentration not adjusted by dilution");
                }

				totalCells.Data.Add(new KeyValuePair<double, double>(assayValue, adjustedCount));
                originalConcentration.Data.Add(new KeyValuePair<double, double>(assayValue,
                    record.SelectedResultSummary.CumulativeResult.ConcentrationML));
            }

            totalCells.CalculateSlope_averageOverAssays();
            originalConcentration.CalculateSlope_averageOverAssays();

            foreach (var pair in totalCells.Data)
            {
                var adjConcValue = (pair.Value * totalCells.Slope) + totalCells.Intercept;
                adjustedConcentration.Data.Add(new KeyValuePair<double, double>(pair.Key, adjConcValue));
            }

            adjustedConcentration.CalculateSlope_averageOverAssays();
        }
        
        #region Private Helper Methods

        private List<SampleEswDomain> GetSamplesForACupConcentration(string aCupConcentrationComment, string username,
            ushort sampleSetIndex, CalibrationConcentrationListDomain aCupConcDomain, int sampleIndex)
        {
            var concentrationSamples = new List<SampleEswDomain>();
            var dtNow = DateTime.Now;

            var samplePosition = SamplePosition.GetAutomationCupSamplePosition();
            var sample = GetSampleForAcupConcentration(aCupConcDomain.AssayValueType, (ushort) sampleIndex,
                sampleSetIndex, samplePosition, aCupConcentrationComment, dtNow, username);

            concentrationSamples.Add(sample);

            return concentrationSamples;
        }

        private SampleEswDomain GetSampleForAcupConcentration(AssayValueEnum assayValue,
            ushort sampleIndex, ushort sampleSetIndex, SamplePosition samplePosition, string comment,
            DateTime dateTime, string username)
        {
            var sample = new SampleEswDomain();

            var cellTypeIndex = (uint) CellTypeIndex.BciConcBeads;
            sample.CellTypeIndex = cellTypeIndex;
            sample.CellTypeQcName = _cellTypeManager.GetCellType(cellTypeIndex)?.CellTypeName;

			// CellHealth uses a dilution that is fixed in the A-Cup Sample workflow script.
            sample.Dilution = 4;

            sample.SaveEveryNthImage = 0;   // Only save the first and last images.
            sample.PlatePrecession = Precession.RowMajor;
            sample.SampleStatus = SampleStatus.NotProcessed;
            sample.SubstrateType = SubstrateType.Carousel;
            sample.WashType = SamplePostWash.NormalWash;

            sample.Index = sampleIndex;
            sample.SampleSetIndex = sampleSetIndex;
            sample.SamplePosition = samplePosition;
            sample.SampleTag = comment;
            sample.TimeStamp = DateTimeConversionHelper.DateTimeToUnixSecondRounded(dateTime);
            sample.Username = username;
            sample.SampleName = Misc.GetConcentrationSampleName(assayValue, sampleIndex + 1);

            return sample;
        }

        #endregion
    }
}