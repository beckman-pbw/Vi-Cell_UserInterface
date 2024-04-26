using ScoutDomains.Analysis;
using ScoutUtilities.Common;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutDomains.RunResult
{
    public class ResultSummaryDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public ResultSummaryDomain()
        {
	        AnalysisDomain = new AnalysisDomain();
            SelectedSignature = new SignatureInstanceDomain();
        }

        public uuidDLL UUID { get; set; }
        public string UserId { get; set; }
        public UInt64 TimeStamp { get; set; }
        public CellTypeDomain CellTypeDomain { get; set; }

        public AnalysisDomain AnalysisDomain
		{
            get { return GetProperty<AnalysisDomain>(); }
            set { SetProperty(value); }
        }

        public BasicResultDomain CumulativeResult
        {
            get { return GetProperty<BasicResultDomain>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<SignatureInstanceDomain> SignatureList
        {
            get { return GetProperty<ObservableCollection<SignatureInstanceDomain>>(); }
            set { SetProperty(value); }
        }

        public SignatureInstanceDomain SelectedSignature
        {
            get { return GetProperty<SignatureInstanceDomain>(); }
            set { SetProperty(value); }
        }

        public DateTime RetrieveDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public object Clone()
        {
            var cloneObj = (ResultSummaryDomain)MemberwiseClone();
            CloneBaseProperties(cloneObj);
            cloneObj.UUID = (uuidDLL) UUID.Clone();
            if (AnalysisDomain != null) cloneObj.AnalysisDomain = (AnalysisDomain)AnalysisDomain.Clone();
            if (CellTypeDomain != null) cloneObj.CellTypeDomain = (CellTypeDomain)CellTypeDomain.Clone();
            if (CumulativeResult != null) cloneObj.CumulativeResult = (BasicResultDomain)CumulativeResult.Clone();

            if (SignatureList != null)
            {
                cloneObj.SignatureList = SignatureList.Select(s =>
                    (SignatureInstanceDomain)s.Clone()).ToObservableCollection();
                cloneObj.SelectedSignature = cloneObj.SignatureList.FirstOrDefault();
            }

            return cloneObj;
        }

        public QCStatus QCStatus
        {
	        get { return GetProperty<QCStatus>(); }
	        set { SetProperty(value); }
        }
    }
}