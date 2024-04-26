using ScoutDataAccessLayer.IDAL;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class WorkListViewModel : BaseViewModel
    {
        public WorkListViewModel(IList<SampleSetViewModel> sampleSets)
        {
            SampleSets = sampleSets;
            CreatedByUser = LoggedInUser.CurrentUserId;

            var last = sampleSets.LastOrDefault(); // use last because the first is the orphan set
            if (last != null)
            {
                SubstrateType = last.SubstrateType;
                Precession = last.PlatePrecession;
            }
        }


        #region Properties & Fields

        public uuidDLL Uuid
        {
            get { return GetProperty<uuidDLL>(); }
            private set { SetProperty(value); }
        }

        public string Label
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string CreatedByUser
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DateTime CreatedDateTime
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public SubstrateType SubstrateType
        {
            get { return GetProperty<SubstrateType>(); }
            set { SetProperty(value); }
        }

        public Precession Precession
        {
            get { return GetProperty<Precession>(); }
            set { SetProperty(value); }
        }

        public IList<SampleSetViewModel> SampleSets
        {
            get { return GetProperty<IList<SampleSetViewModel>>(); }
            set { SetProperty(value); }
        }

        #endregion
    }
}