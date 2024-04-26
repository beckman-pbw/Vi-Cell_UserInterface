using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.UIConfiguration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;


namespace ScoutViewModels.ViewModels.Dialogs
{
    public class MessageHubViewModel : BaseDialogViewModel
    {
        public MessageHubViewModel(MessageHubEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            Messages = new ObservableCollection<HubMessage>(args.Messages);
            
            MessagesCollectionView = CollectionViewSource.GetDefaultView(Messages);
            MessagesCollectionView.SortDescriptions.Add(new SortDescription("Time", ListSortDirection.Descending));
            MaxSize = UISettings.MaximumRecentMessages;
        }

        #region Properties

        public int MaxSize
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public ICollectionView MessagesCollectionView
        {
            get { return GetProperty<ICollectionView>(); }
            private set { SetProperty(value); }
        }

        public ObservableCollection<HubMessage> Messages
        {
            get { return GetProperty<ObservableCollection<HubMessage>>(); }
            set 
            { 
                SetProperty(value);
            }
        }

        #endregion

        #region Commands

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand => _closeCommand
            ?? (_closeCommand = new RelayCommand(CloseWindowCommand, null));

        private void CloseWindowCommand(object param)
        {
            // We need to mark the messages as having been 'read' here, since the MessageHubView
            // (dialog) displays the snapshot of the messages at the time the button
            // (on the TitleBar) was being pressed, and since then new messages
            // might have been arriving that have not been yet shown to the user. 
            foreach (var msg in Messages)
                msg.TimesShown++;

            Messages.Clear();
            Close(true);
        }

        #endregion
    }
}
