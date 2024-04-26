using ScoutUtilities.Common;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScoutDomains.Analysis
{
    public class AnalysisDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public AnalysisDomain()
        {
            AnalysisParameter = new List<AnalysisParameterDomain>();
        }

        public uint AnalysisIndex { get; set; }
        public string Label { get; set; }
        public int MixingCycle { get; set; }

        public List<AnalysisParameterDomain> AnalysisParameter
        {
            get { return GetProperty<List<AnalysisParameterDomain>>(); }
            set { SetProperty(value); }
        }

        public bool IsAnalysisEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public object Clone()
        {
            var clone = (AnalysisDomain) MemberwiseClone();
            CloneBaseProperties(clone);

            if (AnalysisParameter != null)
                clone.AnalysisParameter = AnalysisParameter.Select(x => (AnalysisParameterDomain)x.Clone()).ToList();
            return clone;
        }

        public static AnalysisDomain GetAnalysisDomain(AnalysisDefinition analysis)
        {
            var list = new List<AnalysisParameter>();
            var sz = Marshal.SizeOf(typeof(AnalysisParameter));
            for (int i = 0; i < analysis.num_analysis_parameters; i++)
            {
                list.Add((AnalysisParameter)Marshal.PtrToStructure(analysis.analysis_parameters, typeof(AnalysisParameter)));
                analysis.analysis_parameters += sz;
            }

            var appItem = new AnalysisDomain()
            {
                AnalysisIndex = analysis.Analysis_index,
                MixingCycle = analysis.mixing_cycles,
                Label = analysis.label.ToSystemString(),
                IsAnalysisEnable = false
            };

            if (list.Count > 0)
            {
                list.ForEach(ap =>
                {
                    appItem.AnalysisParameter.Add(new AnalysisParameterDomain()
                    {
                        AboveThreshold = ap.above_threshold,
                        Characteristic = ap.characteristic,
                        Label = ap.label.ToSystemString(),
                        ThresholdValue = ap.threshold_value
                    });
                });
            }

            return appItem;
        }

        public AnalysisDefinition GetAnalysisDefinition()
        {
            var analysis = new AnalysisDefinition
            {
                Analysis_index = (ushort) AnalysisIndex,
                label = Label.ToIntPtr(),
                mixing_cycles = (byte) MixingCycle,
                num_analysis_parameters = (byte) AnalysisParameter.Count,
                analysis_parameters = GetAnalysisPointer()
            };
            return analysis;
        }

        public IntPtr GetAnalysisPointer()
        {
            var apStruct = new List<AnalysisParameter>();
            AnalysisParameter.ForEach(ap =>
            {
                apStruct.Add(new AnalysisParameter
                {
                    above_threshold = ap.AboveThreshold,
                    threshold_value = ap.ThresholdValue ?? default(float),
                    label = ap.Label.ToIntPtr(),
                    characteristic = ap.Characteristic
                });
            });

            var wqiSize = Marshal.SizeOf(typeof(AnalysisParameter));
            IntPtr cellTypeIndicesPtr = Marshal.AllocCoTaskMem(apStruct.Count * wqiSize);
            for (int i = 0; i < apStruct.Count; i++)
            {
                var pTmp = cellTypeIndicesPtr + (i * wqiSize);
                Marshal.StructureToPtr(apStruct[i], pTmp, false);
            }

            return cellTypeIndicesPtr;
        }
    }
}
