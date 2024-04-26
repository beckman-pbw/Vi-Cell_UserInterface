using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using ScoutUtilities;

namespace ScoutDomains.EnhancedSampleWorkflow
{
    public class WorkListDomain : BaseNotifyPropertyChanged
    {
        public uuidDLL Uuid;
        public SubstrateType Carrier;
        public Precession Precession;
        public string Username;
        public string RunByUsername;
        public string Label;
        public string Tag;
        public ulong TimeStamp;
        public SamplePostWash Wash;
        public uint CellTypeIndex;
        public string QualityControlName;
        public uint DilutionFactor;
        public uint SaveEveryNthImage; //For orphans
        public SequenceParametersDomain SequenceParameters;
        public IList<SampleSetDomain> SampleSets;

        public WorkList GetWorkListStruct()
        {
            var workList = new WorkList();

            workList.UseSequencing = SequenceParameters.UseSequencing ? (byte) 1 : (byte) 0;
            workList.SequencingBaseLabel = SequenceParameters.SequencingBaseLabel.ToIntPtr();
            workList.SequencingNumberOfDigits = SequenceParameters.SequencingNumberOfDigits;
            workList.SequencingStartingDigit = SequenceParameters.SequencingStartingDigit;
            workList.SequencingTextFirst = SequenceParameters.SequencingTextFirst ? (byte) 1 : (byte) 0;

            workList.uuid = Uuid;
            workList.carrier = Carrier;
            workList.precession = Precession;
            workList.label = (string.IsNullOrEmpty(Label) ? DateTime.Now.ToString("yyMMdd_Hmmss") : Label).ToIntPtr();
            workList.numSampleSets = (ushort)SampleSets.Count;
            workList.timestamp = TimeStamp;
            workList.username = Username.ToIntPtr();
            workList.runUserId = RunByUsername.ToIntPtr();

            workList.defaultParameterSettings = new SampleParameters();
            workList.defaultParameterSettings.WashType = Wash;
            workList.defaultParameterSettings.AnalysisIndex = ApplicationConstants.AnalysisIndex;
            workList.defaultParameterSettings.CellTypeIndex = CellTypeIndex;
            workList.defaultParameterSettings.bp_qc_name = Misc.GetBaseQualityControlName(QualityControlName).ToIntPtr();
            workList.defaultParameterSettings.DilutionFactor = DilutionFactor;
            workList.defaultParameterSettings.SaveEveryNthImage = SaveEveryNthImage;
            workList.defaultParameterSettings.Tag = Tag.ToIntPtr();
            
            workList.sampleSets = IntPtr.Zero;

            return workList;
        }

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }
    }
}