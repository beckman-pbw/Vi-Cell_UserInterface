using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ScoutUtilities;

namespace HawkeyeCoreAPI
{
    public static partial class Sample
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveAnnotatedImage(uuidDLL result_id, uuidDLL image_id, out IntPtr img);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveBWImage(uuidDLL image_id, out IntPtr img);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError FreeListOfImageRecord(IntPtr recs, UInt32 list_size);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveImage(uuidDLL id, out IntPtr img);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveSampleImageSetRecord(uuidDLL id, out IntPtr rec);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeImageWrapper(out IntPtr image_wrapper, UInt16 num_image_wrapper);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeImageSetWrapper(ref IntPtr image_set_wrapper, UInt16 num_image_set_wrapper);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveSampleImageSetRecords(UInt64 start, UInt64 end, string userId, out IntPtr reclist, out UInt32 listSize);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveSampleImageSetRecordList(IntPtr ids, UInt32 list_size, out IntPtr recs, out UInt32 retrieved_size);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeListOfImageSetRecord(IntPtr list_, UInt32 n_items);

        [DllImport("HawkeyeCore.dll")]
        public static extern void FreeListOfSampleRecord(IntPtr list_, UInt32 n_items);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError DeleteSampleRecord(
            string username, string password,
            IntPtr wqi_uuidlist, 
            UInt32 num_uuid, 
            bool retain_results_and_first_image,
            [MarshalAs(UnmanagedType.FunctionPtr)] delete_sample_record_callback onDeleteCompletion);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveSampleRecord(uuidDLL id, out IntPtr rec);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveSampleRecords(UInt64 start, UInt64 end, string userId, out IntPtr reclist, out UInt32 listSize);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveSampleRecordList(IntPtr ids, UInt32 list_size, out IntPtr recs, out UInt32 retrieved_size);

        #endregion


        #region API_Calls

        public static HawkeyeError RetrieveSampleRecordAPI(uuidDLL id, out SampleRecordDomain sampleRecordDomain)
        {
            IntPtr ptrSampleRecord;
            var hawkeyeError = RetrieveSampleRecord(id, out ptrSampleRecord);
            if (ptrSampleRecord != IntPtr.Zero)
            {
                var recList = ptrSampleRecord.MarshalToSampleRecordDomains(1, Result.RetrieveResultRecordListAPI);
                sampleRecordDomain = recList[0];
                FreeListOfSampleRecord(ptrSampleRecord, 1);
            }
            else
            {
                Log.Warn($"RetrieveSampleRecordAPI: {id}, {hawkeyeError}");
                sampleRecordDomain = new SampleRecordDomain();
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveSampleRecordsAPI(ulong start, ulong end, string userId,
            out List<SampleRecordDomain> recList, out uint listSize)
        {
            IntPtr ptrSampleRecord;
            var hawkeyeError = RetrieveSampleRecords(start, end, userId, out ptrSampleRecord, out listSize);
            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                Log.Warn($"RetrieveSampleRecordsAPI: {start}, {end}, {userId}): {hawkeyeError}");
            }

            recList = ConvertSampleRecords_toManagedList(ptrSampleRecord, listSize);

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveSampleRecordListAPI(uuidDLL[] ids,
            out List<SampleRecordDomain> recList,
            out uint retrievedSize)
        {
            IntPtr ptrSampleRecord;
            var uuidsPtr = ids.MarshalToIntPtr();
            var hawkeyeError = RetrieveSampleRecordList(uuidsPtr.Raw, (uint)ids.Length, out ptrSampleRecord, out retrievedSize);

            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                if (ids.Any())
                {
                    var idsStr = string.Join(", ", ids);
                    Log.Warn($"RetrieveSampleRecordListAPI: {idsStr}, {ids.Length}): {hawkeyeError}");
                }
                else
                {
                    Log.Warn($"RetrieveSampleRecordListAPI: no elements, {hawkeyeError}");
                }
            }

            recList = ConvertSampleRecords_toManagedList(ptrSampleRecord, retrievedSize);

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveSampleImageSetRecordListAPI(uuidDLL[] ids,
            out List<SampleImageRecordDomain> recList)
        {
            var sz = Marshal.SizeOf(typeof(SampleImageSetRecord));
            IntPtr ptrSampleImageSetRecord;
            uint retrievedSize;
            var uuidsPtr = ids.MarshalToIntPtr();
            var hawkeyeError = RetrieveSampleImageSetRecordList(uuidsPtr.Raw, (uint)ids.Length, out ptrSampleImageSetRecord, out retrievedSize);

            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                if (ids.Any())
                {
                    var idsStr = string.Join(", ", ids);
                    Log.Warn($"RetrieveSampleImageSetRecordListAPI: {idsStr}, {ids.Length}, {hawkeyeError}");
                }
                else
                {
                    Log.Warn($"RetrieveSampleImageSetRecordListAPI: no elements, {hawkeyeError}");
                }
            }

            recList = ptrSampleImageSetRecord.MarshalToSampleImageRecordDomains(retrievedSize);

            if (retrievedSize > 0)
            {
                FreeListOfImageSetRecordAPI(ptrSampleImageSetRecord, retrievedSize);
            }
            return hawkeyeError;
        }

        private static void FreeListOfImageSetRecordAPI(IntPtr img_set_rec, uint count)
        {
            FreeListOfImageSetRecord(img_set_rec, count);
        }

        private static void FreeListOfImageRecordAPI(IntPtr img_rec, uint count)
        {
            Misc.LogOnHawkeyeError("FreeListOfImageRecord::", FreeListOfImageRecord(img_rec, count));
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveImageAPI(uuidDLL id, out IntPtr imgPtr)
        {
            var hawkeyeError = RetrieveImage(id, out imgPtr);
            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                Log.Warn($"RetrieveImageAPI: {id}, {hawkeyeError}");
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveAnnotatedImageAPI(uuidDLL result_id, uuidDLL image_id, out IntPtr imgPtr)
        {
            var hawkeyeError = RetrieveAnnotatedImage(result_id, image_id, out imgPtr);
            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                Log.Warn($"RetrieveAnnotatedImageAPI: {result_id}, {image_id}, {hawkeyeError}");
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveBWImageAPI(uuidDLL image_id, out IntPtr imgPtr)
        {
            var hawkeyeError = RetrieveBWImage(image_id, out imgPtr);

            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                Log.Warn($"RetrieveBWImageAPI: {image_id}, {hawkeyeError}");
            }

            return hawkeyeError;
        }

        public static void FreeImageWrapperAPI(IntPtr imgPtr)
        {
            FreeImageWrapper(out imgPtr, 1);
        }

        #endregion


        #region Private Methods

        private static void MarshalImageRecordList_andFreePtr(IntPtr ptrImageRecord, uint listSize, out List<ImageRecordDomain> recList)
        {
            recList = ptrImageRecord.MarshalToImageRecordDomains(listSize);

            if (listSize > 0)
            {
                FreeListOfImageRecordAPI(ptrImageRecord, listSize);
            }
        }

        public static List<SampleRecordDomain> ConvertSampleRecords_toManagedList(IntPtr ptrSampleRecord, uint listSize)
        {
            if (ptrSampleRecord == IntPtr.Zero)
                return new List<SampleRecordDomain>();

            var recList = ptrSampleRecord.MarshalToSampleRecordDomains(listSize, Result.RetrieveResultRecordListAPI);
            if (listSize > 0)
            {
                FreeListOfSampleRecord(ptrSampleRecord, listSize);
            }

            return recList.Where(x => x.SampleCompletionStatus == sample_completion_status.sample_completed).ToList();
        }

        #endregion
    }

}
