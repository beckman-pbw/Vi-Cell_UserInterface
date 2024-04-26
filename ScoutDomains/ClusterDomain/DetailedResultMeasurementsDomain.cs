using System.Collections.Generic;
using ScoutUtilities.Structs;

namespace ScoutDomains.ClusterDomain
{
    public class DetailedResultMeasurementsDomain
    {
        public uuidDLL uuid { get; set; }

        public List<ImageBlobsDomain> BlobsByImage { get; set; }

        public List<LargeClusterDomain> LargeClustersByImage { get; set; }
    }
}