// ***********************************************************************
// <copyright file="SampleStatusImage.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.ComponentModel;

namespace ScoutUtilities.Enums
{
   
    public enum SampleStatusImage
    {
        [Description("/Images/RunningSample.png")]
        PlayImage,

        [Description("/Images/PauseSample.png")]
        PauseImage,

        [Description("/Images/SkipSample.png")]
        SkipImage,
        
        [Description("/Images/ErrorSample.png")]
        ErrorImage
    }
}