using JetBrains.Annotations;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Facade;

namespace ScoutModels.Service
{
    public class CalibrationModel : BaseNotifyPropertyChanged
    {
        #region Public Static Methods

        public static WorkListDomain GetConcentrationWorkListDomain(string concentrationComment)
        {
            ushort setIndex = 1;
            uint dilution = 1;
            uint cellTypeIndex = (uint)CellTypeIndex.BciConcBeads;
            var useSequencing = false;
            var substrateType = SubstrateType.Carousel;
            var precession = Precession.RowMajor;
            var wash = SamplePostWash.NormalWash;
            var username = LoggedInUser.CurrentUserId;

            var ssd = new SampleSetDomain();
            ssd.Carrier = SubstrateType.Carousel;
            ssd.Index = setIndex;
            ssd.Username = username;
            ssd.SampleSetName = LanguageResourceHelper.Get("LID_TabItem_CalibrationControl");
            ssd.SampleSetStatus = SampleSetStatus.Pending;
            ssd.Samples = GetSamplesForConcentration(concentrationComment, username, setIndex);

            var wld = new WorkListDomain();
            wld.Carrier = substrateType;
            wld.Precession = precession;
            wld.RunByUsername = username;
            wld.Username = username;

            wld.Label = LanguageResourceHelper.Get("LID_TabItem_CalibrationControl");
            wld.Tag = string.Empty;
            wld.DilutionFactor = dilution;
            wld.Wash = wash;
            wld.CellTypeIndex = cellTypeIndex;
            wld.QualityControlName = string.Empty;

            wld.SequenceParameters = new SequenceParametersDomain();
            wld.SequenceParameters.SequencingBaseLabel = wld.Label;
            wld.SequenceParameters.UseSequencing = useSequencing;

            var orphanSsd = new SampleSetDomain();
            orphanSsd.Index = 0;
            orphanSsd.Carrier = SubstrateType.Carousel;
            orphanSsd.Username = username;
            orphanSsd.SampleSetName = LanguageResourceHelper.Get("LID_OrphanSetName");
            orphanSsd.SampleSetStatus = SampleSetStatus.Pending;
            orphanSsd.Samples = new List<SampleEswDomain>();

            wld.SampleSets = new List<SampleSetDomain>();
            wld.SampleSets.Add(orphanSsd); // orphan set must be the first set -- backend requires this
            wld.SampleSets.Add(ssd);

            return wld;
        }

        public static WorkListDomain GetACupConcentrationWorkListDomain(
            string aCupConcentrationComment, ICalibrationConcentrationListDomain aCupConcDomain, int listIndex)
        {
            ushort setIndex = 1;
            uint dilution = 1;
            uint cellTypeIndex = (uint)CellTypeIndex.BciConcBeads;
            var useSequencing = false;
            var substrateType = SubstrateType.AutomationCup;
            var precession = Precession.RowMajor;
            var wash = SamplePostWash.NormalWash;
            var username = LoggedInUser.CurrentUserId;

            var ssd = new SampleSetDomain();
            ssd.Carrier = SubstrateType.AutomationCup;
            ssd.Index = setIndex;
            ssd.Username = username;
            ssd.SampleSetName = LanguageResourceHelper.Get("LID_TabItem_CalibrationControl");
            ssd.SampleSetStatus = SampleSetStatus.Pending;
            ssd.Samples = GetSamplesForACupConcentration(aCupConcentrationComment, username, setIndex, aCupConcDomain, listIndex);

            var wld = new WorkListDomain();
            wld.Carrier = substrateType;
            wld.Precession = precession;
            wld.RunByUsername = username;
            wld.Username = username;

            wld.Label = LanguageResourceHelper.Get("LID_TabItem_CalibrationControl");
            wld.Tag = string.Empty;
            wld.DilutionFactor = dilution;
            wld.Wash = wash;
            wld.CellTypeIndex = cellTypeIndex;
            wld.QualityControlName = string.Empty;

            wld.SequenceParameters = new SequenceParametersDomain();
            wld.SequenceParameters.SequencingBaseLabel = wld.Label;
            wld.SequenceParameters.UseSequencing = useSequencing;

            var orphanSsd = new SampleSetDomain();
            orphanSsd.Index = 0;
            orphanSsd.Carrier = SubstrateType.AutomationCup;
            orphanSsd.Username = username;
            orphanSsd.SampleSetName = LanguageResourceHelper.Get("LID_OrphanSetName");
            orphanSsd.SampleSetStatus = SampleSetStatus.Pending;
            orphanSsd.Samples = new List<SampleEswDomain>();

            wld.SampleSets = new List<SampleSetDomain>();
            wld.SampleSets.Add(orphanSsd); // orphan set must be the first set -- backend requires this
            wld.SampleSets.Add(ssd);

            return wld;
        }

