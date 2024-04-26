using ScoutDomains.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutDomains.EnhancedSampleWorkflow
{
    public class SampleEswDomain : BaseNotifyPropertyChanged
    {
        public SampleEswDomain()
        {
            
        }

        public SampleEswDomain(SampleDomain sampleDomain, ushort sampleIndex, ushort sampleSetIndex, string username,
            SubstrateType substrate, Precession precession, DateTime dtCreated, SampleStatus status)
        {
            Index = sampleIndex;
            SampleSetIndex = sampleSetIndex;
            Username = username;
            SubstrateType = substrate;
            PlatePrecession = precession;
            SampleStatus = status;
            TimeStamp = DateTimeConversionHelper.DateTimeToUnixSecondRounded(dtCreated);

            SampleName = sampleDomain.SampleName;
            SamplePosition = sampleDomain.SamplePosition;
            SampleTag = sampleDomain.Comment;

            IsQualityControl = sampleDomain.SelectedCellTypeQualityControlGroup.SelectedCtBpQcType == CtBpQcType.QualityControl;

            if (string.IsNullOrEmpty(sampleDomain.BpQcName))
            {
                // for some reason BpQcName and CellTypeIndex do not get set correctly
                CellTypeIndex = sampleDomain.SelectedCellTypeQualityControlGroup.CellTypeIndex;
                CellTypeQcName = sampleDomain.SelectedCellTypeQualityControlGroup.Name;
            }
            else
            {
                CellTypeIndex = sampleDomain.CellTypeIndex;
                CellTypeQcName = sampleDomain.BpQcName;
            }
            
            Dilution = uint.Parse(sampleDomain.SelectedDilution);
            WashType = sampleDomain.SelectedWash;
            SaveEveryNthImage = (uint) sampleDomain.NthImage;
        }

        #region Properties & Fields

        public ushort Index // the identifier used before the Uuid is generated in the backend
        {
            get { return GetProperty<ushort>(); }
            set { SetProperty(value); }
        }

        public ushort SampleSetIndex // the identifier used before the Uuid is generated in the backend
        {
            get { return GetProperty<ushort>(); }
            set { SetProperty(value); }
        }

        public ulong TimeStamp
        {
            get { return GetProperty<ulong>(); }
            set { SetProperty(value); }
        }

        public uuidDLL Uuid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        public uuidDLL SampleDataUuid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        public uuidDLL SampleSetUid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        public SampleStatus SampleStatus
        {
            get { return GetProperty<SampleStatus>(); }
            set { SetProperty(value); }
        }

        public SubstrateType SubstrateType
        {
            get { return GetProperty<SubstrateType>(); }
            set { SetProperty(value); }
        }

        public Precession PlatePrecession
        {
            get { return GetProperty<Precession>(); }
            set { SetProperty(value); }
        }

        public string SampleName
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

        public bool IsQualityControl
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public uint CellTypeIndex
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public string CellTypeQcName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
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

        public SamplePostWash WashType
        {
            get { return GetProperty<SamplePostWash>(); }
            set { SetProperty(value); }
        }

        public string Username
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public uint SaveEveryNthImage
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public SampleRecordDomain SampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

        public SampleDefinition GetSampleDefinitionStruct()
        {
            var sample = new SampleDefinition();

            sample.index = Index;
            sample.sampleSetIndex = SampleSetIndex;
            sample.position = SamplePosition;
            sample.sampleSetUuid = SampleSetUid;
            sample.sampleDefUuid = Uuid;
            sample.sampleDataUuid = SampleDataUuid;
            sample.status = SampleStatus;
            sample.username = Username.ToIntPtr();
            sample.timestamp = TimeStamp;
            
            sample.parameters = new SampleParameters();
            sample.parameters.Tag = SampleTag.ToIntPtr();
            sample.parameters.bp_qc_name = IsQualityControl
                ? Misc.GetBaseQualityControlName(CellTypeQcName).ToIntPtr()
                : IntPtr.Zero;
            sample.parameters.CellTypeIndex = CellTypeIndex;
            sample.parameters.AnalysisIndex = ApplicationConstants.AnalysisIndex;
            sample.parameters.DilutionFactor = Dilution;
            sample.parameters.Label = SampleName.ToIntPtr();
            sample.parameters.SaveEveryNthImage = SaveEveryNthImage;
            sample.parameters.WashType = WashType;

            return sample;
        }

        public SampleRecordDomain GenerateSampleRecordDomainFrom(List<KeyValuePair<string,string>> parameterList, string bpQcName)
        {
            var sampleRecord = new SampleRecordDomain
            {
                BpQcName = bpQcName,
                ShowParameterList = parameterList,
                UUID = SampleDataUuid,
                SampleIdentifier = SampleName,
                WashName = WashType,
                DilutionName = Misc.ConvertToString(Dilution),
                Position = SamplePosition,
                SelectedResultSummary = null,
                SelectedSampleImageRecord = null,
                IsSampleCompleted = false,
                Tag = SampleTag,
                UserId = Username,
                TimeStamp = TimeStamp,
                RetrieveDate = DateTime.Now,
            };

            return sampleRecord;
        }

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }

        #endregion
    }
}