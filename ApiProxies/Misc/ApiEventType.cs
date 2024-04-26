using System;

namespace ApiProxies.Misc
{
    public enum ApiEventType
    {
        Service_Live_Image,
        Service_Analysis_Result,
        Brightfield_Dust_Subtraction,
        Autofocus_Countdown_Timer,
        Autofocus_State,
        Reagent_Load_Complete,
        Reagent_Unload_Complete,
        Reagent_Load_Status,
        Reagent_Unload_Status,
        Prime_Reagent_Lines,
        Carousel_Motor_Calibration_State,
        Plate_Motor_Calibration_State,
        Motor_Calibration_Cancelled,
        Start_WorkQueue,
        Delete_Sample_Results,
        WorkQueue_Completed,
        WorkQueue_Item_Status,
        WorkQueue_Item_Completed,
        WorkQueue_Image_Result,
        Flowcell_Flush_Status,
        Flowcell_Decontaminate_Status,
        Sample_Analysis,
        ExportProgress,
        ExportCompletion,
        Add_Sample_Set,
        Cancel_Sample_Set,
        Start_Processing,
        Purge_Reagent_Lines,
    }
}