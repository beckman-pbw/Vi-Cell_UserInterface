using ScoutDomains;
using ScoutDomains.Common;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutModels.Common
{
    public class GraphHelper
    {
        public GraphHelper()
        {
        }

        private static LineGraphDomain CreateLineGraphData()
        {
            var graph = new LineGraphDomain();
            return graph;
        }

        public List<LineGraphDomain> GetLineGraphListForQC(List<LineGraphDomain> graphList,
            List<SampleRecordDomain> sampleRecordList, double lowerLimit = 0, double upperLimit = 0)
        {
            var index = 0;
            graphList.ForEach(g =>
            {
                g.XAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Date");
                g.AcceptanceLowerLimit = lowerLimit;
                g.AcceptanceUpperLimit = upperLimit;

                switch (index)
                {
                    case 0:
                        g.YAxisName = g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration");
                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(
                                        Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetConcentrationPoint(sample.SelectedResultSummary.CumulativeResult.ConcentrationML)));
                        }
                        break;
                    case 1:
                        g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Viability");
                        g.YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Viability");
                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetTrailingData(sample.SelectedResultSummary.CumulativeResult.Viability, TrailingPoint.One)));
                        }
                        break;
                    case 2:
                        g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_RunResultsReport_Graph_Diameter_AxisTitle");
                        g.YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_RunResultsReport_Graph_Diameter_AxisTitle");
                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetTrailingData(sample.SelectedResultSummary.CumulativeResult.Size, TrailingPoint.Two)));
                        }
                        break;
                }

                index++;
            });

            return graphList;
        }

        private double GetTrailingData(double graphValue, TrailingPoint trailing)
        {
            return Misc.UpdateDecimalPoint(graphValue, trailing);
        }

        private double GetConcentrationPoint(double graphValue)
        {
            return Misc.UpdateDecimalPoint(Misc.ConvertToPowerSix(graphValue),Misc.ConcDisplayDigits);
        }

        public List<LineGraphDomain> GetLineGraphListForBP(List<LineGraphDomain> graphViewList,
            List<SampleRecordDomain> sampleRecordList)
        {
            var index = 0;
            graphViewList.ForEach(g =>
            {
                g.XAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ElapsedTime");
                switch (index)
                {
                    case 0:
                        g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Viability");
                        g.YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Viability");

                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetTrailingData(sample.ResultSummaryList[0].CumulativeResult.Viability, TrailingPoint.One)));
                        }

                        break;
                    case 1:
                        g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Totalcells");
                        g.YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Totalcells");
                        g.IsMultiAxisEnable = true;
                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                            {
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    sample.ResultSummaryList[0].CumulativeResult.TotalCells));
                                g.MultiGraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    sample.ResultSummaryList[0].CumulativeResult.ViableCells));
                            }
                        }

                        break;
                    case 2:
                        g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Concentration");
                        g.YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_CheckBox_NonviableCellConcentration");
                        g.IsMultiAxisEnable = true;
                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                            {
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetConcentrationPoint(sample.ResultSummaryList[0].CumulativeResult.ConcentrationML)));
                                g.MultiGraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetConcentrationPoint(sample.ResultSummaryList[0].CumulativeResult.ViableConcentration)));
                            }
                        }

                        break;
                    case 3:
                        g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_size");
                        g.YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Size");
                        g.IsMultiAxisEnable = false;
                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                            {
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetTrailingData(sample.ResultSummaryList[0].CumulativeResult.Size, TrailingPoint.Two)));
                            }
                        }

                        break;
                    case 4:
                        g.GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_AverageCircularity");
                        g.YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_AverageCircularity");
                        g.IsMultiAxisEnable = false;
                        foreach (var sample in sampleRecordList)
                        {
                            if (sample.ResultSummaryList.Count > 0)
                            {
                                g.GraphDetailList.Add(new KeyValuePair<dynamic, double>(Misc.ConvertToCustomLongDateTimeFormat(sample.SelectedResultSummary.RetrieveDate),
                                    GetTrailingData(sample.ResultSummaryList[0].CumulativeResult.Circularity, TrailingPoint.Two)));
                            }
                        }

                        break;
                    default:
                        break;
                }

                index++;
            });

            return graphViewList;
        }

        public List<BarGraphDomain> CreateGraph(SampleRecordDomain selectedSample, List<KeyValuePair<int, List<histogrambin_t>>> histogramList, bool isReview)
        {
            var graphViewList = new List<BarGraphDomain>();
            if (selectedSample == null)
                return graphViewList;

            foreach (var item in Enum.GetValues(typeof(GraphType)))
            {
                var graph = new BarGraphDomain();
                switch ((GraphType) item)
                {
                    case GraphType.SizeDistribution:
                    case GraphType.ViableSizeDistribution:
                    case GraphType.CircularityDistribution:
                    case GraphType.ViableCircularityDistribution:
                        if (isReview)
                        {
                            GetHistogramGraphList((GraphType) item, graph, histogramList);
                            graphViewList.Add(graph);
                        }
                        break;
                    case GraphType.Viability:
                    case GraphType.TotalCells:
                    case GraphType.Concentration:
                    case GraphType.AverageSize:
                    case GraphType.AverageCircularity:
                    case GraphType.AverageBackground:
                    case GraphType.Bubble:
                        SetGraph(graph, (GraphType)item, selectedSample);
                        graphViewList.Add(graph);
                        break;
                }
            }

            return graphViewList;
        }

        private void GetHistogramGraphList(GraphType graphType, BarGraphDomain graph,
             List<KeyValuePair<int, List<histogrambin_t>>> histogramList)
        {
            switch (graphType)
            {
                case GraphType.SizeDistribution:
                    graph = SetGraphValue(graph,
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_SizeDistribution"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_RunResultsReport_Graph_Diameter"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Count"), false,
                        GraphType.SizeDistribution);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_SubrangeStatics");
                    graph.PrimaryGraphDetailList = new List<KeyValuePair<dynamic, double>>();
                    if (histogramList.Count == 0)
                        return;
                    if (histogramList[0].Value.Count == 0)
                        return;
                    foreach (var item in histogramList[0].Value)
                        graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(item.bin_nominal_value, 
                            item.count));
                    break;

                case GraphType.ViableSizeDistribution:
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphResult_ViaSize"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_RunResultsReport_Graph_Diameter"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Count"), false,
                        GraphType.ViableSizeDistribution);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_SubrangeStatics");
                    graph.PrimaryGraphDetailList = new List<KeyValuePair<dynamic, double>>();
                    if (histogramList.Count == 0)
                        return;
                    if (histogramList[1].Value.Count == 0)
                        return;
                    foreach (var item in histogramList[1].Value)
                        graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(item.bin_nominal_value, item.count));
                    break;

                case GraphType.CircularityDistribution:
                    graph = SetGraphValue(graph,
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_CircularityDistribution"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Circularity"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Count"), false,
                        GraphType.CircularityDistribution);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_SubrangeStatics");
                    graph.PrimaryGraphDetailList = new List<KeyValuePair<dynamic, double>>();
                    if (histogramList.Count == 0)
                        return;
                    if (histogramList[2].Value.Count == 0)
                        return;
                    foreach (var item in histogramList[2].Value)
                        graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(item.bin_nominal_value,
                            item.count));
                    break;

                case GraphType.ViableCircularityDistribution:
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ViaCir"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Viablecircularity"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Count"), false,
                        GraphType.ViableCircularityDistribution);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_SubrangeStatics");
                    graph.PrimaryGraphDetailList = new List<KeyValuePair<dynamic, double>>();
                    if (histogramList.Count == 0)
                        return;
                    if (histogramList[3].Value.Count == 0)
                        return;
                    foreach (var item in histogramList[3].Value)
                        graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(item.bin_nominal_value,
                            item.count));
                    break;
            }
        }

        private void SetGraph(BarGraphDomain graph, GraphType graphType, SampleRecordDomain selectedSample)
        {
            var graphLock = new object();
            var key = 1;
            var multiList_1 = new List<KeyValuePair<dynamic, double>>();
            var multiList_2 = new List<KeyValuePair<dynamic, double>>();
            graph.MultiGraphDetailList = new List<List<KeyValuePair<dynamic, double>>>();
            graph.MultiLegendNames = new List<string>();
            lock (graphLock)
            {
                var sampleImageRecord = selectedSample.SampleImageList;
                foreach (var resultRecord in sampleImageRecord)
                    GetBarGraphList(graphType, key++, graph, resultRecord, multiList_1, multiList_2);
            }
            graph.MultiLegendNames = new List<string>();
            graph.PrimaryLegendName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Concatenate_Viable");
            if (graph.IsMultiAxisEnable)
            {
                graph.MultiGraphDetailList.Add(multiList_1);
                // Only used for stats table
                graph.MultiGraphDetailList.Add(multiList_2);
                // These are *currently* the only graph types with multiple data series; this check
                // is just here as a safeguard in case that changes
                if (graphType == GraphType.TotalCells || graphType == GraphType.Concentration)
                {
                    graph.MultiLegendNames.Add(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Concatenate_Nonviable"));
                }
            }
        }

        private void GetBarGraphList(GraphType graphType, int key, BarGraphDomain graph,
            SampleImageRecordDomain resultRecord, List<KeyValuePair<dynamic, double>> multiList_1, List<KeyValuePair<dynamic, double>> multiList_2)
        {
            switch (graphType)
            {
                case GraphType.Viability:
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Viability"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Viability"),false, GraphType.Viability);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ImageStat");
                    graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(key,
                        GetTrailingData(resultRecord.ResultPerImage.Viability, TrailingPoint.One)));
                    break;

                case GraphType.TotalCells:
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Totalcells"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Totalcells"),true, GraphType.TotalCells);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ImageStat");
                    graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(key,
                        resultRecord.ResultPerImage.ViableCells));
                    multiList_1.Add(new KeyValuePair<dynamic, double>(key,
                        resultRecord.ResultPerImage.TotalCells - resultRecord.ResultPerImage.ViableCells));
                    // Only used for stats table
                    multiList_2.Add(new KeyValuePair<dynamic, double>(key,
                        resultRecord.ResultPerImage.TotalCells));
                    break;

                case GraphType.Concentration:
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Concentration"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration"), true,
                        GraphType.Concentration);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ImageStat");
                    graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(key,
                        GetConcentrationPoint(resultRecord.ResultPerImage.ViableConcentration)));
                    multiList_1.Add(new KeyValuePair<dynamic, double>(key,
                        GetConcentrationPoint(resultRecord.ResultPerImage.ConcentrationML - resultRecord.ResultPerImage.ViableConcentration)));
                    // Only used for stats table
                    multiList_2.Add(new KeyValuePair<dynamic, double>(key,
                        GetConcentrationPoint(resultRecord.ResultPerImage.ConcentrationML)));
                    break;
                case GraphType.AverageSize:
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Size"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Size"), false, GraphType.AverageSize);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ImageStat");
                    graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(key,
                        GetTrailingData(resultRecord.ResultPerImage.Size, TrailingPoint.Two)));
                    break;
                case GraphType.AverageCircularity:
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_AverageCircularity"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_AverageCircularity"), false,
                        GraphType.AverageCircularity);
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ImageStat");
                    graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(key,
                        GetTrailingData(resultRecord.ResultPerImage.Circularity, TrailingPoint.Two)));
                    break;
                case GraphType.AverageBackground:
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ImageStat");
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Averagebackground"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Averagebackground"), false,
                        GraphType.AverageBackground);
                    graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(key,
                        resultRecord.ResultPerImage.AvgBackground));
                    break;
                case GraphType.Bubble:
                    graph.LegendTitle = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ImageStat");
                    graph = SetGraphValue(graph, ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Bubbles"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                        ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Bubbles"), false, GraphType.Bubble);
                    graph.PrimaryGraphDetailList.Add(new KeyValuePair<dynamic, double>(key,
                        resultRecord.ResultPerImage.Bubble));
                    break;
            }
        }

        private BarGraphDomain SetGraphValue(BarGraphDomain graph, string graphName, string xName, string yName,
            bool isMulti, GraphType graphType)
        {
            graph.GraphName = graphName;
            graph.IsMultiAxisEnable = isMulti;
            graph.XAxisName = xName;
            graph.YAxisName = yName;
            graph.SelectedGraphType = graphType;
            return graph;
        }

        public List<LineGraphDomain> CreateLineGraphList(int count)
        {
            var graphList = new List<LineGraphDomain>();
            for (int i = 0; i < count; i++)
            {
                graphList.Add(CreateLineGraphData());
            }

            return graphList;
        }

        public BarGraphDomain GetAutoFocusGraphData(List<AutofocusDatapoint> dataPointList)
        {
            var barGraph = new BarGraphDomain
            {
                GraphName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_InFocus"),
                XAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_QMgmtHEADER_Position"),
                YAxisName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Count")
            };
            if (dataPointList != null && dataPointList.Any())
            {           
                dataPointList.ForEach(data =>
                {
                    barGraph.PrimaryGraphDetailList.Add(
                        new KeyValuePair<dynamic, double>(data.position, data.focalvalue));
                });
            }

            return barGraph;
        }
    }
}