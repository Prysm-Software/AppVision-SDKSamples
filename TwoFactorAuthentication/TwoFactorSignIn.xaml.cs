using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using Prysm.AppVision.SDK.Plugins;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TwoFactorAuthentication
{
    // ISIgnIn
    public partial class TwoFactorSignIn : UserControl, ISignInPlugin
    {
        public new string Name => nameof(TwoFactor);

        public TwoFactorSignIn()
        {
            InitializeComponent();
        }

        public UIElement GetUI() => this;


        public event EventHandler<NameValue[]> OnLogin;

        private void Signin_Click(object sender, RoutedEventArgs e)
        {
            OnLogin?.Invoke(this, NameValue.Parse($@"
username={user.Text}
password={password.Password}
Authentication={Name}"));
        }

        AppServer _server;
        TaskCompletionSource<bool> _waitCode;

        public void Initialize(IServer server) => _server = (AppServer)server;
        public void CleanUp() { }


        public Task Login(NameValue[] args)
        {
            this.DebugWriteLine();

            // 1er login, le serveur check pwd et envoie un code
            try
            {
                try
                {
                    _server.Login(user.Text, password.Password);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotAcceptable) // 406, demande de création mot de passe
                {
                    SetPassword(user.Text, ex.Message == "Renew");
                }
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized && e.Message == "Continuation needed")
            {
                this.DebugWriteLine("Continuation needed");
            }

            usrPwdForm.Visibility = Visibility.Collapsed;
            codeForm.Visibility = Visibility.Visible;
            code.Text = null;

            _waitCode = new TaskCompletionSource<bool>();
            return _waitCode.Task;
        }


        void SetPassword(string username, bool renew)
        {
            var dlg = new ChangePasswordDialog(username, !renew);

            if (dlg.ShowDialog() != true)
                throw new Exception(RT.Get("Invalid_user_password"));

            _server.ChangePassword(username, dlg.OldPassword, dlg.NewPassword);
            _server.Login(username, dlg.NewPassword);
        }



        private async void Code_Changed(object sender, TextChangedEventArgs e)
        {
            // submit auto dès 4 char
            if (code.Text.Length != 4)
                return;

            this.DebugWriteLine();

            // 2ème login avec le code
            try
            {
                await _server.Login(
                    "username=" + user.Text,
                    "password=" + password.Password,
                    "code=" + code.Text,
                    "Authentication=" + Name
                );

                _waitCode.SetResult(true);
            }
            catch (Exception ex)
            {
                this.DebugWriteLine();
                _waitCode.SetException(ex);
            }
            // retourne à la 1ère étape
            codeForm.Visibility = Visibility.Collapsed;
            usrPwdForm.Visibility = Visibility.Visible;
            password.Password = null;
        }



        public string GetConfig()
        {
            throw new NotImplementedException();
        }

        public void SetConfig(string settings)
        {
        }


    }
}
