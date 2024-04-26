// ***********************************************************************
// <copyright file="HawkeyeError.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace ScoutUtilities.Enums
{
  
    public enum HawkeyeError
    {
        eSuccess = 0,
        eInvalidArgs,
        eNotPermittedByUser,
        eNotPermittedAtThisTime,
        eAlreadyExists,
        eIdle,
        eBusy,
        eTimedout,
        eHardwareFault,
        eSoftwareFault,
        eValidationFailed,
        eEntryNotFound,
        eNotSupported,
        eNoneFound,
        eEntryInvalid,
        eStorageFault,
        eStageNotRegistered,
        ePlateNotFound,
        eCarouselNotFound,
        eLowDiskSpace,
        eReagentError,
        eSpentTubeTray,
        eDatabaseError,
        eDeprecated,
    }
}
