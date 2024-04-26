using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Common;

namespace ScoutUiTest
{
    [TestFixture]
    public class HistogramStatisticsTest
    {
        // Each `BarGraphDomain` is populated by `(float, uint)` pairs in
        // `GraphHelper::GetHistogramGraphList`, so we use those types for our
        // test source data.
        private List<List<KeyValuePair<float, uint>>> _graphData;

        private String _testDataDir;

        /* XXX NUnit "eats" any exceptions thrown in a test-fixture constructor.
         * To provide error info for debugging exceptions in the test fixture itself, 
         * try this:
         * https://github.com/nunit/nunit/pull/597/files
        HistogramStatisticsTest()
        {
            try
            {
                SetupTestData(....);
            }
            catch (Exception e)
            {
                // Expose exception to NUnit (see link above)
            }
        }
        */

        private void SetupTestData(String dataFileName)
        {
            // Ignore exceptions; they indicate bad test data and thus an invalid test.
            _testDataDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            StreamReader sr = new StreamReader(Path.Combine(_testDataDir, dataFileName));
            string line = sr.ReadLine();
            var numDataSets = line.Split(',').Length / 2;
            _graphData = new List<List<KeyValuePair<float, uint>>>();
            for (var i = 0; i < numDataSets; i++)
            {
                _graphData.Add(new List<KeyValuePair<float, uint>>());
            }

            while ((line = sr.ReadLine()) != null)
            {
                float nextValue = 0;
                int nextDataIndex = 0;
                foreach (var token in line.Split(',').Select((value, index) => new { index, value }))
                {
                    if (token.index % 2 == 0)
                    {
                        try
                        {
                            nextValue = (float) Convert.ToDouble(token.value);
                            nextDataIndex = token.index / 2;
                        }
                        catch
                        {
                            // Hopefully, this just means that there is no more data in the data set
                            nextValue = 0;
                        }
                    }
                    else
                    {
                        try
                        {
                            _graphData[nextDataIndex]
                                .Add(new KeyValuePair<float, uint>(nextValue, Convert.ToUInt32(token.value)));
                        }
                        catch
                        {
                            Assert.AreEqual(0, nextValue);
                        }
                    }
                }
            }
        }

        private struct Stats
        {
            public double Mean;
            public double Mode;
            public double StandardDeviation;
        }

        private List<Stats> readExpectedResults(String resultsFileName)
        {
            StreamReader sr = new StreamReader(Path.Combine(_testDataDir, resultsFileName));
            String line = sr.ReadLine();
            var numDataSets = line.Split(',').Length / 3;
            var expected = new List<Stats>();
            line = sr.ReadLine();
            var values = line.Split(',').Select(field => Convert.ToDouble(field));
            for (var i = 0; i < numDataSets; i++)
            {
                var newStats = values.Take(3).ToArray();
                expected.Add(new Stats
                {
                    Mean = newStats[0],
                    Mode = newStats[1],
                    StandardDeviation = newStats[2],
                });
                values = values.Skip(3);
            }

            return expected;
        }

        [Test]
        public void TestHistogramStatistics()
        {
            SetupTestData("RandHistData.csv");
            var graphs = new List<BarGraphDomain>();
            foreach (var dataSet in _graphData)
            {
                var graph = new BarGraphDomain();
                foreach (var item in dataSet)
                {
                    graph.PrimaryGraphDetailList.Add(
                        // As in the GraphHelper code, the `float` is converted
                        // to a `dynamic`, and the `uint` is converted to a
                        // `double`.
                        new KeyValuePair<dynamic, double>(
                            item.Key, item.Value));
                }

                graph.CalculateHistogramStatistics();
                graphs.Add(graph);
            }

            var expected = readExpectedResults("RandHistExpectedStats.csv");
            Assert.AreEqual(graphs[0].Mean, expected[0].Mean, 0.000005);
            Assert.AreEqual(graphs[0].Mode, expected[0].Mode, 0.000005);
            Assert.AreEqual(graphs[0].StandardDeviation, expected[0].StandardDeviation, 0.000005);
            Assert.AreEqual(graphs[1].Mean, expected[1].Mean, 0.000005);
            Assert.AreEqual(graphs[1].Mode, expected[1].Mode, 0.000005);
            Assert.AreEqual(graphs[1].StandardDeviation, expected[1].StandardDeviation, 0.000005);
            Assert.AreEqual(graphs[2].Mean, expected[2].Mean, 0.000005);
            Assert.AreEqual(graphs[2].Mode, expected[2].Mode, 0.000005);
            Assert.AreEqual(graphs[2].StandardDeviation, expected[2].StandardDeviation, 0.000005);
        }
    }
}