        public static List<ICalibrationConcentrationListDomain> GetStandardConcentrationList()
        {
            var list = new List<ICalibrationConcentrationListDomain>();

            var calibrationConcentrationListDomain2M = new CalibrationConcentrationListDomain
            {
                KnownConcentration = ApplicationConstants.KnownConcentration2M,
                NumberOfTubes = ApplicationConstants.NumberOfTubes2M,
                StartPosition = ApplicationConstants.StartPosition2M,
                EndPosition = ApplicationConstants.EndPosition2M,
                AssayValue = ApplicationConstants.AssayValue2M,
                Lot = string.Empty,
                ExpiryDate = DateTime.Now,
                AssayValueType = GetAssayValueEnumFrom(ApplicationConstants.AssayValue2M),
                Status = GetSampleStatusFrom(ApplicationConstants.AssayValue2M)
            };
            list.Add(calibrationConcentrationListDomain2M);
            var calibrationConcentrationListDomain4M = new CalibrationConcentrationListDomain
            {
                KnownConcentration = ApplicationConstants.KnownConcentration4M,
                NumberOfTubes = ApplicationConstants.NumberOfTubes4M,
                StartPosition = ApplicationConstants.StartPosition4M,
                EndPosition = ApplicationConstants.EndPosition4M,
                AssayValue = ApplicationConstants.AssayValue4M,
                Lot = string.Empty,
                ExpiryDate = DateTime.Now,
                AssayValueType = GetAssayValueEnumFrom(ApplicationConstants.AssayValue4M),
                Status = GetSampleStatusFrom(ApplicationConstants.AssayValue4M)
            };
            list.Add(calibrationConcentrationListDomain4M);
            var calibrationConcentrationListDomain10M = new CalibrationConcentrationListDomain
            {
                KnownConcentration = ApplicationConstants.KnownConcentration10M,
                NumberOfTubes = ApplicationConstants.NumberOfTubes10M,
                StartPosition = ApplicationConstants.StartPosition10M,
                EndPosition = ApplicationConstants.EndPosition10M,
                AssayValue = ApplicationConstants.AssayValue10M,
                Lot = string.Empty,
                ExpiryDate = DateTime.Now,
                AssayValueType = GetAssayValueEnumFrom(ApplicationConstants.AssayValue10M),
                Status = GetSampleStatusFrom(ApplicationConstants.AssayValue10M)
            };
            list.Add(calibrationConcentrationListDomain10M);
            return list;
        }

