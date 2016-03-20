using Mattermost.Models;
using Mattermost.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mattermost.ViewModels.Views
{
    class LoginViewModel : BaseViewModel
    {
        public bool ShowMessage
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }
        public MessageType MessageType
        {
            get { return GetValue<MessageType>(); }
            set { SetValue(value); }
        }
        public string ErrorMessage
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
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
                MessageType = MessageType.ServerURLError;
                ShowMessage = true;
                return;
            }

            MessageType = MessageType.Progress;
            ShowMessage = true;

            APIResponse result = await MattermostAPI.Login(Server, Team, Username, password.SecurePassword);

            if (!result.Success)
            {
                MessageType = MessageType.BoundTextError;
                ErrorMessage = result.Error;
                return;
            }

            LocalStorage.StoreConfig(new LocalConfig
            {
                Server = Server,
                Team = Team,
                Username = Username,
                UserID = MattermostAPI.MyID,
                Token = MattermostAPI.Token
            });

            APIResponse<List<User>> users = await MattermostAPI.GetUsers();

            if (!users.Success)
            {
                MessageType = MessageType.BoundTextError;
                ErrorMessage = users.Error;
                return;
            }

            Task<APIResponse<Preferences>> preferencesTask = MattermostAPI.GetPreferences();
            APIResponse<List<Channel>> channels = await MattermostAPI.GetChannels();

            if (!channels.Success)
            {
                MessageType = MessageType.BoundTextError;
                ErrorMessage = channels.Error;
                return;
            }

            await preferencesTask;

            mainVM.CurrentView = new MessageViewModel(mainVM, users.Value, channels.Value);
            ShowMessage = false;

        }
    }

    enum MessageType
    {
        Progress,
        ServerURLError,
        BoundTextError
    }
}
