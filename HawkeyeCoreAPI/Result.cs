using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutDomains.ClusterDomain;
using ScoutDomains.RunResult;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ScoutUtilities;

namespace HawkeyeCoreAPI
{
    public static partial class Result
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        public static extern void FreeListOfResultSummary(IntPtr list_, UInt32 n_items);

        [DllImport("HawkeyeCore.dll")]
        public static extern void FreeListOfResultRecord(IntPtr list_, UInt32 n_items);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveResultRecord(uuidDLL id, out IntPtr rec);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveResultRecords(UInt64 start, UInt64 end, string userId, out IntPtr reclist, out UInt32 listSize);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveResultSummaryRecordList(IntPtr ids, UInt32 list_size, out IntPtr recs, out UInt32 retrieved_size);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveResultRecordList(IntPtr ids, UInt32 list_size, out IntPtr recs, out UInt32 retrieved_size);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveHistogramForResultRecord(uuidDLL id, bool POI, Characteristic_t measurement, out byte bin_count, out IntPtr bins);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveDetailedMeasurementsForResultRecord(uuidDLL id, out IntPtr measurements);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeDetailedResultMeasurement(IntPtr detailed_res);

        [DllImport("HawkeyeCore.dll")]
        static extern HawkeyeError FreeHistogramBins(IntPtr hist_bins);

        [DllImport("HawkeyeCore.dll")]
        static extern HawkeyeError DeleteCampaignData();

		#endregion


		#region API_Calls

		[MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveResultSummaryRecordAPI(uuidDLL id, out ResultSummaryDomain summary)
        {
            List<ResultSummaryDomain> resultSummaryList;
            var he = RetrieveResultSummaryRecordListAPI(new[] { id }, out resultSummaryList);
            summary = (resultSummaryList?.Any() == false) ? null : resultSummaryList.FirstOrDefault();
            return he;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveResultSummaryRecordListAPI(uuidDLL[] summaryIds, out List<ResultSummaryDomain> resultSummaryList)
        {
            var uuidsPtr = summaryIds.MarshalToIntPtr();

            IntPtr summaryListPtr;
            uint numRetrieved;
            var error = RetrieveResultSummaryRecordList(uuidsPtr.Raw, (uint)summaryIds.Length, out summaryListPtr, out numRetrieved);

            if (error != HawkeyeError.eSuccess)
            {
                if (summaryIds.Any())
                {
                    var idsStr = string.Join(", ", summaryIds);
                    Log.Warn($"RetrieveResultSummaryRecordListAPI: {idsStr}), {error}");
                }
                else
                {
                    Log.Warn($"RetrieveResultSummaryRecordListAPI:: no elements found, {error}");
                }
            }

            resultSummaryList = summaryListPtr.MarshalToResultSummaryDomainList(numRetrieved);

            if (numRetrieved > 0)
            {
                FreeListOfResultSummaryAPI(summaryListPtr, numRetrieved);
            }

            return error;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveResultRecordAPI(uuidDLL id, out ResultRecordDomain rec)
        {
            IntPtr ptrResultRecord;
            var hawkeyeError = RetrieveResultRecord(id, out ptrResultRecord);
            if (ptrResultRecord != IntPtr.Zero)
            {
                rec = ptrResultRecord.MarshalToResultRecordDomain();
            }
            else
            {
                Log.Warn($"RetrieveResultRecordAPI: {id}: {hawkeyeError}");
                rec = new ResultRecordDomain();
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveResultRecordListAPI(uuidDLL[] recordIds, out List<ResultRecordDomain> resultRecordList)
        {
            var uuidsPtr = recordIds.MarshalToIntPtr();

            IntPtr recordListPtr;
            uint numRetrieved;
            var error = RetrieveResultRecordList(uuidsPtr.Raw, (uint)recordIds.Length, out recordListPtr, out numRetrieved);

            if (error != HawkeyeError.eSuccess)
            {
                if (recordIds.Any())
                {
                    var idsStr = string.Join(", ", recordIds);
                    Log.Warn($"RetrieveResultRecordListAPI: {idsStr}, {error}");
                }
                else
                {
                    Log.Warn($"RetrieveResultRecordListAPI:: no elements found, {error}");
                }
            }

            resultRecordList = recordListPtr.MarshalToResultRecordDomains(numRetrieved);

            if (numRetrieved > 0)
            {
                FreeListOfResultRecord(recordListPtr, numRetrieved);
            }

            return error;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveDetailedMeasurementsForResultRecordAPI(uuidDLL id, out DetailedResultMeasurementsDomain detailedMeasurements)
        {
            //return RecordDataHelpers.RetrieveDetailedMeasurementsForResultRecordAPI(id, out detailedMeasurements);
            IntPtr intPtr;
            var he = RetrieveDetailedMeasurementsForResultRecord(id, out intPtr);
            if (intPtr != IntPtr.Zero)
            {
                detailedMeasurements = intPtr.MarshalToDetailedResultMeasurementDomain();
                FreeDetailedResultMeasurement(intPtr);
            }
            else
            {
                detailedMeasurements = new ScoutDomains.ClusterDomain.DetailedResultMeasurementsDomain();
                Misc.LogOnHawkeyeError($"RetrieveDetailedMeasurementsForResultRecordAPI: ({id})", he);
            }

			return he;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveHistogramForResultRecordAPI(
            uuidDLL id, 
            bool POI, 
            Characteristic_t measurement,
            byte bin_count, 
            out List<histogrambin_t> HistogramForResultRecord)
        {
            //return RecordDataHelpers.RetrieveHistogramForResultRecordAPI(id, POI, measurement, bin_count, out HistogramForResultRecord);
            IntPtr ptrHistogramForResultRecord;
            var hawkeyeError = RetrieveHistogramForResultRecord(
                id, POI, measurement, out bin_count, out ptrHistogramForResultRecord);
            if (ptrHistogramForResultRecord != IntPtr.Zero && hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                HistogramForResultRecord = ptrHistogramForResultRecord.MarshalToHistogrambins(bin_count);
            }
            else
            {
                Log.Warn($"RetrieveHistogramForResultRecordAPI: {id}, {POI}, {measurement}, {hawkeyeError}");
                HistogramForResultRecord = new List<histogrambin_t>();
            }

            var freeErr = FreeHistogramBins(ptrHistogramForResultRecord);
            if (freeErr != HawkeyeError.eSuccess)
            {
                Log.Warn("RetrieveHistogramForResultRecordAPI: FreeHistogramBins failed");
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError DeleteCampaignDataAPI()
        {
	        return DeleteCampaignData();
        }

        #endregion


		#region Private Methods

		private static void FreeListOfResultSummaryAPI(IntPtr str, uint size)
        {
            Log.Debug($"FreeListOfResultSummaryAPI:: size: {size}");
            FreeListOfResultSummary(str, size);
        }

        #endregion

    }
}
