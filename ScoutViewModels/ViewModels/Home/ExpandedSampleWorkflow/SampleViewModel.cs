using HawkeyeCoreAPI.Facade;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutDomains.RunResult;
//using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Reports;
using ScoutModels.Review;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ScoutDataAccessLayer.IDAL;
using System.Globalization;
using Castle.Core.Internal;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class SampleViewModel : BaseViewModel, ICloneable
    {
        #region Constructor

        public SampleViewModel(SampleRecordDomain sampleRecord, SampleEswDomain sampleEswDomain, string setName,
            SampleHierarchyType hierarchyType, RunOptionSettingsModel runOptionSettings)
        {
            SubstrateType = sampleEswDomain.SubstrateType;
            _platePrecession = sampleEswDomain.PlatePrecession;
            Uuid = sampleEswDomain.Uuid;
            SampleDataUid = sampleEswDomain.SampleDataUuid;
            SampleSetUid = sampleEswDomain.SampleSetUid;
            SampleStatus = sampleEswDomain.SampleStatus;
            SampleName = sampleEswDomain.SampleName;
            SampleTag = sampleEswDomain.SampleTag;
            SamplePosition = sampleEswDomain.SamplePosition;
            Dilution = sampleEswDomain.Dilution;
            Username = sampleEswDomain.Username;

            CellTypeQcName = sampleEswDomain.CellTypeQcName;
            QcAndCellTypes = LoggedInUser.GetCtQcs().ToObservableCollection();
            WashTypes = Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToObservableCollection();
            WashType = sampleEswDomain.WashType;
            SampleSetName = setName;
            AdvancedSampleSettings = new AdvancedSampleSettingsViewModel(runOptionSettings);
            AdvancedSampleSettings.NthImage = sampleEswDomain.SaveEveryNthImage;

            SetFrom(sampleRecord, this, hierarchyType);
        }

        public SampleViewModel(RunOptionSettingsModel runOptionSettings, 
            SubstrateType substrateType = ScoutUtilities.Enums.SubstrateType.Carousel, 
            Precession rowColPrecession = Precession.RowMajor,
            List<CellTypeQualityControlGroupDomain> qcAndCellTypes = null)
        {
            SubstrateType = substrateType;
            _platePrecession = rowColPrecession;
            SampleStatus = SampleStatus.NotProcessed;
            QcAndCellTypes = qcAndCellTypes == null
                ? LoggedInUser.GetCtQcs().ToObservableCollection()
                : qcAndCellTypes.ToObservableCollection();
            // Remove from the list any quality controls that are expired
            for (var i = QcAndCellTypes.Count - 1; i >= 0; i--)
                if (QcAndCellTypes[i].SelectedCtBpQcType == CtBpQcType.QualityControl)
                { 
                    foreach (var qc in QcAndCellTypes[i].CellTypeQualityControlChildItems.ToList())
                    {
                        if (qc != null)
                        {
                            var foundQc = LoggedInUser.GetAllowedQcs().FirstOrDefault(q => q.QcName == qc.KeyName);
                            
                            if (foundQc == null || !foundQc.NotExpired)
                                QcAndCellTypes[i].CellTypeQualityControlChildItems.Remove(qc);
                        }
                    }
                    // If no more quality controls, remove this entry from the QcAndCellTypes list
                    if (QcAndCellTypes[i].CellTypeQualityControlChildItems.IsNullOrEmpty())
                    {
                        if (QcAndCellTypes[i] != null)
                            QcAndCellTypes.Remove(QcAndCellTypes[i]);
                    }
                }
            WashTypes = Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToObservableCollection();
            AdvancedSampleSettings = new AdvancedSampleSettingsViewModel(runOptionSettings);
        }

        public SampleViewModel(SampleDomain sampleDomain, RunOptionSettingsModel runOptionSettings,
            List<CellTypeQualityControlGroupDomain> qcAndCellTypes = null)
        {
            SampleStatus = SampleStatus.NotProcessed;

            sampleDomain.SelectedCellTypeQualityControlGroup.Name = sampleDomain.BpQcName;

            sampleDomain.SamplePosition = SamplePosition.Parse(sampleDomain.SampleRowPosition);
            sampleDomain.Position = sampleDomain.SamplePosition.ToString();
            SubstrateType = sampleDomain.SamplePosition.GetSubstrateType();
            _platePrecession = sampleDomain.RowWisePosition.Equals(ApplicationConstants.Row) ? Precession.RowMajor : Precession.ColumnMajor;

            QcAndCellTypes = qcAndCellTypes == null
                ? LoggedInUser.GetCtQcs().ToObservableCollection()
                : qcAndCellTypes.ToObservableCollection();

            CellTypeQcName = string.IsNullOrEmpty(sampleDomain.BpQcName)
                ? QcAndCellTypes.GetCellTypeQualityControlByIndex(sampleDomain.CellTypeIndex)?.Name
                : sampleDomain.BpQcName;
            
            QcOrCellType = QcAndCellTypes.GetCellTypeQualityControlByName(CellTypeQcName);
            
            SampleName = sampleDomain.SampleID;
            WashTypes = Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToObservableCollection();
            WashType = sampleDomain.SelectedWash;
            Dilution = uint.Parse(sampleDomain.SelectedDilution);
            SampleTag = sampleDomain.Comment;
            SamplePosition = SamplePosition.Parse(sampleDomain.SampleRowPosition);
            Username = LoggedInUser.CurrentUserId; // todo: should this come from the sampleDomain?
            SampleSetName = string.Empty;
            AdvancedSampleSettings = new AdvancedSampleSettingsViewModel(sampleDomain, runOptionSettings);
        }

        public SampleViewModel(SampleEswDomain sampleEswDomain, string sampleSetName, bool useEswSampleIndexes,
            RunOptionSettingsModel runOptionSettings, List<CellTypeQualityControlGroupDomain> qcAndCellTypes = null)
        {
            SubstrateType = sampleEswDomain.SubstrateType;
            _platePrecession = sampleEswDomain.PlatePrecession;
            Uuid = sampleEswDomain.Uuid;
            SampleDataUid = sampleEswDomain.SampleDataUuid;
            SampleSetUid = sampleEswDomain.SampleSetUid;
            SampleStatus = sampleEswDomain.SampleStatus;
            SampleName = sampleEswDomain.SampleName;
            SampleTag = sampleEswDomain.SampleTag;
            SamplePosition = sampleEswDomain.SamplePosition;
            Dilution = sampleEswDomain.Dilution;
            Username = sampleEswDomain.Username;

            QcAndCellTypes = qcAndCellTypes == null 
                ? LoggedInUser.GetCtQcs().ToObservableCollection() 
                : qcAndCellTypes.ToObservableCollection();
            var baseQC = Misc.GetBaseQualityControlName(sampleEswDomain.CellTypeQcName);
            if (!baseQC.IsNullOrEmpty())
            {
                CellTypeQcName = Misc.GetParenthesisQualityControlName(baseQC, QcAndCellTypes.GetCellTypeQualityControlByIndex(sampleEswDomain.CellTypeIndex)?.Name);
                QcOrCellType = QcAndCellTypes.GetCellTypeQualityControlByName(baseQC);
            }
            else
            {
                CellTypeQcName = QcAndCellTypes.GetCellTypeQualityControlByIndex(sampleEswDomain.CellTypeIndex)?.Name;
                QcOrCellType = QcAndCellTypes.GetCellTypeQualityControlByIndex(sampleEswDomain.CellTypeIndex);
            }
            

            WashTypes = Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToObservableCollection();
            WashType = sampleEswDomain.WashType;

            SampleSetName = sampleSetName;

            if (useEswSampleIndexes)
            {
                SampleIndex = sampleEswDomain.Index;
                SampleSetIndex = sampleEswDomain.SampleSetIndex;
            }

            AdvancedSampleSettings = new AdvancedSampleSettingsViewModel(runOptionSettings);
            AdvancedSampleSettings.SampleName = SampleName;
            AdvancedSampleSettings.NthImage = sampleEswDomain.SaveEveryNthImage;

            if (sampleEswDomain.SampleRecord != null)
            {
                SetFrom(sampleEswDomain.SampleRecord, this);
            }
        }

        #endregion

        #region Properties & Fields

        public ushort SampleIndex { get; private set; } // the identifier used before the Uuid is generated in the backend
        public ushort SampleSetIndex { get; private set; } // the identifier used before the Uuid is generated in the backend

        private Precession _platePrecession;

        public SubstrateType SubstrateType
        {
            get { return GetProperty<SubstrateType>(); }
            set { SetProperty(value); }
        }
        
        public uuidDLL Uuid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        public uuidDLL SampleSetUid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        public uuidDLL SampleDataUid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        public SampleStatus SampleStatus
        {
            get { return GetProperty<SampleStatus>(); }
            set { SetProperty(value); }
        }

        public string SampleName
        {
            get { return GetProperty<string>(); }
            set
            {
                var changed = SampleName != value;
                SetProperty(value);
                if (changed)
                {
                    if (UsingSequentialSampleName) UsingSequentialSampleName = false;
                    if (AdvancedSampleSettings != null)
                    {
                        AdvancedSampleSettings.SampleName = value;
                        NotifyPropertyChanged(nameof(AdvancedSampleSettings));
                    }
                }
            }
        }

        public bool UsingSequentialSampleName // used to aid in generating a sample name from a sequence
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int SequentialSampleNumberPart // used to aid in generating a sample name from a sequence
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string CellTypeQcName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string SampleTag
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SamplePosition SamplePosition
        {
            get { return GetProperty<SamplePosition>(); }
            set { SetProperty(value); }
        }

        public double Concentration
        {
            get { return GetProperty<double>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(ConcentrationAsStr));
            }
        }

        public string ConcentrationAsStr
        {
            get { return Misc.ConvertToConcPower(Concentration);  }
        }
        
        public double ViableConcentration
        {
            get { return GetProperty<double>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(ViableConcentrationAsStr));
            }
        }

        public string ViableConcentrationAsStr
        {
            get { return Misc.ConvertToConcPower(ViableConcentration); }
        }

        public double TotalCells
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double TotalViableCells
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double TotalViability
        {
            get { return GetProperty<double>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(TotalViabilityAsStr));
            }
        }

        public string TotalViabilityAsStr
        {
            get { return Misc.UpdateTrailingPoint(TotalViability, null, TrailingPoint.One); }
        }

        public double AverageDiameter
        {
            get { return GetProperty<double>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(AvgDiamAsStr));
            }
        }

        public string AvgDiamAsStr
        {
            get { return Misc.UpdateTrailingPoint(AverageDiameter, null, TrailingPoint.Two); }
        }

        public double AverageViableDiameter
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<CellTypeQualityControlGroupDomain> QcAndCellTypes
        {
            get { return GetProperty<ObservableCollection<CellTypeQualityControlGroupDomain>>(); }
            private set { SetProperty(value); }
        }

        public CellTypeQualityControlGroupDomain QcOrCellType
        {
            get { return GetProperty<CellTypeQualityControlGroupDomain>(); }
            set
            {
                SetProperty(value);
                CellTypeQcName = value?.Name ?? string.Empty;
            }
        }

        public uint Dilution
        {
            get { return GetProperty<uint>(); }
            set
            {
                if ((value >= ApplicationConstants.MinimumDilutionFactor) &&
                    (value <= ApplicationConstants.MaximumDilutionFactor))
                {
                    SetProperty(value);
                }
            }
        }

        public ObservableCollection<SamplePostWash> WashTypes
        {
            get { return GetProperty<ObservableCollection<SamplePostWash>>(); }
            private set { SetProperty(value); }
        }

        //Do not remove. This is used for each sample added in the add sample set dialog.
        public bool IsFastModeEnabled
        {
            get 
            {
                if (SubstrateType == SubstrateType.AutomationCup) return false;
                if (IsAdminOrServiceUser) return true;
                if (LoggedInUser.IsConsoleUserLoggedIn())
                {
                    return LoggedInUser.CurrentUser.IsFastModeEnabled;
                }
                return false;
            }
            set { SetProperty(value); }
        }

        public SamplePostWash WashType
        {
            get { return GetProperty<SamplePostWash>(); }
            set { SetProperty(value); }
        }

        public SampleHierarchyType SampleHierarchy
        {
            get { return GetProperty<SampleHierarchyType>(); }
            set { SetProperty(value); }
        }

        public string Username
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string SampleSetName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SampleRecordDomain SampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
        }

        public ResultSummaryDomain SelectedResultSummary
        {
            get { return GetProperty<ResultSummaryDomain>(); }
            set { SetProperty(value); }
        }

        public AdvancedSampleSettingsViewModel AdvancedSampleSettings
        {
            get { return GetProperty<AdvancedSampleSettingsViewModel>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Methods

        public void ChangePlatePresession(Precession order)
        {
            _platePrecession = order;
        }

        public void Reload()
        {
            if (SampleRecord == null) return;
            if (SampleRecord?.UUID == null) return;
            if (SampleRecord.UUID.IsNullOrEmpty() == true) return;

            var reloaded = ResultModel.RetrieveSampleRecord(SampleRecord.UUID);
            if (reloaded == null) return;
            if (reloaded.SelectedResultSummary == null) return;
            if (reloaded.SelectedResultSummary.UUID.IsNullOrEmpty()) return;

            var resultSummary = ReviewModel.RetrieveResultSummary(reloaded.SelectedResultSummary.UUID);
            if (resultSummary == null) return;

            reloaded.SelectedResultSummary = resultSummary;
            SetFrom(reloaded, this);
        }

        public SampleDomain GenerateSampleDomain(bool rowWiseSorted)
        {
            var position = GetSamplePosition();

            var sampleDomain = new SampleDomain
            {
                SampleID = SampleName,
                SelectedWash = WashType,
                SelectedCellTypeQualityControlGroup = QcOrCellType,
                SelectedDilution = Dilution.ToString(),
                Comment = SampleTag,
                BpQcName = QcOrCellType?.Name,
                SampleStatusColor = SampleStatusColor.Defined,
                SamplePosition = position,
                SampleRowPosition = position.ToString(),
                Position = position.ToString(),
                RowWisePosition = rowWiseSorted ? ApplicationConstants.Row : ApplicationConstants.Col,
                RunSampleProgress = RunSampleProgressIndicator.eNotProcessed
            };

            if (AdvancedSampleSettings != null)
            {
                sampleDomain.NthImage = (int) AdvancedSampleSettings.NthImage;
                sampleDomain.ExportFileName = AdvancedSampleSettings.AppendExportFileName;
                sampleDomain.ExportPathForEachSample = AdvancedSampleSettings.ExportSampleDirectory;
                sampleDomain.AppendResultExport = AdvancedSampleSettings.AppendExportDirectory;
                sampleDomain.IsAppendResultExport = AdvancedSampleSettings.AppendSampleExport;
                sampleDomain.IsExportEachSampleActive = AdvancedSampleSettings.ExportSamples;
                sampleDomain.IsExportPDFSelected = AdvancedSampleSettings.ExportSamplesAsPdf;
            }

            return sampleDomain;
        }

        public SampleEswDomain GetSampleEswDomain(IDataAccess dataAccess, string username)
        {
            var sample = new SampleEswDomain();

            sample.Index = SampleIndex;
            sample.SampleSetIndex = SampleSetIndex;
            sample.TimeStamp = DateTimeConversionHelper.DateTimeToUnixSecondRounded(DateTime.Now);
            sample.Uuid = Uuid;
            sample.SampleSetUid = SampleSetUid;
            sample.SampleDataUuid = SampleDataUid;
            sample.SampleStatus = SampleStatus;
            sample.SubstrateType = SubstrateType;
            sample.PlatePrecession = _platePrecession;
            sample.SampleName = SampleName;
            sample.SampleTag = SampleTag;
            sample.SamplePosition = SamplePosition;
            sample.IsQualityControl = QcOrCellType.SelectedCtBpQcType == CtBpQcType.QualityControl;
            sample.CellTypeIndex = QcOrCellType.CellTypeIndex;
            sample.CellTypeQcName = CellTypeQcName;
            sample.Dilution = Dilution;
            sample.WashType = WashType;
            sample.Username = Username;

            // AdvancedSampleSettings, essentially another SampleViewModel, contains an NthImage value which could
            // be populated from other sources such as carousel (SampleDomain), which in turn was populated by the
            // Keep Every Nth Image (NumberOfImages) in the Settings -> Run Options dialog, but may have been overridden
            // in the sample set. If an AdvancedSampleSettings exists, use it, otherwise drop back and use the value
            // from the Run Options.
            if (null != AdvancedSampleSettings)
            {
                sample.SaveEveryNthImage = AdvancedSampleSettings.NthImage;
            }
            else
            {
                var runOptionSettings = new RunOptionSettingsModel(dataAccess, username);
                sample.SaveEveryNthImage = uint.Parse(runOptionSettings.RunOptionsSettings.NumberOfImages);
            }

            return sample;
        }

        public SamplePosition GetSamplePosition()
        {
            return SamplePosition.Parse(SamplePosition.ToString());
        }

        public void SetSampleIndex(ushort index)
        {
            SampleIndex = index;
        }

        public void SetSampleSetIndex(ushort setIndex)
        {
            SampleSetIndex = setIndex;
        }

        #endregion

        #region IClonable

        public object Clone()
        {
            var clone = (SampleViewModel) MemberwiseClone();
            CloneBaseProperties(clone);

            clone.QcAndCellTypes = new ObservableCollection<CellTypeQualityControlGroupDomain>();
            foreach (var item in QcAndCellTypes)
            {
                clone.QcAndCellTypes.Add((CellTypeQualityControlGroupDomain) item.Clone());
            }
            clone.QcOrCellType = clone.QcAndCellTypes.FirstOrDefault(i => i.Equals(QcOrCellType));

            clone.WashTypes = Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToObservableCollection();
            clone.WashType = clone.WashTypes.FirstOrDefault(w => w.Equals(WashType));

            return clone;
        }

        #endregion

        #region Static Methods

        public static void SetFrom(SampleRecordDomain sampleRecord, SampleViewModel sampleVm,
            SampleHierarchyType? hierarchyType = null)
        {
            if ((sampleRecord?.SelectedResultSummary == null) || (sampleRecord?.UUID == null))
                return;
            if (sampleRecord.UUID.IsNullOrEmpty())
                return;

            sampleVm.SampleRecord = sampleRecord;
            if(!string.IsNullOrEmpty(sampleRecord.SampleIdentifier))
                sampleVm.SampleName = sampleRecord.SampleIdentifier;
            if (uint.TryParse(sampleRecord.DilutionName, out var dilution)) 
                sampleVm.Dilution = dilution;
            if (!string.IsNullOrEmpty(sampleRecord.Tag))
                sampleVm.SampleTag = sampleRecord.Tag;

            sampleVm.WashType = sampleRecord.WashName;

            SetFromResultSummary(sampleVm, sampleRecord.SelectedResultSummary, sampleRecord.BpQcName);
            sampleVm.SampleHierarchy = hierarchyType ?? GetSampleHierarchyType(sampleRecord);
            sampleRecord.Position = sampleVm.SamplePosition;
        }

        private static void SetFromResultSummary(SampleViewModel sampleVm, ResultSummaryDomain resultSummary,
            string bpQcName)
        {
            if (resultSummary == null)
            {
                Log.Warn($"resultSummary is null");
                return;
            }

            if (!string.IsNullOrEmpty(bpQcName))
            {
                sampleVm.QcOrCellType = sampleVm.QcAndCellTypes.GetCellTypeQualityControlByName(bpQcName);
            }
            else if (resultSummary.CellTypeDomain != null)
            {
                sampleVm.QcOrCellType = sampleVm.QcAndCellTypes.GetCellTypeQualityControlByName(resultSummary.CellTypeDomain.CellTypeName);
            }

            sampleVm.SelectedResultSummary = resultSummary;
            sampleVm.SelectedResultSummary.SignatureList = resultSummary.SignatureList;
            sampleVm.SelectedResultSummary.SelectedSignature = resultSummary.SignatureList?.LastOrDefault();

            if (resultSummary.CumulativeResult != null)
            {
                sampleVm.AverageDiameter = resultSummary.CumulativeResult.Size;
                sampleVm.AverageViableDiameter = resultSummary.CumulativeResult.ViableSize;
                sampleVm.TotalCells = resultSummary.CumulativeResult.TotalCells;
                sampleVm.TotalViability = resultSummary.CumulativeResult.Viability;
                sampleVm.TotalViableCells = resultSummary.CumulativeResult.ViableCells;
                sampleVm.Concentration = resultSummary.CumulativeResult.ConcentrationML;
                sampleVm.ViableConcentration = resultSummary.CumulativeResult.ViableConcentration;
            }

            if (resultSummary.CellTypeDomain != null)
            {
                if (string.IsNullOrEmpty(bpQcName))
                {
                    sampleVm.CellTypeQcName = resultSummary.CellTypeDomain.CellTypeName;
                }
                else
                {
                    var baseQcName = Misc.GetBaseQualityControlName(bpQcName);
                    sampleVm.CellTypeQcName = Misc.GetParenthesisQualityControlName(baseQcName, resultSummary.CellTypeDomain.CellTypeName);
                }
            }
        }

        public static SampleHierarchyType GetSampleHierarchyType(SampleRecordDomain sample)
        {
            if (sample.ResultSummaryList.Count > 1)
            {
                return sample.SelectedResultSummary.UUID.Equals(sample.ResultSummaryList.First().UUID)
                    ? SampleHierarchyType.Parent
                    : SampleHierarchyType.Child;
            }

            return SampleHierarchyType.None;
        }

        #endregion
    }
}