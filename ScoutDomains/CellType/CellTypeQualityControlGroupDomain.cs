using System;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutDomains
{
    public static class CellTypeQualityControlExtensions
    {
        public static CellTypeQualityControlGroupDomain GetCellTypeQualityControlByIndex(this IList<CellTypeQualityControlGroupDomain> groupList, uint cellTypeIndex)
        {
            return groupList.SelectMany(x => x.CellTypeQualityControlChildItems)
                .FirstOrDefault(x => x.CellTypeIndex == cellTypeIndex);
        }
        public static CellTypeQualityControlGroupDomain GetCellTypeQualityControlByName(this IList<CellTypeQualityControlGroupDomain> groupList, string name)
        {
            return groupList.SelectMany(x => x.CellTypeQualityControlChildItems)
                .FirstOrDefault((x => (x.Name == name) || x.KeyName == name) );
        }

        public static CellTypeQualityControlGroupDomain FirstOrNull(this IList<CellTypeQualityControlGroupDomain> groupList)
        {
            return groupList.FirstOrDefault()?.CellTypeQualityControlChildItems.FirstOrDefault();
        }
    }

    public class CellTypeQualityControlGroupDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public CellTypeQualityControlGroupDomain()
        {
            CellTypeQualityControlChildItems = new ObservableCollection<CellTypeQualityControlGroupDomain>();
        }

        public CtBpQcType SelectedCtBpQcType
        {
            get { return GetProperty<CtBpQcType>(); }
            set { SetProperty(value); }
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string KeyName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public uint CellTypeIndex
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public uint AppTypeIndex
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<CellTypeQualityControlGroupDomain> CellTypeQualityControlChildItems
        {
            get { return GetProperty<ObservableCollection<CellTypeQualityControlGroupDomain>>(); }
            set { SetProperty(value); }
        }

        public bool HasValue
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSelectionActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool HasValueCount
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public object Clone()
        {
            var clone = (CellTypeQualityControlGroupDomain) MemberwiseClone();
            CloneBaseProperties(clone);
            clone.CellTypeQualityControlChildItems = new ObservableCollection<CellTypeQualityControlGroupDomain>();
            foreach (var item in CellTypeQualityControlChildItems)
            {
                clone.CellTypeQualityControlChildItems.Add((CellTypeQualityControlGroupDomain) item.Clone());
            }
            return clone;
        }

        public bool Equals(CellTypeQualityControlGroupDomain other)
        {
            if (other == null)
                return false;

            return this.Name.Equals(other.Name) &&
                   this.SelectedCtBpQcType.Equals(other.SelectedCtBpQcType) &&
                   this.CellTypeIndex.Equals(other.CellTypeIndex) &&
                   this.AppTypeIndex.Equals(other.AppTypeIndex) &&
                   this.IsSelectionActive.Equals(other.IsSelectionActive) &&
                   this.HasValue.Equals(other.HasValue) &&
                   this.HasValueCount.Equals(other.HasValueCount) &&
                   this.KeyName.Equals(other.KeyName) &&
                   this.CellTypeQualityControlChildItems.Count.Equals(other.CellTypeQualityControlChildItems.Count);
        }
    }
}