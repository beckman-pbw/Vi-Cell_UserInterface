using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using ScoutDomains;
using ScoutDomains.Analysis;

namespace HawkeyeCoreAPI
{
    public static partial class Analysis
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetUserAnalysisIndices(string name, UInt32 nAD, IntPtr analysis_indices);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetUserAnalysisIndices(string name, out UInt32 nAD, out NativeDataType tag, out IntPtr ptrAnalysisIndices);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetAllAnalysisDefinitions(out UInt32 numAnalyses, out IntPtr ptrAnalyses);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetFactoryAnalysisDefinitions(out UInt32 nAD, out IntPtr ptrAnalyses);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetUserAnalysisDefinitions(out UInt32 nAD, out IntPtr ptrAnalyses);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RemoveAnalysisSpecializationForCellType(UInt16 adIndex, UInt32 ctIndex);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeAnalysisDefinitions(IntPtr list, UInt32 numAnalyses);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetAnalysisForCellType(UInt16 adIndex, UInt32 ctIndex, out IntPtr ptrAnalysisDefinition);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SpecializeAnalysisForCellType(AnalysisDefinition ad, UInt32 ctIndex);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetTemporaryAnalysisDefinition(out IntPtr temp_definition);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetTemporaryAnalysisDefinition(IntPtr temp_definition);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetTemporaryAnalysisDefinitionFromExisting(UInt16 analysis_index);

        #endregion


        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetAllAnalysesAPI(ref List<AnalysisDomain> analyses)
        {
            IntPtr ptrAnalysisDefinition;

            uint numAnalyses = 0;
            var hawkeyeError = GetAllAnalysisDefinitions(out numAnalyses, out ptrAnalysisDefinition);
            if (ptrAnalysisDefinition != IntPtr.Zero && analyses != null)
            {
                var ptrCopy = ptrAnalysisDefinition;
                var sz = Marshal.SizeOf(typeof(AnalysisDefinition));
                for (int i = 0; i < numAnalyses; i++)
                {
                    analyses.Add(CreateAnalysisDomain(ptrCopy));
                    ptrCopy += sz;
                }

                FreeAnalysisDefinitions(ptrAnalysisDefinition, numAnalyses);
            }

            return hawkeyeError;
        }

