using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using System;

namespace ScoutDomains.Analysis
{
    public class CellTypeDomain : BaseNotifyPropertyChanged, IListItem, ICloneable
    {
        #region Properties & Fields

        public string ListItemLabel => CellTypeName;

        public uint CellTypeIndex { get; set; }

        public string CellTypeName { get; set; }

        public string QCCellTypeForDisplay { get; set; }

        public AnalysisDomain AnalysisDomain { get; set; }

        public int AspirationCycles { get; set; }

        public bool IsCellEnable
        {
            get { return GetProperty<bool>(); }
            set
            {
                // The first cell type is a default type that is *always* enabled.
                SetProperty(value || CellTypeIndex == 0);
            }
        }

        public bool IsCellSelected
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string TempCellName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsUserDefineCellType { get; set; }

        /// <summary>
        /// When a cell type is used in a quality control, this property is set to true.
        /// </summary>
        public bool IsQualityControlCellType { get; set; }

        public double? MinimumDiameter { get; set; }

        public double? MaximumDiameter { get; set; }

        public int? Images { get; set; }

        public float? CellSharpness { get; set; }

        public eCellDeclusterSetting DeclusterDegree { get; set; }

        public double? MinimumCircularity
        {
            get { return GetProperty<double?>(); }
            set { SetProperty(value); }
        }

        public float? CalculationAdjustmentFactor
        {
            get { return GetProperty<float?>(); }
            set { SetProperty(value); }
        }

        #endregion

        public object Clone()
        {
            var cloneObj = (CellTypeDomain)MemberwiseClone();
            CloneBaseProperties(cloneObj);

            if (cloneObj.AnalysisDomain != null)
                cloneObj.AnalysisDomain = (AnalysisDomain) cloneObj.AnalysisDomain.Clone();
            
            return cloneObj;
        }

        public CellType PopulateNewCellType()
        {
            var cellType = new CellType
            {
                celltype_index = CellTypeIndex,
                label = CellTypeName.ToIntPtr(),
                minimum_diameter_um = MinimumDiameter == null
                    ? default
                    : (float) MinimumDiameter,
                maximum_diameter_um = MaximumDiameter == null
                    ? default
                    : (float) MaximumDiameter,
                max_image_count = Images == null ? default : (ushort) Images,
                minimum_circularity = MinimumCircularity == null
                    ? default
                    : (float) MinimumCircularity,
                sharpness_limit = CellSharpness ?? default,
                decluster_setting = DeclusterDegree,
                aspiration_cycles = (byte) AspirationCycles,
                num_analysis_specializations = 1,
                analysis_specializations = AnalysisDomain.GetAnalysisDefinition().ToPtr(),
                calculation_adjustment_factor = CalculationAdjustmentFactor ?? ApplicationConstants.DefaultAdjustmentFactorValue
            };

            Log.Debug(cellType.ToString());

            return cellType;
        }
    }
}
