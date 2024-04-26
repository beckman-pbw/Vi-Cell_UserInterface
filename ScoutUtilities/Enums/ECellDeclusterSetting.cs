// ***********************************************************************
// <copyright file="ECellDeclusterSetting.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************


using System;
using System.ComponentModel;

namespace ScoutUtilities.Enums
{
  
    public enum eCellDeclusterSetting : UInt16
    {
        [Description("LID_RadioButton_NoneReagent")] eDCNone = 0, // Declustering disabled.
        [Description("LID_Label_Decluster_Low")] eDCLow, // Wider spacing between potential cell centers
        // higher edge threshold
        // higher accumulator (less aggressive, more missed cells)
        [Description("LID_Label_Medium")] eDCMedium, // Middle of the road
        [Description("LID_Label_High")] eDCHigh // Narrower spacing between potential cell centers
        // lower edge threshold
        // lower accumulator (more aggressive, more false positives)
    }

  
    public enum eFlushFlowCellState : UInt16
    {
        ffc_Idle = 0,
        ffc_FlushingCleaner,
        ffc_FlushingConditioningSolution,
        ffc_FlushingBuffer,
        ffc_FlushingAir,
        ffc_Completed,
        ffc_Failed
    }

 
    public enum eDecontaminateFlowCellState : UInt16
    {
        dfc_Idle = 0,
        dfc_AspiratingBleach,
        dfc_Dispensing1,
        dfc_DecontaminateDelay,
        dfc_Dispensing2,
        dfc_FlushingBuffer,
        dfc_FlushingAir,
        dfc_Completed,
        dfc_Failed,
		dfc_FindingTube,
		dfc_TfComplete,
		dfc_TfCancelled,
    }
}
