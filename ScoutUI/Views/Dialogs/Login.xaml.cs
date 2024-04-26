using ScoutViewModels.ViewModels.Dialogs;
using System;

namespace ScoutUI.Views.Dialogs
{
    public partial class Login : Dialog
    {
        public Login(LoginViewModel vm) : base(vm)
        {
            InitializeComponent();
            ContentRendered += OnContentRendered;
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            if ((DataContext is LoginViewModel loginViewModel) && string.IsNullOrWhiteSpace(loginViewModel.DisplayedUsername))
            {
                TxtUserId.Focus();
            }
            else
            {
                PwdLoginPassword.Focus();
            }
        }
    }
}
