using ScoutDomains.DataTransferObjects;
using ScoutDomains.Reagent;
using ScoutDomains.RunResult;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ScoutDomains
{
    // Used to delay retrieval of RecordRecord objects
    public delegate HawkeyeError ResultRecordRetriever(uuidDLL[] ids, out List<ResultRecordDomain> resultRecords);

    public class SampleRecordDomain : BaseNotifyPropertyChanged, IEquatable<SampleRecordDomain>, ICloneable
    {
        public SampleRecordDomain()
        {
            SampleImageList = new ObservableCollection<SampleImageRecordDomain>();
            ResultSummaryList = new ObservableCollection<ResultSummaryDomain>();
            ImageIndexList = new ObservableCollection<KeyValuePair<int, string>>();
            ShowParameterList = new List<KeyValuePair<string, string>>();
            RegentInfoRecordList = new List<ReagentInfoRecordDomain>();
            IsSampleHasBubble = Visibility.Hidden;
        }

        #region Properties & Fields

        private Dictionary<uuidDLL, ResultRecordDomain> _cachedRecordList = new Dictionary<uuidDLL, ResultRecordDomain>();

        public bool IsSingleExportStatus { get; set; }
        public uuidDLL UUID { get; set; }
        public uint NumOfResultRecord { get; set; }
        public ImageSetDto ImageSet { get; set; }
        public uuidDLL[] ImageSetIds { get; set; }
        public uuidDLL[] ResultRecordIds { get; set; }
        public SampleHierarchyType SampleHierarchy { get; set; }
        public SubstrateType SubstrateType { get; set; }

        public bool IsSampleCompleted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public KeyValuePair<int, string> SelectedImageIndex
        {
            get { return GetProperty<KeyValuePair<int,string>>(); }
            set
            {
                SetProperty(value);
                if (ImageIndexList != null && ImageIndexList.Count > 0)
                {
                    SetImage(ImageIndexList.IndexOf(value));
                }
            }
        }

        public bool IsSettingsCheckboxEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string Tag
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsSelected
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public uint NumImageSets
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public bool HasValue
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SamplePosition Position
        {
            get { return GetProperty<SamplePosition>(); }
            set { SetProperty(value); }
        }

        public string SampleIdentifier
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string UserId
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public UInt64 TimeStamp
        {
            get { return GetProperty<UInt64>(); }
            set { SetProperty(value); }
        }

        public DateTime RetrieveDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public string BpQcName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string DilutionName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SamplePostWash WashName
        {
            get { return GetProperty<SamplePostWash>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<SampleImageRecordDomain> SampleImageList
        {
            get { return GetProperty<ObservableCollection<SampleImageRecordDomain>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<ResultSummaryDomain> ResultSummaryList
        {
            get { return GetProperty<ObservableCollection<ResultSummaryDomain>>(); }
            set { SetProperty(value); }
        }

        private ResultSummaryDomain _selectedResultSummary;
        public ResultSummaryDomain SelectedResultSummary
        {
            get { return _selectedResultSummary ?? (_selectedResultSummary = ResultSummaryList.FirstOrDefault()); }
            set
            {
                _selectedResultSummary = value;
                NotifyPropertyChanged(nameof(SelectedResultSummary));
            }
        }

        public SampleImageRecordDomain SelectedSampleImageRecord
        {
            get { return GetProperty<SampleImageRecordDomain>(); }
            set
            {
                if (value != null)
                {
                    // We need to "clear" the image before we can set it proper and have it reload on screen (see PC3549-3840, PC3549-3935)
                    SetProperty(default(SampleImageRecordDomain));
                }

                SetProperty(value);
            }
        }

        public ObservableCollection<KeyValuePair<int, string>> ImageIndexList
        {
            get { return GetProperty<ObservableCollection<KeyValuePair<int, string>>>(); }
            set { SetProperty(value); }
        }

        public List<KeyValuePair<string, string>> ShowParameterList
        {
            get { return GetProperty<List<KeyValuePair<string, string>>>(); }
            set { SetProperty(value); }
        }

        public ResultRecordRetriever ResultRecordsRetriever
        {
            get { return GetProperty<ResultRecordRetriever>(); }
            set { SetProperty(value); }
        }

        public Visibility IsSampleHasBubble
        {
            get { return GetProperty<Visibility>(); }
            set { SetProperty(value); }
        }

        public sample_completion_status SampleCompletionStatus
        {
            get { return GetProperty<sample_completion_status>(); }
            set { SetProperty(value); }
        }

        public List<ReagentInfoRecordDomain> RegentInfoRecordList
        {
            get { return GetProperty<List<ReagentInfoRecordDomain>>(); }
            set { SetProperty(value); }
        }

        #endregion

        public bool IsReanalyzedResult(ResultSummaryDomain result)
        {
            return !result.RetrieveDate.Equals(RetrieveDate);
        }

        public void UpdateSampleBubbleStatus()
        {
            if (SampleImageList != null)
            {
                if (SampleImageList.Any(sampleImage =>
                    sampleImage.ResultPerImage != null && sampleImage.ResultPerImage.ProcessedStatus != E_ERRORCODE.eSuccess))
                {
                    IsSampleHasBubble = Visibility.Visible;
                    return;
                }
            }
            IsSampleHasBubble = Visibility.Hidden;
        }

        // Return a list of copies of the SampleRecordDomain, for which *each* copy is associated
        // with exactly *one* ResultRecordDomain from the current list of associated result-records.
        public List<SampleRecordDomain> FlattenResultSummaries_wrappedInDuplicateSamples()
        {
            var flatResultListWrappedInParentSampleCopies = new List<SampleRecordDomain>();
            foreach (var resultSummary in ResultSummaryList)
            {
                var sampleRecordDomain = WrapResultRecord_inParentSampleCopy(resultSummary.UUID);
                string ctName = "";
                if(!string.IsNullOrEmpty(resultSummary.CellTypeDomain?.CellTypeName))
                    ctName = resultSummary.CellTypeDomain.CellTypeName;

                if (string.IsNullOrEmpty(sampleRecordDomain?.BpQcName))
                    sampleRecordDomain.SelectedResultSummary.CellTypeDomain.QCCellTypeForDisplay = ctName;
                else
                    sampleRecordDomain.SelectedResultSummary.CellTypeDomain.QCCellTypeForDisplay = Misc.GetParenthesisQualityControlName(sampleRecordDomain.BpQcName, ctName);

                flatResultListWrappedInParentSampleCopies.Add(sampleRecordDomain);
            }

            foreach (var srd in flatResultListWrappedInParentSampleCopies)
            {
                if (srd.ResultSummaryList == null)
                    srd.ResultSummaryList = new ObservableCollection<ResultSummaryDomain>();

                var selectedSummary = srd.ResultSummaryList.FirstOrDefault();

                srd.ResultSummaryList.Clear();
                foreach (var x in ResultSummaryList)
                {
                    srd.ResultSummaryList.Add(x);
                }

                srd.SelectedResultSummary = selectedSummary;
            }

            // the parent (oldest) should be the first in the list
            var index = GetIndexOfOldestResultSummary(flatResultListWrappedInParentSampleCopies, out var oldest);
            flatResultListWrappedInParentSampleCopies.RemoveAt(index);
            flatResultListWrappedInParentSampleCopies.Insert(0, oldest);

            return flatResultListWrappedInParentSampleCopies;
        }

        private int GetIndexOfOldestResultSummary(List<SampleRecordDomain> samples, out SampleRecordDomain oldest)
        {
            oldest = null;

            var oldestResult = samples.OrderBy(s => s.SelectedResultSummary.RetrieveDate).FirstOrDefault()?.SelectedResultSummary;
            var index = samples.Count - 1;
            if (oldestResult == null) return index;
            
            for (var i = 0; i < samples.Count; i++)
            {
                var s = samples[i];
                if (s.SelectedResultSummary.UUID.Equals(oldestResult.UUID))
                {
                    oldest = s;
                    return i;
                }
            }

            return index;
        }

        public object Clone()
        {
            var cloneObj = (SampleRecordDomain) MemberwiseClone();
            CloneBaseProperties(cloneObj);

            if (!UUID.IsEmpty())
                cloneObj.UUID = (uuidDLL) UUID.Clone();

            if (ImageSet != null)
                cloneObj.ImageSet = (ImageSetDto) ImageSet.Clone();

            if (ImageSetIds != null)
                cloneObj.ImageSetIds = ImageSetIds.Select(i => (uuidDLL) i.Clone()).ToArray();

            if (ResultRecordIds != null)
                cloneObj.ResultRecordIds = ResultRecordIds.Select(r => (uuidDLL) r.Clone()).ToArray();

            if (ShowParameterList != null)
            {
                cloneObj.ShowParameterList = new List<KeyValuePair<string, string>>();
                foreach (var item in ShowParameterList)
                    cloneObj.ShowParameterList.Add(new KeyValuePair<string, string>(item.Key, item.Value));
            }

            if (ResultSummaryList != null)
                cloneObj.ResultSummaryList = ResultSummaryList.Select(x =>
                    (ResultSummaryDomain) x.Clone()).ToObservableCollection();

            if (SelectedResultSummary != null)
                cloneObj.SelectedResultSummary = ResultSummaryList?.FirstOrDefault(r =>
                    r.UUID.Equals(SelectedResultSummary.UUID));

            if (SampleImageList != null)
                cloneObj.SampleImageList = SampleImageList.Select(x =>
                    (SampleImageRecordDomain) x.Clone()).ToObservableCollection();

            if (SelectedSampleImageRecord != null)
                cloneObj.SelectedSampleImageRecord = SampleImageList?.FirstOrDefault(s =>
                    s.UUID.Equals(SelectedSampleImageRecord.UUID));

            if (ResultRecordsRetriever != null)
                cloneObj.ResultRecordsRetriever = ResultRecordsRetriever;

            if (RegentInfoRecordList != null)
                cloneObj.RegentInfoRecordList = RegentInfoRecordList.Select(r =>
                    (ReagentInfoRecordDomain) r.Clone()).ToList();

            if (ImageIndexList != null)
            {
                cloneObj.ImageIndexList = new ObservableCollection<KeyValuePair<int, string>>();
                foreach (var kvp in ImageIndexList)
                    cloneObj.ImageIndexList.Add(new KeyValuePair<int, string>(kvp.Key, kvp.Value));
            }

            if (_cachedRecordList != null)
            {
                var cloneDictionary = new Dictionary<uuidDLL, ResultRecordDomain>();
                foreach (var item in _cachedRecordList)
                    cloneDictionary.Add((uuidDLL) item.Key.Clone(), (ResultRecordDomain) item.Value.Clone());
                cloneObj._cachedRecordList = cloneDictionary;
            }

            return cloneObj;
        }

        public ResultRecordDomain GetResultRecord(uuidDLL resultUuidFromCurrentSample)
        {
            if (!ResultSummaryList.Any(x => x.UUID.Equals(resultUuidFromCurrentSample)))
                return null;
            if (!_cachedRecordList.ContainsKey(resultUuidFromCurrentSample))
                GetResultRecordList(new uuidDLL[] { resultUuidFromCurrentSample });
            return _cachedRecordList.ContainsKey(resultUuidFromCurrentSample) ? _cachedRecordList[resultUuidFromCurrentSample] : null;
        }

        public void SortImages()
        {
            SampleImageList = new ObservableCollection<SampleImageRecordDomain>(SampleImageList.OrderBy(x => x.SequenceNumber));
            ImageIndexList = new ObservableCollection<KeyValuePair<int, string>>(ImageIndexList.OrderBy(imageCount => imageCount.Key));
            if (ImageIndexList.Any()) SelectedImageIndex = ImageIndexList.First();
        }

        private SampleRecordDomain WrapResultRecord_inParentSampleCopy(uuidDLL resultRecordId)
        {
            var clone = (SampleRecordDomain)Clone();
            clone.ResultRecordIds = new uuidDLL[] { resultRecordId };
            if (ResultSummaryList != null)
            {
                // Restrict the result-record list to *only* the specified record
                clone.ResultSummaryList =
                    ResultSummaryList.Where(result => result.UUID.Equals(resultRecordId)).ToObservableCollection();
            }
            // Otherwise, the result-record list will be auto-populated

            return clone;
        }

        private void SetImage(int selectedImageIndex)
        {
            if (selectedImageIndex >= 0
                && SampleImageList.Any()
                && SampleImageList.Count > selectedImageIndex)
            {
                var sampleImageRecord = SampleImageList[selectedImageIndex];
                SelectedSampleImageRecord = sampleImageRecord;
            }
        }

        private void GetResultRecordList(uuidDLL[] uuidList)
        {
            // Select uuids for which result record have not been retrieved yet or the ResultRecordDomain" is null 
            var uuids = uuidList.Where(x => !_cachedRecordList.ContainsKey(x) || _cachedRecordList[x] == null).ToArray();

            if (!uuids.Any())
                return;
            List<ResultRecordDomain> records;
            var he = ResultRecordsRetriever(uuids, out records);
            var msg = Logger.GetHawkeyeErrorMessage(he);
            if (!string.IsNullOrEmpty(msg)) 
                Log.Warn(msg);

            if (he != HawkeyeError.eSuccess || records == null || uuids.Length != records.Count)
            {
                _cachedRecordList.Clear();
                return;
            }

            for (var index = 0; index < records.Count; ++index)
            {
                if (!_cachedRecordList.ContainsKey(uuids[index]))
                    _cachedRecordList.Add(uuids[index], records[index]);
                else
                    _cachedRecordList[uuids[index]] = records[index];
            }
        }

        public bool Equals(SampleRecordDomain other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (SampleIdentifier == null && other.SampleIdentifier == null)
                return true;
            if (SampleIdentifier != null && other.SampleIdentifier == null)
                return false;
            if (SampleIdentifier == null && other.SampleIdentifier != null)
                return false;

            return SampleIdentifier.Equals(other.SampleIdentifier) && UUID.Equals(other.UUID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SampleRecordDomain) obj);
        }

        public override int GetHashCode()
        {
            return UUID.GetHashCode();
        }

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }
    }
}
