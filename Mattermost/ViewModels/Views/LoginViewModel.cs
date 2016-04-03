using MaterialDesignThemes.Wpf;
using Mattermost.Models;
using Mattermost.Utils;
using Mattermost.Views.Dialogs;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mattermost.ViewModels.Views
{
    class LoginViewModel : BaseViewModel
    {
        public string Server
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Team
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Username
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public ICommand LoginCommand
        {
            get
            {
                if (login == null)
                    login = new RelayCommand<PasswordBox>(Login, p => !string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Team) && !string.IsNullOrWhiteSpace(Username) && p.SecurePassword.Length != 0);

                return login;
            }
        }

        RelayCommand<PasswordBox> login;
        MainWindowViewModel mainVM;

        public LoginViewModel(MainWindowViewModel mainVM)
        {
            this.mainVM = mainVM;
        }

        async void Login(PasswordBox password)
        {
            if (!Regex.IsMatch(Server, @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)"))
            {
                await ShowMessageDialog("Server URL is invalid", false);
                return;
            }

            DialogHost.Show(new ProgressDialog(), "RootDialog");

            APIResponse result = await MattermostAPI.Login(Server, Team, Username, password.SecurePassword);

            if (!result.Success)
            {
                await ShowMessageDialog(result.Error);
                return;
            }

            LocalStorage.Store("configs", new LocalConfig
            {
                Server = Server,
                Team = Team,
                Username = Username,
                UserID = MattermostAPI.MyID,
                Token = MattermostAPI.Token,
                TeamID = MattermostAPI.Team.ID,
            });

            LocalStorage.Store("teams", MattermostAPI.Team);

            APIResponse<List<User>> users = await MattermostAPI.GetUsers();

            if (!users.Success)
            {
                await ShowMessageDialog(users.Error);
                return;
            }

            Task<APIResponse<Preferences>> preferencesTask = MattermostAPI.GetPreferences();
            APIResponse<List<Channel>> channels = await MattermostAPI.GetChannels();

            if (!channels.Success)
            {
                await ShowMessageDialog(channels.Error);
                return;
            }

            await preferencesTask;

            mainVM.CurrentView = new MessageViewModel(mainVM, users.Value, channels.Value);

            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        async Task ShowMessageDialog(string message, bool closeFirst = true)
        {
            if (closeFirst)
                DialogHost.CloseDialogCommand.Execute(true, null);

            SimpleMessageDialog view = new SimpleMessageDialog()
            {
                DataContext = message
            };

            await DialogHost.Show(view, "RootDialog");
        }
    }
}
