using ScoutUtilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutDomains.RunResult
{
    public class ResultRecordDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public ResultRecordDomain()
        {
            ResultSummary = new ResultSummaryDomain();
        }

        public ResultSummaryDomain ResultSummary
        {
            get { return GetProperty<ResultSummaryDomain>(); }
            set { SetProperty(value); }
        }

        public List<BasicResultDomain> ResultPerImage
        {
            get { return GetProperty<List<BasicResultDomain>>(); }
            set { SetProperty(value); }
        }

        public object Clone()
        {
            var cloneObj = (ResultRecordDomain) MemberwiseClone();
            CloneBaseProperties(cloneObj);

            cloneObj.ResultPerImage = new List<BasicResultDomain>();
            foreach (var item in ResultPerImage)
            {
                cloneObj.ResultPerImage.Add((BasicResultDomain)item.Clone());
            }

            return cloneObj;
        }
    }
}