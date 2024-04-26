// ***********************************************************************
// <copyright file="ReagentLoadSequence.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace ScoutUtilities.Enums
{
   
    public enum ReagentLoadSequence : ushort
    {
        eLIdle = 0,
        eLWaitingForDoorLatch,
        eLWaitingForReagentSensor,
        eLIdentifyingReagentContainers,
        eLWaitingOnContainerLocation, // If one or more single-fluid containers are discovered...
        eLInsertingProbes,
        eLSynchronizingReagentData,
        eLPrimingFluidLines,

        // Successful termination state
        eLComplete,

        // Failure termination state
        eLFailure_DoorLatchTimeout,
        eLFailure_ReagentSensorDetect,
        eLFailure_NoReagentsDetected,
        eLFailure_NoWasteDetected,
        eLFailure_ReagentInvalid, // Failed validation
        eLFailure_ReagentEmpty, // No remaining quantity
        eLFailure_ReagentExpired, // Expiration date passed
        eLFailure_InvalidContainerLocations, // multi-fluid must be in 1, 
        eLFailure_ProbeInsert,
        eLFailure_Fluidics,
        eLFailure_StateMachineTimeout
    }

   
    public enum ePrimeReagentLinesState : ushort
    {
        prl_Idle = 0,
        prl_PrimingCleaner1,
        prl_PrimingCleaner2,
        prl_PrimingCleaner3,
        prl_PrimingReagent1,
        prl_PrimingDiluent, // May be skipped if not present
        prl_Completed,
        prl_Failed
    }

    public enum ePurgeReagentLinesState : ushort
    {
	    dprl_Idle = 0,
	    dprl_PurgeCleaner1,
	    dprl_PurgeCleaner2,
	    dprl_PurgeCleaner3,
	    dprl_PurgeReagent1,
	    dprl_PurgeReagent2, // May be skipped if not present
	    dprl_Completed,
	    dprl_Failed
    }
}
