using System.Collections.Generic;
using ScoutUtilities.Enums;
using CoordinatePair = System.Drawing.Point;

namespace ScoutDomains.ClusterDomain
{
    public class BlobMeasurementDomain
    {
        public CoordinatePair Coordinates { get; set; }
        public Dictionary<BlobCharacteristicKeys, double> Measurements { get; set; }
    }
}
