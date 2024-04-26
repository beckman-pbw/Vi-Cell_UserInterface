// ***********************************************************************
// <copyright file="SampleResultRecordExportDomain.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ScoutUtilities.Common;

namespace ScoutDomains
{
    public class SampleResultRecordExportDomain : BaseNotifyPropertyChanged
    {
        public string SampleId { get; set; }

        public string Dilution { get; set; }

        public string Wash { get; set; }

        public string CellType { get; set; }

        public string MinimumDiameter { get; set; }

        public string MaximumDiameter { get; set; }

        public string Images { get; set; }

        public string CellSharpness { get; set; }

        public string MinimumCircularity { get; set; }

        public string DeclusterDegree { get; set; }

        public string AnalysisType { get; set; }

        public string ViableSpotBrightness { get; set; }

        public string ViableSpotArea { get; set; }

        public string Tag { get; set; }

        public string AnalysisDateTime { get; set; }

        public string ReAnalysisDateTime { get; set; }

        public string AnalysisBy { get; set; }

        public string ReAnalysisBy { get; set; }

        public string TotalCells { get; set; }

        public string ViableCells { get; set; }

        public string Concentration { get; set; }

        public string ViableConcentration { get; set; }

        public string Viability { get; set; }

        public string Size { get; set; }

        public string ViableSize { get; set; }

        public string Circularity { get; set; }

        public string ViableCircularity { get; set; }

        public string AvgBckGroundBrightFieldIntensity { get; set; }

        public string Bubbles { get; set; }

        public string ClusterCount { get; set; }

        public string TotalImages { get; set; }

        public string QCStatus { get; set; }
    }
}