        //Initializes list shown on left side pane of A-Cup Concentration slope tab
        public static List<ICalibrationConcentrationListDomain> GetACupConcentrationListDefault()
        {
            var list = new List<ICalibrationConcentrationListDomain>();
            for (var i = 0; i < ApplicationConstants.NumberOfTubesInConcentration; i++)
            {
                var calibrationConcentrationListDomain = new CalibrationConcentrationListDomain();
                if (i < ApplicationConstants.EndPosition2M) //i < 10
                {
                    calibrationConcentrationListDomain.KnownConcentration = ApplicationConstants.KnownConcentration2M;
                    calibrationConcentrationListDomain.Status = GetSampleStatusFrom(ApplicationConstants.AssayValue2M);
                    calibrationConcentrationListDomain.StartPosition = ApplicationConstants.StartPosition2M;
                    calibrationConcentrationListDomain.EndPosition = ApplicationConstants.EndPosition2M;
                }
                if (i >= ApplicationConstants.EndPosition2M && i < ApplicationConstants.EndPosition4M) //i >= 10 && i < 15
                {
                    calibrationConcentrationListDomain.KnownConcentration = ApplicationConstants.KnownConcentration4M;
                    calibrationConcentrationListDomain.Status = GetSampleStatusFrom(ApplicationConstants.AssayValue4M);
                    calibrationConcentrationListDomain.StartPosition = ApplicationConstants.StartPosition4M;
                    calibrationConcentrationListDomain.EndPosition = ApplicationConstants.EndPosition4M;
                }
                if (i >= ApplicationConstants.EndPosition4M) //i >= 15
                {
                    calibrationConcentrationListDomain.KnownConcentration = ApplicationConstants.KnownConcentration10M;
                    calibrationConcentrationListDomain.Status = GetSampleStatusFrom(ApplicationConstants.AssayValue10M);
                    calibrationConcentrationListDomain.StartPosition = ApplicationConstants.StartPosition10M;
                    calibrationConcentrationListDomain.EndPosition = ApplicationConstants.EndPosition10M;
                }
                list.Add(calibrationConcentrationListDomain);
            }

            return list;
        }

        //After validating user entered values for Assay Value, Lot and Expiration - Copies these
        //values to the ACupConcentrationList
        public static void UpdateACupConcentrationList(IList<ICalibrationConcentrationListDomain> aCupUserEnteredValuesList,
            IList<ICalibrationConcentrationListDomain> aCupConcentrationList)
        {
            for (var i = 0; i < ApplicationConstants.NumberOfTubesInConcentration; i++)
            {
                if (i < ApplicationConstants.EndPosition2M) //i < 10
                {
                    aCupConcentrationList[i].AssayValue = aCupUserEnteredValuesList[0].AssayValue;
                    aCupConcentrationList[i].Lot = aCupUserEnteredValuesList[0].Lot;
                    aCupConcentrationList[i].ExpiryDate = aCupUserEnteredValuesList[0].ExpiryDate;
                    aCupConcentrationList[i].AssayValueType = GetAssayValueEnumFrom(aCupUserEnteredValuesList[0].AssayValue);
                    aCupConcentrationList[i].IsCorrectAssayValue = aCupUserEnteredValuesList[0].IsCorrectAssayValue;
                }
                if (i >= ApplicationConstants.EndPosition2M && i < ApplicationConstants.EndPosition4M) //i >= 10 && i < 15
                {
                    aCupConcentrationList[i].AssayValue = aCupUserEnteredValuesList[1].AssayValue;
                    aCupConcentrationList[i].Lot = aCupUserEnteredValuesList[1].Lot;
                    aCupConcentrationList[i].ExpiryDate = aCupUserEnteredValuesList[1].ExpiryDate;
                    aCupConcentrationList[i].AssayValueType = GetAssayValueEnumFrom(aCupUserEnteredValuesList[1].AssayValue);
                    aCupConcentrationList[i].IsCorrectAssayValue = aCupUserEnteredValuesList[1].IsCorrectAssayValue;
                }
                if (i >= ApplicationConstants.EndPosition4M) //i >= 15
                {
                    aCupConcentrationList[i].AssayValue = aCupUserEnteredValuesList[2].AssayValue;
                    aCupConcentrationList[i].Lot = aCupUserEnteredValuesList[2].Lot;
                    aCupConcentrationList[i].ExpiryDate = aCupUserEnteredValuesList[2].ExpiryDate;
                    aCupConcentrationList[i].AssayValueType = GetAssayValueEnumFrom(aCupUserEnteredValuesList[2].AssayValue);
                    aCupConcentrationList[i].IsCorrectAssayValue = aCupConcentrationList[2].IsCorrectAssayValue;
                }
            }
        }

