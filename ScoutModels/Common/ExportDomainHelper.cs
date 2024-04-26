using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutModels.Common
{
    public static class ExportDomainHelper
    {
        private const int DefaultValue = 0;

        public static List<SampleDomain> ConvertExportDomainToSampleList(IEnumerable<ExportQueueCreationDomain> collections)
        {
            var list = new List<SampleDomain>();
            var ctQcs = CellTypeFacade.Instance.GetAllCtQcGroupList_BECall();

            foreach (var item in collections)
            {
                var sample = new SampleDomain
                {
                    SelectedCellTypeQualityControlGroup = ctQcs.GetCellTypeQualityControlByName(item.CellTypes),
                    ItemStatus = item.StageTypeAsString,
                    SampleID = item.SampleIDs,
                    BpQcName = item.CellTypes,
                    SelectedDilution = item.Dilution,
                    SelectedWash = item.Wash,
                    Comment = item.Comment,
                    SampleStatusColor = SampleStatusColor.Defined,
                    RowWisePosition = item.RowColWise,
                    NthImage = item.NthImage,
                    IsExportPDFSelected = item.IsExportPdfResultExport,
                    IsExportEachSampleActive = item.IsExportEachSampleActive,
                    IsAppendResultExport = item.IsAppendResultExport,
                    ExportPathForEachSample = item.ExportEachSample,
                    AppendResultExport = item.AppendResultExport,
                    ExportFileName = item.ExportFileName,
                    SampleRowPosition = item.Positions,
                    Position = item.Positions,
                    SamplePosition = SamplePosition.Parse(item.Positions),
                };

                list.Add(sample);
            }

            return list;
        }

        public static bool ValidateCellTypeAndQualityControlOnImport(IEnumerable<ExportQueueCreationDomain> sampleList, List<CellTypeDomain> cellTypeList)
        {
            foreach (var sample in sampleList)
            {
                switch (sample.SelectedQcCtBpQcType)
                {
                    case CtBpQcType.CellType:
                        //Validate the imported cell type details with system cell type details
                        if (!ValidateCellTypeOnImport(sample, cellTypeList))
                        {
                            //Cell type validation message
                            DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_CellType_Validation"));
                            return false;
                        }
                        break;
                    case CtBpQcType.QualityControl:
                        //Validate the imported quality control details with system quality control details
                        if (!ValidateQualityControlOnImport(sample, cellTypeList))
                        {
                            //Quality control validation message
                            DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_QualityControl_Validation"));
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        public static ExportQueueCreationQualityControlDomain GetQualityControlDetailsForExport(string qcName)
        {
            qcName = Misc.GetBaseQualityControlName(qcName);
            var qualityControlList = CellTypeFacade.Instance.GetAllQualityControls_BECall(out var allCells);
            var qualityControlDetailsFromApi = qualityControlList?.FirstOrDefault(q => q.QcName.Equals(qcName));
            var qualityControlDetails = new ExportQueueCreationQualityControlDomain();
            if (qualityControlDetailsFromApi == null)
                return qualityControlDetails.GetEmptyData();
            qualityControlDetails.QcName = qualityControlDetailsFromApi.QcName;
            qualityControlDetails.CellTypeName = qualityControlDetailsFromApi.CellTypeName;
            qualityControlDetails.CellTypeIndex = qualityControlDetailsFromApi.CellTypeIndex;
            qualityControlDetails.AssayParameter = (int)qualityControlDetailsFromApi.AssayParameter;
            qualityControlDetails.LotInformation = qualityControlDetailsFromApi.LotInformation;
            qualityControlDetails.AcceptanceLimit = qualityControlDetailsFromApi.AcceptanceLimit;
            qualityControlDetails.ExpirationDate = qualityControlDetailsFromApi.ExpirationDate;
            qualityControlDetails.Comments = qualityControlDetailsFromApi.CommentText;
            qualityControlDetails.AssayValue = qualityControlDetailsFromApi.AssayValue;
            return qualityControlDetails;
        }

        public static ExportQueueCreationCellTypeDomain GetCellTypeDetailsForExport(uint cellTypeIndex, List<CellTypeDomain> cellTypeList)
        {
            var cellTypeDetailsFromApi = cellTypeList.FirstOrDefault(c => c.CellTypeIndex == cellTypeIndex);
            var cellTypeDetails = new ExportQueueCreationCellTypeDomain();

            if (cellTypeDetailsFromApi == null)
                return cellTypeDetails.GetEmptyData();

            cellTypeDetails.CellTypeIndex = cellTypeDetailsFromApi.CellTypeIndex;
            cellTypeDetails.CellTypeName = cellTypeDetailsFromApi.CellTypeName;
            cellTypeDetails.MinimumDiameter = cellTypeDetailsFromApi.MinimumDiameter;
            cellTypeDetails.MaximumDiameter = cellTypeDetailsFromApi.MaximumDiameter;
            cellTypeDetails.Images = cellTypeDetailsFromApi.Images;
            cellTypeDetails.CellSharpness = cellTypeDetailsFromApi.CellSharpness;
            cellTypeDetails.MinimumCircularity = cellTypeDetailsFromApi.MinimumCircularity;
            cellTypeDetails.DeclusterDegree = cellTypeDetailsFromApi.DeclusterDegree.ToString();
            cellTypeDetails.AspirationCycles = cellTypeDetailsFromApi.AspirationCycles;
            cellTypeDetails.MixingCycle = cellTypeDetailsFromApi.AnalysisDomain.MixingCycle;

            foreach (var ap in cellTypeDetailsFromApi.AnalysisDomain.AnalysisParameter)
            {
                switch (ap.Label)
                {
                    case ApplicationConstants.CellSpotArea:
                        if (ap.ThresholdValue != null)
                            cellTypeDetails.ViableSpotArea = Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
                        break;
                    case ApplicationConstants.AvgSpotBrightness:
                        if (ap.ThresholdValue != null)
                            cellTypeDetails.ViableSpotBrightness = Misc.UpdateTrailingPoint(ap.ThresholdValue.Value, TrailingPoint.One);
                        break;
                }
            }

            return cellTypeDetails;
        }

        private static bool ValidateCellTypeOnImport(ExportQueueCreationDomain sample, List<CellTypeDomain> cellTypeList)
        {
            var cellTypeDomain =
                cellTypeList.FirstOrDefault(c => c.CellTypeName.Equals(sample.CellTypeDetails.CellTypeName));
            if (cellTypeDomain == null)
                return false;
            if (!cellTypeDomain.CellTypeName.Equals(sample.CellTypeDetails.CellTypeName) ||
                cellTypeDomain.AspirationCycles != sample.CellTypeDetails.AspirationCycles ||
                cellTypeDomain.Images != sample.CellTypeDetails.Images ||
                !cellTypeDomain.DeclusterDegree.ToString().Equals(sample.CellTypeDetails.DeclusterDegree) ||
                cellTypeDomain.AnalysisDomain.MixingCycle != sample.CellTypeDetails.MixingCycle)
                return false;
            //If imported sample CellDetails match the existing cell details. But has different index, then update with existing/local cell index.
            if (cellTypeDomain.CellTypeIndex != sample.CellTypeDetails.CellTypeIndex)
                sample.CellTypeDetails.CellTypeIndex = cellTypeDomain.CellTypeIndex;
            if (cellTypeDomain.MinimumDiameter == null)
                cellTypeDomain.MinimumDiameter = DefaultValue;
            if (sample.CellTypeDetails?.MinimumDiameter == null)
                sample.CellTypeDetails.MinimumDiameter = DefaultValue;
            if (cellTypeDomain.MaximumDiameter == null)
                cellTypeDomain.MaximumDiameter = DefaultValue;
            if (sample.CellTypeDetails?.MaximumDiameter == null)
                sample.CellTypeDetails.MaximumDiameter = DefaultValue;
            if (cellTypeDomain.MinimumCircularity == null)
                cellTypeDomain.MinimumCircularity = DefaultValue;
            if (sample.CellTypeDetails?.MinimumCircularity == null)
                sample.CellTypeDetails.MinimumCircularity = DefaultValue;
            if (cellTypeDomain.CellSharpness == null)
                cellTypeDomain.CellSharpness = DefaultValue;
            if (sample.CellTypeDetails?.CellSharpness == null)
                sample.CellTypeDetails.CellSharpness = DefaultValue;

            if (!cellTypeDomain.MinimumDiameter.Value.Equals(sample.CellTypeDetails?.MinimumDiameter.Value))
                return false;
            if (!cellTypeDomain.MaximumDiameter.Value.Equals(sample.CellTypeDetails?.MaximumDiameter.Value))
                return false;
            if (!cellTypeDomain.MinimumCircularity.Value.Equals(sample.CellTypeDetails?.MinimumCircularity.Value))
                return false;
            return cellTypeDomain.CellSharpness.Value.Equals(sample.CellTypeDetails?.CellSharpness.Value);
        }

        private static bool ValidateQualityControlOnImport(ExportQueueCreationDomain sample, List<CellTypeDomain> cellTypeList)
        {
            var qualityControlList = CellTypeFacade.Instance.GetAllQualityControls_BECall(out var allCells);
            var qcName = Misc.GetBaseQualityControlName(sample.CellTypes);
            var qualityControlDetails =
                qualityControlList?.FirstOrDefault(q => q.QcName.Equals(qcName));
            if (qualityControlDetails == null)
                return false;
            qualityControlDetails.CommentText = qualityControlDetails.CommentText ?? string.Empty;
            if (sample.QualityControlDetails.AcceptanceLimit == null)
                sample.QualityControlDetails.AcceptanceLimit = DefaultValue;
            if (qualityControlDetails.AcceptanceLimit == null)
                qualityControlDetails.AcceptanceLimit = DefaultValue;
            if (sample.QualityControlDetails.AssayValue == null)
                sample.QualityControlDetails.AssayValue = DefaultValue;
            if (qualityControlDetails.AssayValue == null)
                qualityControlDetails.AssayValue = DefaultValue;
            if (sample.QualityControlDetails.ExpirationDate < DateTime.Today)
                return false;
            if (!sample.QualityControlDetails.AcceptanceLimit.Value.Equals(qualityControlDetails.AcceptanceLimit.Value) ||
                sample.QualityControlDetails.AssayParameter != (int)qualityControlDetails.AssayParameter ||
                !sample.QualityControlDetails.AssayValue.Value.Equals(qualityControlDetails.AssayValue.Value) ||
                sample.QualityControlDetails.CellTypeIndex != qualityControlDetails.CellTypeIndex ||
                !sample.QualityControlDetails.Comments.Equals(qualityControlDetails.CommentText) ||
                sample.QualityControlDetails.ExpirationDate != qualityControlDetails.ExpirationDate ||
                !sample.QualityControlDetails.LotInformation.Equals(qualityControlDetails.LotInformation) ||
                !sample.QualityControlDetails.QcName.Equals(qualityControlDetails.QcName))
                return false;
            if (qualityControlDetails.CellTypeIndex > 0)
            {
                return ValidateCellTypeOnImport(sample, cellTypeList);
            }

            return true;
        }
    }
}
