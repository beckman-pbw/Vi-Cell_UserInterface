using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public static partial class Reagent
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetReagentDefinitions(out UInt32 num_reagents, out IntPtr reagents);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetReagentContainerStatus(UInt16 container_num, out IntPtr status);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetReagentContainerStatusAll(out UInt16 num_reagents, out IntPtr status);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError SampleTubeDiscardTrayEmptied();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError StartPrimeReagentLines([MarshalAs(UnmanagedType.FunctionPtr)] prime_reagentlines_callback on_status_change);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError StartPurgeReagentLines([MarshalAs(UnmanagedType.FunctionPtr)] purge_reagentlines_callback on_status_change);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetPrimeReagentLinesState(out ePrimeReagentLinesState state);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError CancelPrimeReagentLines();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError CancelPurgeReagentLines();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError StartDecontaminateFlowCell([MarshalAs(UnmanagedType.FunctionPtr)] flowcell_decontaminate_status_callback on_status_change);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError StartFlushFlowCell([MarshalAs(UnmanagedType.FunctionPtr)] flowcell_flush_status_callback on_status_change);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetFlushFlowCellState(out eFlushFlowCellState state);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetDecontaminateFlowCellState(out eDecontaminateFlowCellState state);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError CancelFlushFlowCell();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError CancelCleanFluidics();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError CancelDecontaminateFlowCell();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError SetReagentContainerLocation(out ReagentContainerLocation location); //for hunter

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError LoadReagentPack(
            [MarshalAs(UnmanagedType.FunctionPtr)] reagent_load_status_callback onLoadStatusChange,
            [MarshalAs(UnmanagedType.FunctionPtr)] reagent_load_complete_callback onLoadComplete);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError UnloadReagentPack(
            IntPtr UnloadActions, 
            UInt16 nContainers,
            [MarshalAs(UnmanagedType.FunctionPtr)] reagent_unload_status_callback onUnloadStatusChange,
            [MarshalAs(UnmanagedType.FunctionPtr)] reagent_unload_complete_callback onUnloadComplete);

        [DllImport("HawkeyeCore.dll")]
        static extern IntPtr GetReagentPackUnloadStatusAsStr(ReagentUnloadSequence status);

        [DllImport("HawkeyeCore.dll")]
        static extern IntPtr GetReagentPackLoadStatusAsStr(ReagentLoadSequence status);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeReagentDefinitions(out IntPtr reagents, out UInt32 num_reagents);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeListOfReagentContainerState(IntPtr list, UInt32 num_containers);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError StartCleanFluidics([MarshalAs(UnmanagedType.FunctionPtr)] flowcell_flush_status_callback on_status_change);

        [DllImport("HawkeyeCore.dll")]
        static extern HawkeyeError GetReagentVolume (CellHealthFluidType type, out Int32 volume);

        [DllImport("HawkeyeCore.dll")]
        static extern HawkeyeError SetReagentVolume(CellHealthFluidType type, Int32 volume);

        [DllImport("HawkeyeCore.dll")]
        static extern HawkeyeError AddReagentVolume(CellHealthFluidType type, Int32 volume);

	#endregion

	#region API_Calls

		public static UInt16 num_reagents;

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartPrimeReagentLinesAPI(prime_reagentlines_callback on_status_change)
        {
            return StartPrimeReagentLines(on_status_change);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartFlushFlowCellAPI(flowcell_flush_status_callback on_status_change)
        {
            return StartFlushFlowCell(on_status_change);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartCleanFluidicsAPI(flowcell_flush_status_callback on_status_change)
        {
	        return StartCleanFluidics(on_status_change);
        }

		[MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartPurgeReagentLinesAPI(purge_reagentlines_callback on_status_change)
        {
	        return StartPurgeReagentLines(on_status_change);
        }

		[MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartDecontaminateFlowCellAPI(flowcell_decontaminate_status_callback on_status_change)
        {
            return StartDecontaminateFlowCell(on_status_change);
        }

		[MustUseReturnValue("Use HawkeyeError")]
		public static HawkeyeError CancelDecontaminateFlowCellAPI()
		{
			return CancelDecontaminateFlowCell();
		}

		[MustUseReturnValue("Use HawkeyeError")]
		public static HawkeyeError CancelFlushFlowCellAPI()
		{
			return CancelFlushFlowCell();
		}

		[MustUseReturnValue("Use HawkeyeError")]
		public static HawkeyeError CancelCleanSequenceAPI()
		{
			return CancelCleanFluidics();
		}

		[MustUseReturnValue("Use HawkeyeError")]
		public static HawkeyeError CancelPrimeReagentLinesAPI()
		{
			return CancelPrimeReagentLines();
		}

		[MustUseReturnValue("Use HawkeyeError")]
		public static HawkeyeError CancelPurgeReagentLinesAPI()
		{
			return CancelPurgeReagentLines();
		}

		[MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SampleTubeDiscardTrayEmptiedAPI()
        {
            return SampleTubeDiscardTrayEmptied();
        }

        [HandleProcessCorruptedStateExceptions]
        public static Tuple<HawkeyeError, IntPtr> GetReagentContainerStatusAllAPI(
            ref List<ReagentContainerState> reagentContainerStateAll, byte reagentNum)
        {
            var sz = Marshal.SizeOf(typeof(ReagentContainerState));
            IntPtr ptrReagentContainerStateAll;
            var hawkeyeError = GetReagentContainerStatusAll(out num_reagents, out ptrReagentContainerStateAll);

//TODO: where is ptrReagentContainerStateAll freed???
            if (hawkeyeError.Equals(HawkeyeError.eSuccess) && ptrReagentContainerStateAll != IntPtr.Zero)
            {
                var reagentContainerPtr = ptrReagentContainerStateAll;
                for (var i = 0; i < num_reagents; i++)
                {
                    reagentContainerStateAll.Add((ReagentContainerState)Marshal.PtrToStructure(reagentContainerPtr, typeof(ReagentContainerState)));
                    reagentContainerPtr += sz;
                }
            }

            return new Tuple<HawkeyeError, IntPtr>(hawkeyeError, ptrReagentContainerStateAll);
        }

        public static void FreeReagentStateALLAPI(IntPtr reagentPtr)
        {
            FreeListOfReagentContainerState(reagentPtr, num_reagents);
        }

        [HandleProcessCorruptedStateExceptions]
        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetReagentDefinitionsAPI(ref uint container_num,
            ref List<ReagentDefinition> reagentDefinitionListAll)
        {
            var reagentDefinitionState = new ReagentDefinition();
            var size = Marshal.SizeOf(reagentDefinitionState);

            IntPtr reagentDefinitions;

            var hawkeyeError = GetReagentDefinitions(out container_num, out reagentDefinitions);
            var reagentDefPtr = reagentDefinitions;
            if (hawkeyeError.Equals(HawkeyeError.eSuccess) && reagentDefPtr != IntPtr.Zero)
            {
                for (var i = 0; i < container_num; i++)
                {
                    reagentDefinitionListAll.Add((ReagentDefinition)Marshal.PtrToStructure(reagentDefPtr, typeof(ReagentDefinition)));
                    reagentDefPtr += size;
                }

                FreeReagentDefinitions(out reagentDefinitions, out container_num);
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError LoadReagentPackAPI(reagent_load_status_callback onLoadStatusChange,
            reagent_load_complete_callback onLoadComplete)
        {
            return LoadReagentPack(onLoadStatusChange, onLoadComplete);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError UnloadReagentPackAPI(IntPtr unloadOptionPtr, UInt16 nContainers
            , reagent_unload_status_callback onUnloadStatusChange, reagent_unload_complete_callback onUnloadComplete)
        {
            return UnloadReagentPack(unloadOptionPtr, nContainers, onUnloadStatusChange, onUnloadComplete);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static bool GetReagentVolumeAPI (CellHealthFluidType type, ref Int32 volume)
        {
	        var hawkeyeError = GetReagentVolume(type, out volume);
	        return hawkeyeError == HawkeyeError.eSuccess;
        }

		[MustUseReturnValue("Use HawkeyeError")]
        public static bool SetReagentVolumeAPI(CellHealthFluidType type, Int32 volume)
        {
	        var hawkeyeError = SetReagentVolume(type, volume);
	        return hawkeyeError == HawkeyeError.eSuccess;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static bool AddReagentVolumeAPI(CellHealthFluidType type, Int32 volume)
        {
	        var hawkeyeError = AddReagentVolume(type, volume);
	        return hawkeyeError == HawkeyeError.eSuccess;
        }

		#endregion


		#region Private Methods


		#endregion

	}
}
