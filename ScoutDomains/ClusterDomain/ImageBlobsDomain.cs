using System.Collections.Generic;

namespace ScoutDomains.ClusterDomain
{
    public class ImageBlobsDomain
    {
        public int ImageSequenceNumber { get; set; }

        public List<BlobMeasurementDomain> BlobList { get; set; }
    }
}