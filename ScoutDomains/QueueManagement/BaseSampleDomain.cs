using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System.ComponentModel;

namespace ScoutDomains
{
    /// <summary>
    /// Base ViewModel for SampleDomain classes. SampleDomain extends this class.
    /// </summary>
    public class BaseSampleDomain : BaseNotifyPropertyChanged, IEditableObject
    {
        public BaseSampleDomain()
        {
            SampleID = string.Empty;
            SampleName = string.Empty;
            Comment = string.Empty;
            SelectedDilution = string.Empty;
            ItemStatus = string.Empty;
        }

        #region Properties

        public string SampleID
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                var positionText = $".{SampleRowPosition}";
                SampleName = value.EndsWith(positionText) ? value.Substring(0, value.Length - positionText.Length) : value;
            }
        }

        public CellTypeQualityControlGroupDomain SelectedCellTypeQualityControlGroup
        {
            get { return GetProperty<CellTypeQualityControlGroupDomain>(); }
            set
            {
                SetProperty(value);
                if (SelectedCellTypeQualityControlGroup != null && SelectedCellTypeQualityControlGroup.SelectedCtBpQcType == CtBpQcType.QualityControl)
                {
                    var cellTypeName = SelectedCellTypeQualityControlGroup.Name?.Split('(');
                    if (cellTypeName?[0] != null)
                    {
                        SampleName = cellTypeName[0].Trim();
                    }
                }
            }
        }

        public SamplePosition SamplePosition
        {
            get { return GetProperty<SamplePosition>(); }
            set { SetProperty(value); }
        }

        public string Position
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Comment
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string SelectedDilution
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SamplePostWash SelectedWash
        {
            get { return GetProperty<SamplePostWash>(); }
            set { SetProperty(value); }
        }

        public string ItemStatus
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool InActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #region 96 Well Properties

        public string SampleRowPosition { get; set; }
        public uint CellTypeIndex { get; set; }
        public ushort AnalysisIndex { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        public string SampleName
        {
            get { return GetProperty<string>(); }
            private set { SetProperty(value); }
        }

        public bool IsRunning
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string RowWisePosition
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int Id
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int Row
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int Column
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string ColumnName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string RowName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int Type
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region IEditable

        public void BeginEdit()
        {
            IsInEditMode = true;
        }

        public void EndEdit()
        {
            IsInEditMode = false;
        }

        public void CancelEdit()
        {
            IsInEditMode = false;
        }

        public bool IsInEditMode
        {
            get { return GetProperty<bool>(); }
            private set { SetProperty(value); }
        }

        #endregion

        #endregion
    }
}
