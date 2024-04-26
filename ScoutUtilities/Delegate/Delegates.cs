// ***********************************************************************
// <copyright file="Delegates.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Delegate
{
   
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void reagent_unload_status_callback(ReagentUnloadSequence unloadsequence);

  
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void reagent_unload_complete_callback(ReagentUnloadSequence unloadsequence);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void reagent_load_status_callback(ReagentLoadSequence loadSequence);
    

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void reagent_load_complete_callback(ReagentLoadSequence loadSequence);

   
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void autofocus_state_callback_t(eAutofocusState status, IntPtr results);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void countdown_timer_callback_t(UInt32 timeRemaining_seconds);

   
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void brightfield_dustsubtraction_callback(eBrightfieldDustSubtractionState On_DustSubtraction,
        IntPtr dust_ref, ushort num_dust_images, IntPtr source_dust_images);

   
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void service_analysis_result_callback(HawkeyeError On_hawkeyeError,
        BasicResultAnswers On_basicresultanswer, IntPtr On_imagewrapper);

  
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void service_live_image_callback(HawkeyeError ptr, IntPtr image_wrapper);

   
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void plate_motor_calibration_state_callback(CalibrationState cali_state);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void carousel_motor_calibration_state_callback(CalibrationState cali_state);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void cancel_motor_calibration_state_callback(CalibrationState cali_state);

  
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void prime_reagentlines_callback(ePrimeReagentLinesState status);


	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void purge_reagentlines_callback(ePurgeReagentLinesState status);


	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void flowcell_decontaminate_status_callback(eDecontaminateFlowCellState status);

   
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void flowcell_flush_status_callback(eFlushFlowCellState status);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void export_data_progress_callback(HawkeyeError hawkeyeStatus, uuidDLL uuid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void export_data_progress_callback_pcnt(HawkeyeError hawkeyeStatus, uuidDLL uuid, int percent);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void export_data_completion_callback(HawkeyeError hawkeyeStatus, string completion);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void export_data_status_callback(string filename, string bulkDataId, uint ExportStatus, uint percent);

    #region WorkQueueDelegate   

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void sample_status_callback(IntPtr ptr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void sample_image_result_callback(IntPtr ptr, UInt16 imageSeqNum, IntPtr image,
        BasicResultAnswers cumulativeResults, BasicResultAnswers imageResults);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void worklist_completion_callback(uuidDLL uuid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void sample_analysis_callback(HawkeyeError status, uuidDLL sampleId, IntPtr resultRecord);

  
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void start_WorkQueue();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void start_Processing();


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void delete_sample_record_callback(HawkeyeError deletionStatus, uuidDLL uuid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void delete_sample_record_callback_pcnt(HawkeyeError deletionStatus, uuidDLL uuid, int percent);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void delete_work_queue_callback(HawkeyeError deletionStatus, uuidDLL uuid);

    public delegate void StopLoadingIndicator(bool isEnable);

    public delegate void UpdateSelectedSampleCell(object cellType);

    public delegate void UpdateGraph();
    
    #endregion
}