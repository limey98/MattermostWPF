using Mattermost.Models;
using System.Collections.Generic;
using System.Net;

namespace Mattermost.ViewModels.Views
{
    class MainWindowViewModel : BaseViewModel
    {
        public BaseViewModel CurrentView
        {
            get { return GetValue<BaseViewModel>(); }
            set { SetValue(value); }
        }

        public MainWindowViewModel()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            LocalStorage.Initialise();

            LocalConfig config = LocalStorage.GetConfig();

            if (config == null)
                CurrentView = new LoginViewModel(this);

            CheckConfig(config);
        }

        ~MainWindowViewModel()
        {
            LocalStorage.Close();
        }

        async void CheckConfig(LocalConfig config)
        {
            APIResponse response = await MattermostAPI.CheckSession(config);
            
            APIResponse<List<User>> users = await MattermostAPI.GetUsers();
            APIResponse<List<Channel>> channels = await MattermostAPI.GetChannels();
            APIResponse<Preferences> preferences = await MattermostAPI.GetPreferences();

            CurrentView = new MessageViewModel(this, users.Value, channels.Value);
        }
    }
}