        public static void RetrieveCalibrationActivityLogRange(calibration_type cal_type, ulong startTime, ulong endTime,
            out List<CalibrationActivityLogDomain> calibrationErrorLog)
        {
            var logEntriesAll = new List<calibration_history_entry>();
            Log.Debug("RetrieveCalibrationActivityLogRange:: calibration type: " + cal_type);
            var hawkeyeError = HawkeyeCoreAPI.Calibration.RetrieveCalibrationActivityLogRangeAPI(cal_type, startTime, endTime, out var num_entries, ref logEntriesAll);
            Log.Debug("- Output from Method:RetrieveCalibrationActivityLogRange:: " + hawkeyeError.Item1 + ", num_entries " + num_entries);
            calibrationErrorLog = CreateRetrieveLogList(logEntriesAll, hawkeyeError.Item2);
        }

        public static List<CalibrationActivityLogDomain> RetrieveCalibrationActivityLog(calibration_type cal_type)
        {
            var logEntriesAll = new List<calibration_history_entry>();

            Log.Debug("RetrieveCalibrationActivityLog:: calibration type: " + cal_type);
            var hawkeyeError = HawkeyeCoreAPI.Calibration.RetrieveCalibrationActivityLogAPI(cal_type, out var num_entries, ref logEntriesAll);
            Log.Debug("RetrieveCalibrationActivityLog:: hawkeyeError: " + hawkeyeError.Item1 + " num_entries " + num_entries);
            return CreateRetrieveLogList(logEntriesAll, hawkeyeError.Item2);
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError ClearCalibrationActivityLog(calibration_type cal_type, ulong archive_prior_to_time, string password, bool clearAllACupData)
        {
            Log.Debug("ClearCalibrationActivityLog:: cal_type: " + cal_type.ToString() + 
                      ", archive_prior_to_time: " + archive_prior_to_time +
					  ", clearAllACupData: " + clearAllACupData);
            var hawkeyeError = HawkeyeCoreAPI.Calibration.ClearCalibrationActivityLogAPI(cal_type, archive_prior_to_time, password, clearAllACupData);
            Log.Debug("ClearCalibrationActivityLog:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError SetConcentrationCalibration(calibration_type calType, double slope, double intercept, uint cal_image_count,
            uuidDLL queue_id, UInt16 num_consumables, List<CalibrationConcentrationOverTimeDomain> consumablesId)
        {
            var consumableStr = CreateListOfCalibration(consumablesId);
            LogSetConcentrationCalibration(calType,slope, intercept, cal_image_count, queue_id, num_consumables, consumableStr);

            var hawkeyeError = HawkeyeCoreAPI.Calibration.SetConcentrationCalibrationAPI(calType,slope, intercept, cal_image_count, queue_id, num_consumables, consumableStr);
            Log.Debug("SetConcentrationCalibration:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        #endregion

        #region Private Static Methods

        private static void LogSetConcentrationCalibration(calibration_type calType,double slope, double intercept, uint cal_image_count, uuidDLL queue_id, ushort num_consumables, List<calibration_consumable> consumableStr)
        {
            Log.Debug($"LogSetConcentrationCalibration::{Environment.NewLine}" +
                      $"type: {calType}{Environment.NewLine}" +
                      $"slope: {slope}{Environment.NewLine}" +
                $"intercept: {intercept}{Environment.NewLine}" +
                $"cal_image_count: {cal_image_count}{Environment.NewLine}" +
                $"queue_id: {queue_id}{Environment.NewLine}" +
                $"num_consumables: {num_consumables}{Environment.NewLine}");

            for (var i = 0; i < consumableStr.Count; i++)
            {
                var c = consumableStr[i];
                LogConsumable(c);
            }
        }

        public static AssayValueEnum GetAssayValueEnumFrom(double assayValue)
        {
            if (Math.Abs(assayValue - 2) < 0.00001) return AssayValueEnum.M2;
            if (Math.Abs(assayValue - 4) < 0.00001) return AssayValueEnum.M4;
            if (Math.Abs(assayValue - 10) < 0.00001) return AssayValueEnum.M10;

            return AssayValueEnum.M2;
        }

        public static SampleStatusColor GetSampleStatusFrom(double assayValue)
        {
            if (Math.Abs(assayValue - 2) < 0.00001) return SampleStatusColor.ConcentrationType1;
            if (Math.Abs(assayValue - 4) < 0.00001) return SampleStatusColor.ConcentrationType2;
            if (Math.Abs(assayValue - 10) < 0.00001) return SampleStatusColor.ConcentrationType3;

            return SampleStatusColor.ConcentrationType1;
        }

        private static SampleEswDomain GetSampleForAssayValue(AssayValueEnum assayValue, IList<CellTypeDomain> cellTypes,
            ushort sampleIndex, ushort sampleSetIndex, SamplePosition samplePosition, string comment, DateTime dateTime, string username)
        {
            var sample = new SampleEswDomain();

            sample.CellTypeIndex = (uint)CellTypeIndex.BciConcBeads;
            sample.CellTypeQcName = cellTypes.FirstOrDefault(c => c.CellTypeIndex == (uint)CellTypeIndex.BciConcBeads)?.CellTypeName;
            sample.Dilution = 1;
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

        private static List<CalibrationActivityLogDomain> CreateRetrieveLogList(List<calibration_history_entry> logEntriesAll, IntPtr calibrationPtr)
        {
            var list = new List<CalibrationActivityLogDomain>();
            logEntriesAll.ForEach(logValue =>
            {
                var caliLog = CreateCalibrationErrorLogDomain(logValue);
                list.Add(caliLog);
            });

            HawkeyeCoreAPI.Calibration.FreeCalibrationHistoryEntryAPI(calibrationPtr, (uint)logEntriesAll.Count);

            return list;
        }

        private static List<calibration_consumable> CreateListOfCalibration(List<CalibrationConcentrationOverTimeDomain> calibrationList)
        {
            var calibrationListStr = new List<calibration_consumable>();
            Log.Debug("CreateListOfCalibration::");
            calibrationList.ForEach(calibration => { calibrationListStr.Add(CreateStructure(calibration)); });

            return calibrationListStr;
        }

        private static calibration_consumable CreateStructure(CalibrationConcentrationOverTimeDomain calibrationDomain)
        {
            var calibrationStr = new calibration_consumable()
            {
                label = calibrationDomain.ConsumableLabel.ToIntPtr(),
                lot_id = calibrationDomain.ConsumableLotId.ToIntPtr(),
                assay_value = calibrationDomain.AssayValue,
                expiration_date = (uint)DateTimeConversionHelper.DateTimeToUnixDays(calibrationDomain.Date)
            };

            LogConsumable(calibrationStr);

            return calibrationStr;
        }

        private static void LogConsumable(calibration_consumable calibrationStr)
        {
            var dt = DateTimeConversionHelper.FromDaysUnixToDateTime(calibrationStr.expiration_date).ToString("G");
            Log.Debug($"LogConsumable()::{Environment.NewLine}" +
                $"AssayValue: {calibrationStr.assay_value}{Environment.NewLine}" +
                $"ExpirationDate: {calibrationStr.expiration_date} ({dt}){Environment.NewLine}" +
                $"Label: {calibrationStr.label}{Environment.NewLine}" +
                $"LotId: {calibrationStr.lot_id}{Environment.NewLine}");
        }

        private static CalibrationActivityLogDomain CreateCalibrationErrorLogDomain(calibration_history_entry error)
        {
            var calibration = new CalibrationActivityLogDomain()
            {
                Date = DateTimeConversionHelper.FromSecondUnixToDateTime(error.timestamp),
                UserId = error.user_id.ToSystemString(),
                NumberOfConsumables = error.num_consumables,
                Slope = error.slope,
                ImageCount = (int)error.image_count,
                Intercept = error.intercept,
                CalibrationType = error.cal_type
            };

            var consumableList = new List<CalibrationConsumableDomain>();
            var calibrationList = new List<calibration_consumable>();
            var sz = Marshal.SizeOf(typeof(calibration_consumable));
            var consumableListPtr = error.consumable_list;

            for (int i = 0; i < error.num_consumables; i++)
            {
                calibrationList.Add((calibration_consumable)Marshal.PtrToStructure(consumableListPtr,
                    typeof(calibration_consumable)));
                consumableListPtr += sz;
            }

            calibrationList.ForEach(consume => { consumableList.Add(CreateConsumable(consume)); });
            calibration.Consumable = new List<CalibrationConsumableDomain>(consumableList);
            if (calibration.Consumable.Count > 0)
            {
                calibration.Label = calibration.Consumable[0].Label;
                calibration.LotId = calibration.Consumable[0].LotId;
                calibration.AssayValue = calibration.Consumable[0].AssayValue;
                calibration.ExpirationDate = calibration.Consumable[0].ExpirationDate;
            }

            return calibration;
        }

        private static CalibrationConsumableDomain CreateConsumable(calibration_consumable consumable)
        {
            var consume = new CalibrationConsumableDomain()
            {
                LotId = consumable.lot_id.ToSystemString(),
                Label = consumable.label.ToSystemString(),
                ExpirationDate = DateTimeConversionHelper.FromDaysUnixToDateTime(consumable.expiration_date),
                AssayValue = consumable.assay_value
            };
            return consume;
        }

        private static List<SampleEswDomain> GetSamplesForConcentration(string concentrationComment, string username,
            ushort sampleSetIndex)
        {
            var concSampleList = GetStandardConcentrationList();
            var concSampleEswDomains = new List<SampleEswDomain>();
            var cellTypes = CellTypeFacade.Instance.GetFactoryCellTypes_BECall();
            var dtNow = DateTime.Now;
            ushort sampleIndex = 0;

            foreach (var concentration in concSampleList)
            {
                for (var i = 0; i < concentration.NumberOfTubes; i++)
                {
                    var samplePosition = SamplePosition.Parse((sampleIndex + 1).ToString());
                    var sample = GetSampleForAssayValue(concentration.AssayValueType, cellTypes, sampleIndex,
                        sampleSetIndex, samplePosition, concentrationComment, dtNow, username);

                    concSampleEswDomains.Add(sample);

                    sampleIndex++;
                }
            }

            return concSampleEswDomains;
        }

        private static List<SampleEswDomain> GetSamplesForACupConcentration(string aCupConcentrationComment, string username,
            ushort sampleSetIndex, ICalibrationConcentrationListDomain aCupConcDomain, int listIndex)
        {
            var concSampleEswDomains = new List<SampleEswDomain>();
            var cellTypes = CellTypeFacade.Instance.GetFactoryCellTypes_BECall();
            var dtNow = DateTime.Now;
            ushort sampleIndex = (ushort)listIndex;

            var samplePosition = SamplePosition.GetAutomationCupSamplePosition();
            var sample = GetSampleForAssayValue(aCupConcDomain.AssayValueType, cellTypes, sampleIndex,
                sampleSetIndex, samplePosition, aCupConcentrationComment, dtNow, username);

            concSampleEswDomains.Add(sample);

            return concSampleEswDomains;
        }

        #endregion
    }
}