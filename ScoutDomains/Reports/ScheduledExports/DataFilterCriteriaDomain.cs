using ScoutDomains.Analysis;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities;

namespace ScoutDomains.Reports.ScheduledExports
{
    public class DataFilterCriteriaDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public DataFilterCriteriaDomain()
        {
            FromDate = DateTime.Now;
            ToDate = DateTime.Now;

            IsAllCellTypeSelected = true;
        }

        public bool DataFilterIsValid()
        {
            if (SelectedCellTypeOrQualityControlGroup == null && !IsAllCellTypeSelected)
            {
                Log.Warn($"SelectedCellTypeOrQualityControlGroup == null && !IsAllCellTypeSelected");
                return false;
            }

            if (string.IsNullOrEmpty(SelectedUsername))
            {
                Log.Warn($"string.IsNullOrEmpty(SelectedUsername)");
                return false;
            }

            return true;
        }

        public bool SinceLastExport // todo: not sure what this is for
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public eFilterItem FilterType
        {
            get { return GetProperty<eFilterItem>(); }
            set { SetProperty(value); }
        }
        
        public DateTime FromDate
        {
            get { return GetProperty<DateTime>(); }
            set 
            {
                var newDt = DateTimeConversionHelper.DateTimeToStartOfDay(value);
                SetProperty(newDt);
            }
        }

        public DateTime ToDate
        {
            get { return GetProperty<DateTime>(); }

            set 
            {
                var newDt = DateTimeConversionHelper.DateTimeToEndOfDay(value);
                SetProperty(newDt);
            }
        }

        public ObservableCollection<string> Usernames
        {
            get { return GetProperty<ObservableCollection<string>>(); }
            set { SetProperty(value); }
        }

        public string SelectedUsername
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Tag
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string SampleSearchString
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
        
        public bool IsAllCellTypeSelected
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        
        public ObservableCollection<CellTypeQualityControlGroupDomain> CellTypesAndQualityControls
        {
            get { return GetProperty<ObservableCollection<CellTypeQualityControlGroupDomain>>(); }
            set { SetProperty(value); }
        }

        public CellTypeQualityControlGroupDomain SelectedCellTypeOrQualityControlGroup
        {
            get { return GetProperty<CellTypeQualityControlGroupDomain>(); }
            set { SetProperty(value); }
        }
        
        public object Clone()
        {
            var cloneObj = (DataFilterCriteriaDomain) MemberwiseClone();
            CloneBaseProperties(cloneObj);

            cloneObj.CellTypesAndQualityControls = new ObservableCollection<CellTypeQualityControlGroupDomain>();
            if (CellTypesAndQualityControls != null)
            {
                foreach (var item in CellTypesAndQualityControls)
                {
                    cloneObj.CellTypesAndQualityControls.Add((CellTypeQualityControlGroupDomain)item.Clone());
                }

                cloneObj.SelectedCellTypeOrQualityControlGroup = cloneObj.CellTypesAndQualityControls.FirstOrDefault(i => i.Name.Equals(SelectedCellTypeOrQualityControlGroup.Name));
            }
            
            cloneObj.Usernames = new ObservableCollection<string>();
            if (Usernames != null)
            {
                foreach (var u in Usernames)
                {
                    cloneObj.Usernames.Add(u);
                }
            }

            return cloneObj;
        }
    }
}