// ***********************************************************************
// <copyright file="ReagentUnloadOption.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;


namespace ScoutUtilities.Enums
{
   
    public enum ReagentUnloadOption : UInt16
    {
        eULNone = 0, // No specific action
        eULDrainToWaste, // Drain container contents to Waste 
        eULPurgeLinesToContainer // Flush reagent lines back into container
    }
}