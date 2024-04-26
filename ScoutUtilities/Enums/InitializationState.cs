// ***********************************************************************
// <copyright file="InitializationState.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace ScoutUtilities.Enums
{
  
    public enum InitializationState : UInt16
    {
        eInitializationInProgress = 0,
        eInitializationFailed,
        eFirmwareUpdateInProgress,
        eInitializationComplete,
        eFirmwareUpdateFailed,
        eInitializationStopped_CarosuelTubeDetected
    }
}