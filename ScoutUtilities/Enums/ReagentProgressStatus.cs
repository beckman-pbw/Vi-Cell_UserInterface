// ***********************************************************************
// <copyright file="ReagentProgressStatus.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace ScoutUtilities.Enums
{
   
    public enum CallBackProgressStatus
    {
        IsStart,
        
        IsRunning,
        
        IsFinish,
        
        IsError,
        
        IsCanceling,

        IsCanceled
    }

    public enum ReagentReplaceOption
    {
        IsNone,
        
        IsDrain,
        
        IsPurge
    }
}