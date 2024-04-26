using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public static partial class WorkQueue
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError DeleteWorkQueueRecord(
            IntPtr wq_uuidlist, 
            UInt32 num_uuid,
            bool retain_results_and_first_image, 
            [MarshalAs(UnmanagedType.FunctionPtr)] delete_work_queue_callback onDeleteCompletion);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetWorkQueueStatus(out uint queueLength, out IntPtr wq, out ScoutUtilities.Enums.SystemStatus wqStatus);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetSystemStatus(out ScoutUtilities.Enums.SystemStatus wqStatus);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError StartWorkQueue(
            [MarshalAs(UnmanagedType.FunctionPtr)] sample_status_callback onWQStart,
            [MarshalAs(UnmanagedType.FunctionPtr)] sample_status_callback onWQStatus,
            [MarshalAs(UnmanagedType.FunctionPtr)] worklist_completion_callback onWQComplete,
            [MarshalAs(UnmanagedType.FunctionPtr)] sample_image_result_callback onWQIImageProcessed);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SkipCurrentWorkQueueItem();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError PauseQueue();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ResumeQueue();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError StopQueue();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetWorkQueue(ScoutUtilities.Structs.WorkQueue wq);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError AddItemToWorkQueue(out WorkQueueItem wqi);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetCurrentWorkQueueItem(out IntPtr wq);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeWorkQueueItem(IntPtr wq, UInt32 queue_length);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveWorkQueue(uuidDLL id, out IntPtr rec);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveWorkQueues(UInt64 start, UInt64 end, string userId, out IntPtr reclist, out uint listSize);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveWorkQueueList(IntPtr ids, UInt32 list_size, out IntPtr recs, out UInt32 retrieved_size);

        [DllImport("HawkeyeCore.dll")]
        public static extern void FreeListOfWorkQueueRecord(IntPtr list_, UInt32 n_items);

        #endregion


        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartQueueAPI(
            sample_status_callback onWQIStatus,
            sample_status_callback onWQIComplete,
            worklist_completion_callback onWQComplete,
            sample_image_result_callback onWQIImageProcessed)
        {
            return StartWorkQueue(onWQIStatus, onWQIComplete, onWQComplete, onWQIImageProcessed);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SkipCurrentWorkQueueItemAPI()
        {
            return SkipCurrentWorkQueueItem();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError PauseQueueAPI()
        {
            return PauseQueue();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ResumeQueueAPI()
        {
            return ResumeQueue();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StopQueueAPI()
        {
            return StopQueue();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetWorkQueueAPI(ScoutUtilities.Structs.WorkQueue wq, WorkQueueItem[] wqItems)
        {
            var wqiSize = Marshal.SizeOf(typeof(WorkQueueItem));
            var pWQ = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ScoutUtilities.Structs.WorkQueue)));
            wq.workQueueItems = Marshal.AllocCoTaskMem(wq.numWQI * Marshal.SizeOf(typeof(WorkQueueItem)));
            for (int i = 0; i < wq.numWQI; i++)
            {
                var pTmp = wq.workQueueItems + (i * wqiSize);
                Marshal.StructureToPtr(wqItems[i], pTmp, false);
            }

            Marshal.StructureToPtr(wq, pWQ, false);
            var hawkeyeError = SetWorkQueue(wq);

            Marshal.FreeCoTaskMem(pWQ);
            Marshal.FreeCoTaskMem(wq.workQueueItems);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetCurrentWorkQueueItemAPI(out WorkQueueItemDto wq)
        {
            IntPtr WqDataPtr;
            var HawkeyeError = GetCurrentWorkQueueItem(out WqDataPtr);
            if (WqDataPtr != IntPtr.Zero)
            {
                wq = WqDataPtr.MarshalToWorkQueueItemDto();
                FreeWorkQueueItemAPI(WqDataPtr, 1);
            }
            else
                wq = null;

            return HawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AddItemToWorkQueueAPI(ref WorkQueueItem wqi)
        {
            return AddItemToWorkQueue(out wqi);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError DeleteWorkQueueRecordAPI(
            IntPtr uuidListPtr, 
            UInt32 sizeList, bool firstImg,
            delete_work_queue_callback onDeleteCompletion)
        {
            return DeleteWorkQueueRecord(uuidListPtr, sizeList, firstImg, onDeleteCompletion);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveWorkQueueAPI(uuidDLL id, out WorkQueueRecordDomain rec)
        {
            IntPtr ptrWorkQueue;
            var hawkeyeError = RetrieveWorkQueue(id, out ptrWorkQueue);
            if (ptrWorkQueue != IntPtr.Zero)
            {
                rec = ptrWorkQueue.MarshalToWorkQueueRecordDomain();
                FreeListOfWorkQueueRecordAPI(ptrWorkQueue, (uint)1);
            }
            else
                rec = null;


            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveWorkQueuesAPI(UInt64 start, UInt64 end, string userId,
            out List<WorkQueueRecordDomain> recList, out uint listSize)
        {
            recList = new List<WorkQueueRecordDomain>();
            var sz = Marshal.SizeOf(typeof(WorkQueueRecord));
            IntPtr ptrWorkQueue;
            var hawkeyeError = RetrieveWorkQueues(start, end, userId, out ptrWorkQueue, out listSize);
            var workQueuePtr = ptrWorkQueue;
            for (int i = 0; i < listSize; i++)
            {
                recList.Add(workQueuePtr.MarshalToWorkQueueRecordDomain());
                workQueuePtr += sz;
            }

            FreeListOfWorkQueueRecordAPI(ptrWorkQueue, listSize);

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveWorkQueueListAPI(IntPtr ids, uint list_size,
            out List<WorkQueueRecordDomain> recList,
            out uint retrieved_size)
        {
            IntPtr ptrWorkQueue;
            var hawkeyeError = RetrieveWorkQueueList(ids, list_size, out ptrWorkQueue, out retrieved_size);

            recList = ptrWorkQueue.MarshalToWorkQueueRecordDomains(retrieved_size);

            if (retrieved_size > 0)
            {
                FreeListOfWorkQueueRecordAPI(ptrWorkQueue, retrieved_size);
            }

            return hawkeyeError;
        }
        #endregion


        #region Private Methods

        private static void FreeListOfWorkQueueRecordAPI(IntPtr str, uint size)
        {
            FreeListOfWorkQueueRecord(str, size);
        }

        private static void FreeWorkQueueItemAPI(IntPtr wq, uint queue_length)
        {
            FreeWorkQueueItem(wq, queue_length);
        }

        #endregion

    }
}
