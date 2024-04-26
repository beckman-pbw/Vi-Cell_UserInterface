using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ScoutUtilities.Helper;

namespace ScoutUtilitiesTest
{
    [TestFixture]
    class ConcentrationSlopeTest
    {
        private readonly int numAssay_1 = 10;
        private readonly int numAssay_2 = 5;
        private readonly int numAssay_3 = 3;

        CalibrationData runCalibration(double[] cellCounts, double[] assayConcentrations)
        {
            var data = initializeCalibrationData(cellCounts, assayConcentrations);

            data.CalculateSlope_averageOverAssays();

            return data;
        }

        CalibrationData initializeCalibrationData(double[] cellCounts, double[] assayConcentrations)
        {
            Debug.Assert(cellCounts.Length == numAssay_1 + numAssay_2 + numAssay_3);

            var data = new CalibrationData();
            var sampleIndex = 0;
            var assayLimit = numAssay_1;
            for (; sampleIndex < assayLimit; sampleIndex++)
            {
                data.Data.Add(new KeyValuePair<double, double>(assayConcentrations[0], cellCounts[sampleIndex]));
            }

            assayLimit += numAssay_2;
            for (; sampleIndex < assayLimit; sampleIndex++)
            {
                data.Data.Add(new KeyValuePair<double, double>(assayConcentrations[1], cellCounts[sampleIndex]));
            }

            assayLimit += numAssay_3;
            for (; sampleIndex < assayLimit; sampleIndex++)
            {
                data.Data.Add(new KeyValuePair<double, double>(assayConcentrations[2], cellCounts[sampleIndex]));
            }

            Debug.Assert(data.Data.Count() == cellCounts.Length);
            return data;
        }

        double simpleImpl_origMethod(CalibrationData data)
        {
            double numerator = 0;
            double denominator = 0;
            foreach (var pair in data.Data)
            {
                numerator += (pair.Key * pair.Value);
                denominator += (pair.Value * pair.Value);
            }

            return numerator / denominator;
        }

        double simpleImpl_JulieMethod(CalibrationData data)
        {
            var sampleIndex = 0;
            var assayLimit = numAssay_1;

            double avgCount_2m = 0;
            double assay_2m = data.Data[sampleIndex].Key;
            for (; sampleIndex < assayLimit; sampleIndex++)
            {
                avgCount_2m += data.Data[sampleIndex].Value;
            }

            avgCount_2m /= numAssay_1;

            assayLimit += numAssay_2;
            double avgCount_4m = 0;
            double assay_4m = data.Data[sampleIndex].Key;
            for (; sampleIndex < assayLimit; sampleIndex++)
            {
                avgCount_4m += data.Data[sampleIndex].Value;
            }
            avgCount_4m /= numAssay_2;

            assayLimit += numAssay_3;
            double avgCount_10m = 0;
            double assay_10m = data.Data[sampleIndex].Key;
            for (; sampleIndex < assayLimit; sampleIndex++)
            {
                avgCount_10m += data.Data[sampleIndex].Value;
            }
            avgCount_10m /= numAssay_3;

            var cf_2m = assay_2m / avgCount_2m;
            var cf_4m = assay_4m / avgCount_4m;
            var cf_10m = assay_10m / avgCount_10m;

            return (cf_2m + cf_4m + cf_10m) / 3.0;
        }

        // These are unused. Julie and I agree that they would give too much credence to
        // the 2M assays, since they have more data points, but those data points are
        // expected to vary *together* rather than independently.
        double simpleImpl_KyleMethod_geometricMean(CalibrationData data)
        {
            var product = data.Data.Aggregate(1.0, (agg, item) => agg * item.Key / item.Value);
            return Math.Pow(product, 1.0 / data.Data.Count);
        }

        double simpleImpl_KyleMethod_logLogRegression(CalibrationData data)
        {
            // This is equivalent to the geometric-mean method; it's more expensive, but it demonstrates
            // that the geometric-mean method minimizes the *percent* errors, which is probably what
            // we want.

            var logData = data.Data.Select(item =>
                new KeyValuePair<double, double>(Math.Log(item.Key), Math.Log(item.Value)));

            var logIntercept = logData.Average(item => item.Key - item.Value);

            return Math.Exp(logIntercept);
        }

