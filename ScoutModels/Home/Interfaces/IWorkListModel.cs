using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;

namespace ScoutModels.Interfaces
{
    public interface IWorkListModel
    {
        void UpdateStartWorkQueueDateTime(DateTime dt);

        /// <summary>
        /// Returns 'true' if the samplesToProcess will exceed current reagents or waste tube capacity
        /// </summary>
        /// <param name="samplesToProcess"></param>
        /// <param name="substrateType"></param>
        /// <param name="needMoreReagent"></param>
        /// <param name="needMoreWasteTray"></param>
        /// <param name="showPrompt"></param>
        /// <returns></returns>
        bool CheckReagentsAndWasteTray(int samplesToProcess, SubstrateType substrateType,
            out bool needMoreReagent, out bool needMoreWasteTray, bool showPrompt = true);

        string ImageFilePathGenerator(
            Tuple<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> callbackResults);
    }
}