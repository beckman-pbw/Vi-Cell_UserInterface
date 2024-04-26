// ***********************************************************************
// <copyright file="RunSampleProgressIndicator.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************


using System.ComponentModel;

namespace ScoutUtilities.Enums
{
   
    public enum RunSampleProgressIndicator
    {
        [Description("LID_Status_Processing")] eNotProcessed = 0,

        [Description("LID_Status_Aspirating")] eInProcess_Aspirating, // several states of "in process"
        
        [Description("LID_Status_Mixing")] eInProcess_Mixing,
        
        [Description("LID_Status_Image_Acquiring")] eInProcess_ImageAcquisition,

        [Description("LID_Status_Cleaning")] eInProcess_Cleaning,
        
        [Description("LID_Status_AcquisitionComplete")] eAcquisition_Complete,
        
        [Description("LID_Status_Completed")] eCompleted,

        eSkip_Error
    }
}