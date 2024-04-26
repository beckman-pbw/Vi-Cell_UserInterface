// ***********************************************************************
// <copyright file="CalibrationEnum.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace ScoutUtilities.Enums
{
  
    public enum calibration_type : UInt16
    {
        cal_All = 0,
        cal_Concentration,
        cal_Size,
        cal_ACupConcentration,
        cal_UNKNOWN = 99
    }

    public enum CalibrationGuiState : int
    {
        NotStarted = 0,     // fresh state
        Started,            // the calibration has been started
        Aborted,            // the calibration has been aborted (or is finishing the current running sample)
        Ended,              // ended and waiting for user action to accept/reject
        CalibrationApplied, // the calibration has ended and user applied the results
        CalibrationRejected // the calibration has ended and user rejected the results
    }
}