        [Test]
        public void PC3527_3219_Test1()
        {
            double[] cellCounts =
            {
                5442, 5734, 5431, 5543, 5687, 5583, 5345, 4984, 5316, 5808, 11710, 11530, 11913, 10911, 11336, 29380,
                28669, 29631
            };

            double[] assayConcentrations = {1.92e06, 3.86e06, 9.63e06 };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(338.54, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(338.54, data.Slope, 0.01);

            Assert.AreEqual(0.99888, data.R2, 0.01);
        }

        [Test]
        public void PC3527_3219_Test2()
        {
            double[] cellCounts =
            {
                4979, 4828, 5150, 4716, 4974, 4979, 4887, 5156, 4809, 4683, 10352, 10058, 10063, 10626, 10180, 26978,
                26502, 27200
            };

            double[] assayConcentrations = {1.92e06, 3.86e06, 9.79e06};

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);

            Assert.AreEqual(376.99, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(376.99, data.Slope, 0.01);

            Assert.AreEqual(0.99642, data.R2, 0.01);
        }

        [Test]
        public void PC3527_3219_Test3()
        {
            double[] cellCounts =
            {
                5209, 4428, 5090, 4311, 5753, 4289, 5509, 5720, 5079, 5087, 11221, 10860, 10926, 12038, 11529, 30012,
                30012, 30012
            };

            double[] assayConcentrations = { 1.92e06, 3.90e06, 9.73e06 };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(349.76, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(349.76, data.Slope, 0.01);

            Assert.AreEqual(0.996752, data.R2, 0.01);
        }

        [Test]
        public void PC3527_3219_Test4()
        {
            double[] cellCounts =
            {
                5052, 4961, 5662, 5264, 5254, 5228, 5544, 5451, 5051, 5227, 10250, 10260, 10446, 10539, 10353, 25945, 25857, 25937
            };

            double[] assayConcentrations = { 1.99e06, 3.86e06, 9.74e06  };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(375.26, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(375.26, data.Slope, 0.01);

            Assert.AreEqual(0.999797, data.R2, 0.01);
        }

        [Test]
        public void PC3527_3219_Test5()
        {
            double[] cellCounts =
            {
                5198, 5475, 5324, 5264, 5629, 5469, 5349, 5433, 5697, 5321, 11137, 11683, 10994, 11290, 11245, 27501, 27600, 27550
            };

            double[] assayConcentrations = { 1.92e06, 3.86e06, 9.63e06  };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(348.85, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(348.85, data.Slope, 0.01);

        }

        [Test]
        public void PC3527_3219_Test6()
        {
            double[] cellCounts =
            {
                4157, 5098, 3298, 6500, 5469, 4500, 3600, 7800, 4500, 1429, 11137, 11683, 10994, 11290, 11245, 27501, 35000, 27550
            };

            double[] assayConcentrations = { 1.90e06, 4.20e06, 9.45e06 };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(365.81, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(365.81, data.Slope, 0.01);

        }

        [Test]
        public void PC3527_3219_Test7()
        {
            double[] cellCounts =
            {
                4927, 4535, 4714, 5261, 4648, 4632, 4799, 4846, 5146, 5036, 10224, 10690, 10228, 10152, 10681, 26909, 25652, 26444
            };

            double[] assayConcentrations = { 1.92e06, 3.86e06, 9.73e06  };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(378.77, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(378.77, data.Slope, 0.01);

        }

        [Test]
        public void PC3527_3219_Test8()
        {
            double[] cellCounts =
            {
                4685, 4887, 4920, 4786, 4620, 4744, 5081, 4679, 4663, 4930, 10190, 10234, 10410, 10190, 10231, 25790, 26187, 25764
            };

            double[] assayConcentrations = { 1.92e06, 3.86e06, 9.63e06 };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(382.74, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(382.74, data.Slope, 0.01);

        }

        [Test]
        public void PC3527_3219_Test9()
        {
            double[] cellCounts =
            {
                5990, 5794, 9843, 5615, 5927, 5948, 5703, 5889, 5873, 5767, 11322, 12438, 11898, 12478, 12324, 27341, 28282, 29684
            };

            double[] assayConcentrations = { 1.98e06, 3.93e06, 1.03e07 };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(334.93, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(334.93, data.Slope, 0.01);

        }

        [Test]
        public void PC3527_3219_Test10()
        {
            double[] cellCounts =
            {
                5855, 5393, 4948, 5319, 4955, 5159, 5217, 5417, 4993, 5182, 10561, 10575, 10899, 10262, 10397, 25859, 26227, 25996
            };

            double[] assayConcentrations = { 1.99e06, 3.86e06, 9.74e06  };

            var data = runCalibration(cellCounts, assayConcentrations);

            Assert.AreEqual(0, data.Intercept);
            Assert.AreEqual(373.33, simpleImpl_JulieMethod(data), 0.01);  // Value from the spreadsheet in ticket 3219 
            Assert.AreEqual(simpleImpl_JulieMethod(data), data.Slope, 0.01);
            Assert.AreEqual(373.33, data.Slope, 0.01);

        }

        [Test]
        public void CalculateCV_Test() 
        {
            var cellCounts =
                new List<double> { 5229, 5108, 5012, 5044, 5333, 5200, 4975, 5105, 5117, 5045 };
            var avgCellCount = cellCounts.Average();
            var data = new CalibrationData();

            var expectedCV = 2.1;
            var actualCV = data.CalculateCV(cellCounts, avgCellCount);

            Assert.AreEqual(expectedCV, actualCV);
        }

        [Test]
        public void CalculateCV_Test2()
        {
            var cellCounts = new List<double> {10282,10075,10285,9863,10093};
            var avgCellCount = cellCounts.Average();
            var data = new CalibrationData();

            var expectedCV = 1.7;
            var actualCV = data.CalculateCV(cellCounts, avgCellCount);

            Assert.AreEqual(expectedCV,actualCV);
        }

        [Test]
        public void CalculateCV_Test3()
        {
            var cellCounts = new List<double> {29784, 29842, 29897};
            var avgCellCount = cellCounts.Average();
            var data = new CalibrationData();

            var expectedCV = 0.2;
            var actualCV = data.CalculateCV(cellCounts, avgCellCount);

            Assert.AreEqual(expectedCV, actualCV);
        }

        [Test]
        public void CalculateCV_Test4()
        {
            var cellCounts = new List<double> { 0 };
            var avgCellCount = 0;
            var data = new CalibrationData();

            var expectedCV = 0.0;
            var actualCV = data.CalculateCV(cellCounts, avgCellCount);

            Assert.AreEqual(expectedCV, actualCV);
        }

        [Test]
        public void CalculateCV_Test5()
        {
            var cellCounts = new List<double> { 1,1,1,1 };
            var avgCellCount = 1;
            var data = new CalibrationData();

            var expectedCV = 0.0;
            var actualCV = data.CalculateCV(cellCounts, avgCellCount);

            Assert.AreEqual(expectedCV, actualCV);
        }
    }
}
