using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;

namespace ScoutDomains
{
    public class ReagentContainerStateDomain : BaseNotifyPropertyChanged
    {
        #region struct ReagentContainerState in the API

        public string LotInformation { get; set; }

        public string PartNumber { get; set; }

        public ReagentContainerStatus Status { get; set; }

        public string ContainerName { get; set; }

        public uint? EventsRemaining { get; set; }

        public int? NumberOfReagents { get; set; }

        #endregion

        public IDictionary<string, IList<ReagentStateDomain>> ReagentNames { get; set; }

        public int? EventsPossible { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}