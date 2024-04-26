// ***********************************************************************
// <copyright file="ReagentUnloadSequence .cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace ScoutUtilities.Enums
{
   
    public enum ReagentUnloadSequence : UInt16
    {
        eULIdle = 0,
        eULDraining1,
        eULPurging1,
        eULDraining2,
        eULPurging2,
        eULDraining3,
        eULDraining4,
        eULDraining5,
        eULPurging3,
        eULPurgingReagent1,
        eULPurgingReagent2,
        eULRetractingProbes,
        eULUnlatchingDoor, // If probes are retracted, jump ahead to this state.

        // Successful termination state
        eULComplete,

        // Failure termination state
        eULFailed_DrainPurge, // Unable to continue.
        eULFailed_ProbeRetract,
        eULFailed_DoorUnlatch,
        eULFailure_StateMachineTimeout
    }


   
    public enum eAutofocusState : UInt32
    {
        af_WaitingForSample = 0, // Moving to the supplied sample location
        af_PreparingSample,      // Adding reagents, mixing, measuring
        af_SampleToFlowCell,     // Dispensing sample into flow cell
        af_SampleSettlingDelay,  // Delay to allow sample to settle / stabilize in flow cell
        af_AcquiringFocusData,   // Moving motor through focal range, acquiring and analyzing image
        af_WaitingForFocusAcceptance, // Focus results have been provided to the host; waiting for instructions
        //  Host can : Accept (will record new focus, move to af_FlushingSample
        //  Cancel(will retain previous focus, move to af_FlushingSample
        //  Retry on current cells (will revert to af_AcquiringFocusData
        //  Retry on new cells (will revert to af_SampleToFlowCell)
        af_FlushingSample,       // Cleaning up (after success or failure)
        af_Cancelling,           // Operation is being cancelled either by user or unable to perform
        af_Idle,                 // Completed successfully or cancelled
        af_Failed
    }


    public enum eAutofocusCompletion : UInt16
    {
        afc_Accept = 0,
        afc_Cancel,
        afc_RetryOnCurrentCells
    }

  
    public enum CalibrationState
    {
        eIdle = 0,
        eApplyDefaults,
        eHomingRadiusTheta,
        eWaitingForRadiusPos,
        eWaitingForThetaPos,
        eWaitingForRadiusThetaPos,
        eCalibrateRadius,
        eCalibrateTheta,
        eCalibrateRadiusTheta,
        eWaitingForFinish,
        eInitialize,
        eCompleted,
        eFault
    }

   
    public enum eBrightfieldDustSubtractionState : UInt16
    {
        bds_Idle = 0,
        bds_AspiratingBuffer,
        bds_DispensingBufferToFlowCell,
        bds_AcquiringImages,
        bds_ProcessingImages,
        bds_WaitingOnUserApproval,
        bds_SettingReferenceImage,
        bds_Cancelling,
        bds_Completed,
        bds_Failed
    }
}