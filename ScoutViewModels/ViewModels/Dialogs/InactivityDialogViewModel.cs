using System.Windows.Forms;
using ScoutLanguageResources;
using ScoutUtilities.CustomEventArgs;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class InactivityDialogViewModel : BaseDialogViewModel
    {
        public InactivityDialogViewModel(InactivityEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            DialogTitle = LanguageResourceHelper.Get("LID_Label_InActivityTimeout");
            CountDown = 30;
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        protected override void DisposeManaged()
        {
            _timer?.Stop();
            _timer?.Dispose();
            base.DisposeManaged();
        }

        private void OnTimerTick(object sender, System.EventArgs e)
        {
            if (CountDown > 0)
            {
                CountDown--;
                return;
            }

            Close(false);
        }

        private Timer _timer;

        public int CountDown
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        protected override void OnAccept()
        {
            Close(true);
        }

        protected override void OnCancel()
        {
            Close(true);
        }
    }
}