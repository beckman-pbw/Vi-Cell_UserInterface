using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;

namespace ScoutModels.ExpandedSampleWorkflow
{
    public class WorkListModel : IWorkListModel
    {
        private readonly ICapacityManager _capacityManager;

        public WorkListModel(ICapacityManager capacityManager)
        {
            _capacityManager = capacityManager;
        }

        public DateTime StartWorkQueueDateTime;

        public WorkListModel()
        {
            StartWorkQueueDateTime = DateTime.Now;
        }

        public void UpdateStartWorkQueueDateTime(DateTime dt)
        {
            StartWorkQueueDateTime = dt;
        }
        public bool CheckReagentsAndWasteTray(int samplesToProcess, SubstrateType substrateType,
            out bool needMoreReagent, out bool needMoreWasteTray, bool showPrompt = true)
        {
            needMoreReagent = _capacityManager.InsufficientReagentPackUsesLeft((ushort) samplesToProcess, showPrompt);

            needMoreWasteTray = false;
            if (substrateType == SubstrateType.Carousel)
            {
                needMoreWasteTray = _capacityManager.InsufficientDisposalTrayCapacity((ushort) samplesToProcess, showPrompt);
            }

            return needMoreReagent || needMoreWasteTray;
        }

        public string ImageFilePathGenerator(Tuple<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> callbackResults)
        {
            var workItemName = $"{ScoutUtilities.Misc.ConvertToFileNameFormat(StartWorkQueueDateTime)}_{callbackResults.Item1.SampleName}";
            return FileSystem.GetImageSavePath(workItemName, true);
        }
    }
}