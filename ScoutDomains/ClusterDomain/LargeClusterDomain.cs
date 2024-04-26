using System.Collections.Generic;

namespace ScoutDomains.ClusterDomain
{
    public class LargeClusterDomain
    {
        public int ImageSequenceNumber { get; set; }

        public List<LargeClusterDataDomain> LargeClusterDataList { get; set; }
    }
}