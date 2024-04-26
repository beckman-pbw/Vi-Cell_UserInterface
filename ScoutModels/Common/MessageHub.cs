using System;
using System.Collections.ObjectModel;
using ScoutUtilities.Common;
using ScoutUtilities.Events;
using ScoutUtilities.Enums;
using System.Windows.Media;
using ScoutUtilities.UIConfiguration;


namespace ScoutDomains.Common
{
    public class MessageHub : BaseNotifyPropertyChanged
    {
        public MessageHub()
        {
            MaxSize = 100;
            _allMessages = new Collection<HubMessage>();
            ForegroundBrush = new SolidColorBrush(Colors.White);
            BackgroundBrush = new SolidColorBrush(Colors.Transparent);
            MessageBus.Default.Subscribe<SystemMessageDomain>(OnPublishedSystemMessageDomainMessage);
        }

        private Collection<HubMessage> _allMessages;
        public Collection<HubMessage> Messages
        {
            get { return _allMessages; }
        }

        public int Count
        {
            get { return _allMessages.Count; }
        }

        private int _unreadMessageCount;
        private int _unclearedWarningMessageCount;
        private int _unclearedCriticalMessageCount;

        public int UnreadCount
        {
            get { return _unreadMessageCount; }
        }

        public int UnclearedCriticalMessageCount
        {
            get { return _unclearedCriticalMessageCount; }
        }

        public int UnclearedWarningMessageCount
        {
            get { return _unclearedWarningMessageCount; }
        }

        public int MaxSize
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public SolidColorBrush BackgroundBrush
        {
            get { return GetProperty<SolidColorBrush>(); }
            set { SetProperty(value); }
        }

        public SolidColorBrush ForegroundBrush
        {
            get { return GetProperty<SolidColorBrush>(); }
            set { SetProperty(value); }
        }

        bool _lastLoggedUserIsService;
        public void OnUserLogin(bool newUserIsService)
        {
            OnUserChanged(newUserIsService);
        }

        public void OnUserLogout()
        {
            if (_allMessages.Count > 0)
            {
                if (_lastLoggedUserIsService)
                {
                    _allMessages.Clear();
                }
                else
                {
                    for (var idx = _allMessages.Count - 1; idx >= 0; idx--)
                    {
                        HubMessage msg = _allMessages[idx];
                        if (msg.Type == MessageType.Normal)
                            _allMessages.Remove(msg);
                        else
                        {
                            // The question is whether we mark the surviving
                            // messages on logout or login. When we do it here
                            // a non-zero count may display on the TitleBar
                            // of the MessageHub button even if no user is logged in
                        }
                    }
                }
            }
            OnUserChanged(false);
        }

        private void OnUserChanged(bool newUserIsService)
        {
            UpdateCounts();
            UpdateColors();
            _lastLoggedUserIsService = newUserIsService;
        }

        /// <summary>
        /// This method is made public only for the sake of Automated tests.
        /// </summary>
        /// <param name="msg"></param>
        public void AddMessage(HubMessage msg)
        {
            if (!Messages.Contains(msg))
            {
                _allMessages.Add(msg);
                _unreadMessageCount++;
                if (msg.Type == MessageType.System)
                    _unclearedCriticalMessageCount++;
                else if (msg.Type == MessageType.Warning)
                    _unclearedWarningMessageCount++;
                UpdateColors();
                NotifyPropertyChanged(nameof(UnreadCount));
            }
        }

        private void Add(HubMessage msg)
        {
            DispatcherHelper.ApplicationExecute(() => AddMessage(msg));
        }

        private void OnPublishedSystemMessageDomainMessage(SystemMessageDomain sysMessage)
        {
            if (sysMessage != null)
            {
                Add(new HubMessage(DateTime.Now, sysMessage.MessageType, sysMessage.Message));
            }
        }

        public void UpdateColors()
        {
            SolidColorBrush textColor;
            SolidColorBrush bgColor;

            if (UnclearedCriticalMessageCount > 0)
            {
                textColor = new SolidColorBrush(Colors.White);
                bgColor = new SolidColorBrush(Colors.Red);
            }
            else if (UnclearedWarningMessageCount > 0)
            {
                textColor = new SolidColorBrush(Colors.Black);
                bgColor = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                textColor = new SolidColorBrush(Colors.White);
                bgColor = new SolidColorBrush(Colors.Transparent);
            }

            DispatcherHelper.ApplicationExecute(() => {
                BackgroundBrush = bgColor;
                ForegroundBrush = textColor;
                NotifyPropertyChanged(nameof(MessageHub));
            });
        }

        private void UpdateCounts()
        {
            int unreadMessageCount = 0;
            int warningMessageCount = 0;
            int criticalMessageCount = 0;
            foreach (var msg in _allMessages)
            {
                if (msg.TimesShown == 0)
                {
                    unreadMessageCount++;
                    if (msg.Type == MessageType.System)
                        criticalMessageCount++;
                    else if (msg.Type == MessageType.Warning)
                        warningMessageCount++;
                }
            }

            if (unreadMessageCount != _unreadMessageCount)
            {
                _unreadMessageCount = unreadMessageCount;
                NotifyPropertyChanged(nameof(UnreadCount));
            }

            if (warningMessageCount != _unclearedWarningMessageCount)
            {
                _unclearedWarningMessageCount = warningMessageCount;
                // Currently nobody listens for the change to this property
                //NotifyPropertyChanged(nameof(UnclearedWarningMessageCount));
            }

            if (criticalMessageCount != _unclearedCriticalMessageCount)
            {
                _unclearedCriticalMessageCount = criticalMessageCount;
                // Currently nobody listens for the change to this property
                // NotifyPropertyChanged(nameof(UnclearedCriticalMessageCount));
            }
        }

        public bool HasUninspectedElevatedMessages()
        {
            foreach (HubMessage msg in _allMessages)
                if (msg.TimesShown == 1 && msg.Type != MessageType.Normal)
                    return true;
            return false;
        }

        public void OnCloseMessageHubDialog()
        {
            UpdateCounts();
            UpdateColors();
        }
    }
}
