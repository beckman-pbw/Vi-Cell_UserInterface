using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public static class SampleSet
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static extern HawkeyeError GetSampleSetList(eFilterItem filterItem, ulong fromDate, ulong toDate, 
            string userId, string nameSearchString, string tagSearchString, string cellTypeOrQualityControlName,
            uint skip, uint take, out uint totalQueryResultCount, out IntPtr sampleSetListIntPtr, out uint listSize);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static extern HawkeyeError GetSampleSetAndSampleList(uuidDLL sampleSetUuidDll, out IntPtr sampleSetIntPtr);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static extern HawkeyeError GetSampleDefinitionBySampleId(uuidDLL sampleUuidDll, out IntPtr sampleDefinitionIntPtr);

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeSampleDefinition(IntPtr sampleDefinitionIntPtr, uint numberOfSamples);

        [DllImport(ApplicationConstants.DllName)]
        public static extern void FreeSampleSet(IntPtr sampleSetIntPtr, uint numSampleSets);

        #endregion

        #region API_Calls

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError GetSampleDefinitionBySampleIdApiCall(uuidDLL sampleUuidDll, out SamplePosition samplePosition)
        {
            var hawkeyeError = GetSampleDefinitionBySampleId(sampleUuidDll, out var sampleDefinitionPtr);
            
            if (hawkeyeError != HawkeyeError.eSuccess || sampleDefinitionPtr.Equals(IntPtr.Zero))
            {
                Log.Error($"GetSampleDefinitionBySampleId({sampleUuidDll},...) = {hawkeyeError}");
                samplePosition = new SamplePosition();
                return hawkeyeError;
            }
            
            Log.Debug($"GetSampleDefinitionBySampleId({sampleUuidDll},...) = {hawkeyeError}");
            var sampleDefinition = (SampleDefinition)Marshal.PtrToStructure(sampleDefinitionPtr, typeof(SampleDefinition));
            samplePosition = sampleDefinition.position;

            FreeSampleDefinition(sampleDefinitionPtr, 1);

            return hawkeyeError;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError GetSampleSetListApiCall(eFilterItem filterItem, ulong fromDate, ulong toDate,
            string userId, string nameSearchString, string tagSearchString, string cellTypeOrQualityControlName,
            uint skip, uint take, out uint totalQueryResultCount, out List<SampleSetDomain> sampleSets)
        {
            var result = GetSampleSetList(filterItem, fromDate, toDate, userId, nameSearchString, tagSearchString,
                cellTypeOrQualityControlName, skip, take, out totalQueryResultCount, out var sampleSetListPtr,
                out var listSize);

            if (result != HawkeyeError.eSuccess || sampleSetListPtr.Equals(IntPtr.Zero))
            {
                Log.Error($"GetSampleSetList({filterItem},{fromDate},{toDate},{userId},{nameSearchString}," +
                          $"{tagSearchString},{cellTypeOrQualityControlName},{skip},{take},...) = {result}");

                totalQueryResultCount = 0;
                listSize = 0;
                sampleSets = new List<SampleSetDomain>();
                return result;
            }

            Log.Debug($"GetSampleSetList({filterItem},{fromDate},{toDate},{userId},{nameSearchString}," +
                      $"{tagSearchString},{cellTypeOrQualityControlName},{skip},{take},...) = {result}");

            sampleSets = sampleSetListPtr.MarshalToSampleSetDomainList(listSize);

            FreeSampleSet(sampleSetListPtr, listSize);

            return result;
        }


        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError GetSampleSetAndSampleListApiCall(uuidDLL sampleSetUuidDll, 
            out SampleSetDomain sampleSetDomain, bool getSampleResults = true)
        {
            var hawkeyeError = GetSampleSetAndSampleList(sampleSetUuidDll, out var sampleSetIntPtr);

            Log.Debug($"GetSampleSetAndSampleList({sampleSetUuidDll},...) = {hawkeyeError}");

			if (hawkeyeError != HawkeyeError.eSuccess || sampleSetIntPtr.Equals(IntPtr.Zero))
            {
                sampleSetDomain = null;
                return hawkeyeError;
            }

            sampleSetDomain = sampleSetIntPtr.MarshalToSampleSetDomain();

            FreeSampleSet(sampleSetIntPtr, 1);

            if (getSampleResults)
            {
                var resultUuids = sampleSetDomain.Samples.Select(s => s.SampleDataUuid).ToArray();
                hawkeyeError = Sample.RetrieveSampleRecordListAPI(resultUuids, out var sampleRecords, out var count);
                if (hawkeyeError == HawkeyeError.eSuccess)
                {
                    foreach (var sampleRecord in sampleRecords)
                    {
                        var sample = sampleSetDomain.Samples.FirstOrDefault(s => s.SampleDataUuid.Equals(sampleRecord.UUID));
                        if (sample == null)
                        {
                            Log.Warn($"Unable to find matching {nameof(SampleEswDomain)} for {nameof(SampleEswDomain.SampleDataUuid)} ({sampleRecord.UUID}).");
                            continue;
                        }

                        sample.SampleRecord = sampleRecord;
                    }
                }
                else
                {
                    Log.Warn($"{nameof(Sample.RetrieveSampleRecordList)}() failed: {hawkeyeError}");
                }
            }
            
            return hawkeyeError;
        }

        #endregion
    }
}