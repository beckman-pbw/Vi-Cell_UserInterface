using System;
using System.ComponentModel;

namespace ScoutUtilities.Enums
{
   
    public enum SamplePostWash : UInt16
    {
        //[Description("LID_Label_NormalWorkflowType")] NormalWash = 0,
        //[Description("LID_Label_LCDWorkflowType")] FastWash
        [Description("LID_Label_FluidicType_1")] NormalWash = 0,
        [Description("LID_Label_FluidicType_2")] FastWash
    }

    public enum FluidicCycleVolume : UInt16
    {
        [Description("LID_Label_FluidicType_1")] FluidicCycleVolume_1 = 0,
        [Description("LID_Label_FluidicType_2")] FluidicCycleVolume_2,
        [Description("LID_Label_FluidicType_3")] FluidicCycleVolume_3,
    }

    public enum SubstrateType : UInt16
    {
        NoType = 0,
        Carousel,
        Plate96,
        AutomationCup
    }

   
    public enum Precession : UInt16
    {
        RowMajor = 0,
        ColumnMajor
    }

   
    public enum SampleStatus : UInt16
    {
        [Description("LID_Status_NotProcessed")] NotProcessed = 0,
        [Description("LID_Status_Aspirating")] InProcessAspirating, // several states of "in process"
        [Description("LID_Status_Mixing")] InProcessMixing,
        [Description("LID_Status_ProcessingImages")] InProcessImageAcquisition,
        [Description("LID_Status_Cleaning")] InProcessCleaning,
        [Description("LID_Status_AcquisitionComplete")] AcquisitionComplete,
        [Description("LID_Status_Completed")] Completed,
        [Description("LID_Status_Skipped")] SkipManual,
        [Description("LID_Label_Error")] SkipError
    }

  
    public enum SystemStatus : UInt16
    {
        Idle = 0,
        ProcessingSample,
        Pausing,
        Paused,
        Stopping,
        Stopped,
        Faulted,
        SearchingTube, // Applicable only for Carousel
        NightlyClean
    }
}