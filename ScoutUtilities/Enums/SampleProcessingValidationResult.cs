using System;

namespace ScoutUtilities.Enums
{
    [Flags]
    public enum SampleProcessingValidationResult : ulong
    {
        Valid                                 = 0x00000000, // cannot use HasFlag(), must use extension below
        WorkListStatusNotIdle                 = 0x00000001,
        NoSamplesDefined                      = 0x00000002,
        InvalidStageAndCarrier                = 0x00000004,
        CarrierNotDefined                     = 0x00000008,
        CarouselInstalledAndSetIs96WellPlate  = 0x00000010,
        InvalidAutomationCupSample            = 0x00000020,
        Invalid96WellPlateSample              = 0x00000040,
        InvalidSamplePosition                 = 0x00000080,
        InvalidDilution                       = 0x00000100,
        InvalidSaveNthImage                   = 0x00000200,
        InvalidAutomationCupNumberOfSamples   = 0x00000400,
        InvalidCellTypeOrQcName               = 0x00000800,
        MultipleSamplesSharePosition          = 0x00001000,
        ACupNotInstalled                      = 0x00002000,
        InvalidSampleOrSampleSetName          = 0x00004000,
        QcExpired                             = 0x00008000,
        InvalidPermissions                    = 0x00010000,
        InsufficientReagents                  = 0x00020000,
        InsufficientWasteTubeCapacity         = 0x00040000,
    }

    public static class SampleProcessingValidationResultExtensions
    {
        /// <summary>
        /// When using Enum.HasFlag with bitwise flag 0, the 0 value enum will ALWAYS return true.
        /// We need to make an easy-to-use method that can check that "looks" similar to HasFlag()
        /// for checking if the value is SampleProcessingValidationResult.Valid.
        /// Enum.HasFlag(SampleProcessingValidationResult.Valid) always returns TRUE.
        /// </summary>
        /// <param name="validationResult"></param>
        /// <returns></returns>
        public static bool HasValidFlag(this SampleProcessingValidationResult validationResult)
        {
            return validationResult == SampleProcessingValidationResult.Valid;
        }

        /// <summary>
        /// When using Enum.HasFlag with bitwise flag 0, the 0 value enum will ALWAYS return true.
        /// We need to make an easy-to-use method that can check if the value is
        /// SampleProcessingValidationResult.Valid.
        /// Enum.HasFlag(SampleProcessingValidationResult.Valid) always returns TRUE.
        /// </summary>
        /// <param name="validationResult"></param>
        /// <returns></returns>
        public static bool IsValid(this SampleProcessingValidationResult validationResult)
        {
            return validationResult == SampleProcessingValidationResult.Valid;
        }
    }
}
