using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels.Interfaces;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace ScoutModels.Service
{
    /// <summary>
    /// The capacity manager manages reagents and disposal levels
    /// </summary>
    public class CapacityManager : ICapacityManager
    {
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly IDisplayService _displayService;

        public CapacityManager(IInstrumentStatusService instrumentStatusService, IDisplayService displayService)
        {
            _instrumentStatusService = instrumentStatusService;
            _displayService = displayService;
        }

        public bool InsufficientReagentPackUsesLeft(ushort queueLength, bool showPrompt = true)
        {
            var needMoreReagent =
                queueLength > _instrumentStatusService.SystemStatusDom.RemainingReagentPackUses;
            if (needMoreReagent)
            {
                _displayService.DisplayMessage(LanguageResourceHelper.Get("LID_MSG_NotEnoughReagent"), showPrompt);
            }

            return needMoreReagent;
        }

        public bool InsufficientDisposalTrayCapacity(ushort queueLength, bool showPrompt = true)
        {
            var needMoreTrayCapacity =
                queueLength > _instrumentStatusService.SystemStatusDom.SampleTubeDisposalRemainingCapacity;
            if (needMoreTrayCapacity)
            {
                _displayService.DisplayMessage(LanguageResourceHelper.Get("LID_MSG_NotEnoughDisposalTraySpace"), showPrompt);
            }

            return needMoreTrayCapacity;
        }
    }
}
