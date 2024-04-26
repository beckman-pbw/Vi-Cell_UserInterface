using ScoutUtilities.Common;
using System;

namespace ScoutDomains
{
    public class ReagentStateDomain : BaseNotifyPropertyChanged
    {
        #region struct ReagentDefinition and struct ReagentState in the API

        public string ReagentName { get; set; }

        public int ReagentIndex { get; set; }

        public int MixingCycles { get; set; }

        public string LotInformation { get; set; }

        public int DaysToExpiration { get; set; }

        public int EventsPossible { get; set; }

        public int EventsRemaining { get; set; }

        public int ValveLocation { get; set; }

        public string PartNumber { get; set; }

        #endregion

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddDays(unixTime);
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public DateTime ExpiryDate => FromUnixTime(DaysToExpiration);
    }
}