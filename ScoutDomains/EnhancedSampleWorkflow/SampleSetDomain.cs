using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutDomains.EnhancedSampleWorkflow
{
    public class SampleSetDomain : BaseNotifyPropertyChanged
    {
        public uuidDLL Uuid;
        public ushort Index;
        public ulong Timestamp;
        public SubstrateType Carrier;
        public string SampleSetName;
        public string Username;
        public SampleSetStatus SampleSetStatus;
        public IList<SampleEswDomain> Samples;
        public Precession PlatePrecession;

        public SampleSet GetSampleSetStruct()
        {
            var sampleSet = new SampleSet();

            sampleSet.carrier = Carrier;
            sampleSet.index = Index;
            sampleSet.timestamp = Timestamp;
            sampleSet.uuid = Uuid;
            sampleSet.name = SampleSetName.ToIntPtr();
            sampleSet.username = Username.ToIntPtr();
            sampleSet.setStatus = SampleSetStatus;
            sampleSet.numSamples = (ushort) Samples.Count;
            sampleSet.samples = new IntPtr();
            sampleSet.platePrecession = PlatePrecession;
            
            return sampleSet;
        }

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }

        public int NumSamplesNotYetRun()
        {
            if (Samples == null || !Samples.Any())
                return 0;
            return Samples.Count(s => s.SampleStatus == SampleStatus.NotProcessed);
        }
    }
}