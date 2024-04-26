using System.Collections.Generic;

namespace ScoutServices.Watchdog
{
    public class WatchDogConfigModel
    {
        public string PollingIntervalMS { get; set; }
        public List<string> Servers { get; set; }
    }
}
