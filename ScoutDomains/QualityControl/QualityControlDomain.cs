using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;

namespace ScoutDomains
{
    public class QualityControlDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public QualityControlDomain()
        {
            ExpirationDate = DateTime.Now;
        }

        #region Properties & Fields

        public string QcName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string CellTypeName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public uint CellTypeIndex
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public string LotInformation
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string CommentText
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public assay_parameter AssayParameter
        {
            get { return GetProperty<assay_parameter>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(AssayValueString));
            }
        }

        public double? AssayValue
        {
            get { return GetProperty<double?>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(AssayValueString));
            }
        }

        public string AssayValueString
        {
            get
            {
                var assayValue = AssayValue ?? 0.0;
                switch (AssayParameter)
                {
                    case assay_parameter.ap_Size:
                    case assay_parameter.ap_Concentration:
                        return Misc.UpdateTrailingPoint(assayValue, TrailingPoint.Two);
                    case assay_parameter.ap_PopulationPercentage:
                        return Misc.UpdateTrailingPoint(assayValue, TrailingPoint.One);
                }

                return string.Empty;
            }
        }

        public int? AcceptanceLimit
        {
            get { return GetProperty<int?>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(AcceptanceLimitString));
            }
        }

        public string AcceptanceLimitString => AcceptanceLimit == null ? string.Empty : "+/- " + AcceptanceLimit + " %";

        public DateTime ExpirationDate
        {
            get { return GetProperty<DateTime>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(NotExpired));
            }
        }

        public bool NotExpired => DateTime.Compare(ExpirationDate.Date, DateTime.Now.Date) != -1;

        #endregion

        #region Methods

        public object Clone()
        {
            var clone = (QualityControlDomain) MemberwiseClone();
            CloneBaseProperties(clone);
            return clone;
        }

        public qualitycontrol_t CreateQualityControlStruct()
        {
            var qualityControl = new qualitycontrol_t
            {
                assay_type = AssayParameter,
                assay_value = Convert.ToDouble(AssayValue),
                cell_type_index = CellTypeIndex,
                comment_text = CommentText.ToIntPtr(),
                expiration_date = DateTimeConversionHelper.DaysElapsedSinceEpochAbsolute(ExpirationDate),
                lot_information = LotInformation.ToIntPtr(),
                plusminus_percentage = Convert.ToDouble(AcceptanceLimit),
                qc_name = Misc.GetBaseQualityControlName(QcName).ToIntPtr()
            };
            return qualityControl;
        }

        #endregion
    }
}
