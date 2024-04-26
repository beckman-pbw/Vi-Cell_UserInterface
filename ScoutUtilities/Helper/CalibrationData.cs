using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;

namespace ScoutUtilities.Helper
{

    public class CalibrationData
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CalibrationData()
        {
            Data = new List<KeyValuePair<double, double>>();
        }

        public List<KeyValuePair<double, double>> Data;

        public double Slope;
        
        public double Intercept;
        
        public double R2;

        public void CalculateSlope_averageOverAssays()
        {
            var assaysToCounts = new Dictionary<double, List<double>>();
            foreach (var item in Data)
            {
                var assay = item.Key;
                if (!assaysToCounts.ContainsKey(assay))
                    assaysToCounts[assay] = new List<double>();
                assaysToCounts[assay].Add(item.Value);
            }

            Intercept = 0;
            Slope = 0;
            var subgroupCf = 0.0;
            foreach (var assayVals in assaysToCounts)
            {
                var avgCount = assayVals.Value.Sum() / assayVals.Value.Count;
                if(!avgCount.Equals(0))
                    subgroupCf = assayVals.Key / avgCount;
                Slope += subgroupCf / assaysToCounts.Count;
            }

            CalculateR2();
        }

        private void CalculateR2()
        {
            if (Slope == 0) Log.Warn($"CalculateR2::Slope is ZERO");
            var meanAssays = Data.Any() ? Data.Average(item => item.Key) : 0;
            var varianceResiduals = Data.Sum(item => Math.Pow(item.Key - item.Value * Slope, 2));
            var varianceSamples = Data.Sum(item => Math.Pow(item.Key - meanAssays, 2));
            R2 = varianceSamples > 0 ? 1.0 - Math.Pow(varianceResiduals / varianceSamples, 2) : 0;
        }

        public double CalculateCV(List<double> totalCells, double avgTotCount)
        {
            double sumOfSquares = 0.0;
            double standardDev = 0.0;
            double percentCV = 0.0;
            if (totalCells.Count <= 1 || avgTotCount.Equals(0))
            {
                return percentCV;
            }
            double denom = 1.0 / (totalCells.Count - 1);
            foreach (var cellCount in totalCells)
            {
                sumOfSquares += Math.Pow((cellCount - avgTotCount), 2);
            }
            standardDev = Math.Sqrt((sumOfSquares * denom));
            percentCV = (standardDev / avgTotCount) * 100;
            percentCV = Misc.UpdateDecimalPoint(percentCV, TrailingPoint.One);
            return percentCV;
        }
    }

}
