using ScoutDomains;
using ScoutModels;
using ScoutModels.Review;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels
{
    public class SignatureViewModel : BaseViewModel
    {
        public SignatureViewModel(ReviewModel reviewModel)
        {
        }

        public List<SignatureDomain> SignatureList
        {
            get
            {
                var list = GetProperty<List<SignatureDomain>>();
                if (list != null) return list;

                list = new List<SignatureDomain>();
                var availSigns = ReviewModel.RetrieveSignatureDefinitions();
                availSigns.ForEach(a => list.Add(new SignatureDomain(a.SignatureMeaning, a.SignatureIndicator)));
                SetProperty(list);

                return GetProperty<List<SignatureDomain>>();
            }
            set { SetProperty(value); }
        }

        public string UserId
        {
            get 
            { 
                var user = GetProperty<string>();
                if (string.IsNullOrEmpty(user)) return LoggedInUser.CurrentUserId;
                return user;
            }
            set { SetProperty(value); }
        }
    }
}