        public static HawkeyeError GetFactoryAnalysesAPI(ref List<AnalysisDomain> factoryAnalyses)
        {
            IntPtr ptrAnalysisDefinition;

            uint numAnalyses = 0;
            var hawkeyeError = GetFactoryAnalysisDefinitions(out numAnalyses, out ptrAnalysisDefinition);

            var ptrCopy = ptrAnalysisDefinition;
            var sz = Marshal.SizeOf(typeof(AnalysisDefinition));
            for (int i = 0; i < numAnalyses; i++)
            {
                factoryAnalyses.Add(CreateAnalysisDomain(ptrCopy));
                ptrCopy += sz;
            }

            FreeAnalysisDefinitions(ptrAnalysisDefinition, numAnalyses);

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetUserAnalysesAPI(ref uint nAD, ref List<AnalysisDefinition> userAnalyses) //not required 
        {
            IntPtr ptrAnalysisDefinition;

            var hawkeyeError = GetUserAnalysisDefinitions(out nAD, out ptrAnalysisDefinition);
            var analysisPtr = ptrAnalysisDefinition;

            for (int i = 0; i < nAD; i++)
            {
                userAnalyses.Add((AnalysisDefinition)Marshal.PtrToStructure(analysisPtr, typeof(AnalysisDefinition)));
                analysisPtr = new IntPtr(analysisPtr.ToInt64() + Marshal.SizeOf(typeof(AnalysisDefinition)));
            }

            if (nAD > 0)
            {
                FreeAnalysisDefinitionsAPI(ptrAnalysisDefinition, nAD);
            }

            return hawkeyeError;
        }
        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SpecializeAnalysisForCellTypeAPI(AnalysisDefinition ad, UInt32 ctIndex)
        {
            return SpecializeAnalysisForCellType(ad, ctIndex);
        }

        public static void FreeAnalysisDefinitionsAPI(IntPtr list, uint num_analyses)
        {
            FreeAnalysisDefinitions(list, num_analyses);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserAnalysesAPI(string name, uint nAD, List<ushort> analysis_indices)
        {
            IntPtr analysisIndicesPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(UInt16)));
            for (int i = 0; i < analysis_indices.Count; i++)
            {
                Marshal.StructureToPtr(analysis_indices[i], analysisIndicesPtr, false);
            }

            return SetUserAnalysisIndices(name, nAD, analysisIndicesPtr);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetUserAnalysisIndicesAPI(string name, ref uint nAD, ref List<int> analysisIndices)
        {
            IntPtr ptrAnalysisIndices;
            NativeDataType tag;

            HawkeyeError hawkeyeError = GetUserAnalysisIndices(name, out nAD, out tag, out ptrAnalysisIndices);
            if (nAD > 0)
            {
                int[] indices = new int[nAD];
                Marshal.Copy(ptrAnalysisIndices, indices, 0, (int)nAD);

                for (int i = 0; i < nAD; i++)
                    analysisIndices.Add(indices[i]);

                GenericFree.FreeListOfTaggedBuffersAPI(tag, ptrAnalysisIndices, nAD);
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetAnalysisForCellTypeAPI(UInt16 adIndex, uint ctIndex, ref AnalysisDomain analysisDomain)
        {
            IntPtr ptrAnalysisDefinition;
            var hawkeyeError = GetAnalysisForCellType(adIndex, ctIndex, out ptrAnalysisDefinition);
            if (ptrAnalysisDefinition != IntPtr.Zero)
            {
                analysisDomain = CreateAnalysisDomain(ptrAnalysisDefinition);
                FreeAnalysisDefinitions(ptrAnalysisDefinition, 1);
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RemoveAnalysisSpecializationForCellTypeAPI(UInt16 adIndex, uint ctIndex)
        {
            return RemoveAnalysisSpecializationForCellType(adIndex, ctIndex);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetTemporaryAnalysisDefinitionAPI(ref List<AnalysisDomain> analysisList)
        {
            IntPtr intPtr;
            var hawkeyeError = GetTemporaryAnalysisDefinition(out intPtr);
            if (intPtr != IntPtr.Zero && analysisList != null)
            {
                analysisList.Add(CreateAnalysisDomain(intPtr));
                Analysis.FreeAnalysisDefinitionsAPI(intPtr, (uint)analysisList.Count);
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetTemporaryAnalysisDefinitionAPI(AnalysisDefinition analysis)
        {
            var analysisPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(AnalysisDefinition)));
            Marshal.StructureToPtr(analysis, analysisPtr, false);
            var hawkeyeStatus = SetTemporaryAnalysisDefinition(analysisPtr);
            Marshal.FreeCoTaskMem(analysisPtr);
            return hawkeyeStatus;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetTemporaryAnalysisDefinitionFromExistingAPI(UInt16 analysis_index)
        {
            return SetTemporaryAnalysisDefinitionFromExisting(analysis_index);
        }

        [HandleProcessCorruptedStateExceptions]
        private static AnalysisDomain CreateAnalysisDomain(IntPtr intPtr)
        {
            var appDomain = new AnalysisDomain();
            if (intPtr == IntPtr.Zero)
            {
                return appDomain;
            }

            var analysisDefinition = (AnalysisDefinition)Marshal.PtrToStructure(intPtr, typeof(AnalysisDefinition));

            var analysisParameterList = new List<AnalysisParameter>();
            var sz = Marshal.SizeOf(typeof(AnalysisParameter));
            for (int i = 0; i < analysisDefinition.num_analysis_parameters; i++)
            {
                analysisParameterList.Add((AnalysisParameter)Marshal.PtrToStructure(analysisDefinition.analysis_parameters, typeof(AnalysisParameter)));
                analysisDefinition.analysis_parameters += sz;
            }

            var analysisDomain = new AnalysisDomain()
            {
                AnalysisIndex = analysisDefinition.Analysis_index,
                MixingCycle = (int)analysisDefinition.mixing_cycles,
                Label = analysisDefinition.label.ToSystemString(),
                IsAnalysisEnable = false
            };

            if (analysisParameterList.Count > 0)
            {
                analysisParameterList.ForEach(ap =>
                {
                    analysisDomain.AnalysisParameter.Add(new AnalysisParameterDomain()
                    {
                        AboveThreshold = ap.above_threshold,
                        Characteristic = ap.characteristic,
                        Label = ap.label.ToSystemString(),
                        ThresholdValue = ap.threshold_value
                    });
                });
            }

            return analysisDomain;
        }

        #endregion


        #region Private Methods

        #endregion
    }

}
