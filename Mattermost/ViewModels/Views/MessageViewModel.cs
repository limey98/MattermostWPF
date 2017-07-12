using Mattermost.Models;
using Mattermost.ViewModels.Models;
using Newtonsoft.Json;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using WebSocket4Net;

namespace Mattermost.ViewModels.Views
{
    class MessageViewModel : BaseViewModel
    {
        public ObservableCollection<ChannelViewModel> PublicChannels
        {
            get { return GetValue<ObservableCollection<ChannelViewModel>>(); }
            set { SetValue(value); }
        }
        public ChannelViewModel SelectedPublicChannel
        {
            get { return GetValue<ChannelViewModel>(); }
            set
            {
                if (SetValue(value) && value != null)
                {
                    ActiveChannel = value;
                    SelectedPrivateGroup = null;
                    SelectedDirectMessage = null;
                }
            }
        }
        public ObservableCollection<ChannelViewModel> PrivateGroups
        {
            get { return GetValue<ObservableCollection<ChannelViewModel>>(); }
            set { SetValue(value); }
        }
        public ChannelViewModel SelectedPrivateGroup
        {
            get { return GetValue<ChannelViewModel>(); }
            set
            {
                if (SetValue(value) && value != null)
                {
                    ActiveChannel = value;
                    SelectedPublicChannel = null;
                    SelectedDirectMessage = null;
                }
            }
        }
        public ObservableCollection<ChannelViewModel> DirectMessages
        {
            get { return GetValue<ObservableCollection<ChannelViewModel>>(); }
            set { SetValue(value); }
        }
        public ChannelViewModel SelectedDirectMessage
        {
            get { return GetValue<ChannelViewModel>(); }
            set
            {
                if (SetValue(value) && value != null)
                {
                    ActiveChannel = value;
                    SelectedPublicChannel = null;
                    SelectedPrivateGroup = null;
                }
            }
        }
        public ChannelViewModel ActiveChannel
        {
            get { return GetValue<ChannelViewModel>(); }
            set { SetValue(value); }
        }
        public BitmapImage MyAvatar
        {
            get
            {
                if (GetValue<BitmapImage>() == null)
                    LoadAvatar();

                return GetValue<BitmapImage>();
            }
            set { SetValue(value); }
        }
        public string Username
        {
            get { return string.Format("@{0}", MattermostAPI.Me.Username); }
        }
        public string TeamName
        {
            get { return MattermostAPI.Team.DisplayName; }
        }

        WebSocket websocket;

        public MessageViewModel(MainWindowViewModel mainVM, List<User> users, List<Channel> channels)
        {
            foreach (Channel channel in channels.Where(c => c.Type == ChannelType.Direct))
            {
                string otherID = channel.Name.Substring(0, 26);

                if (otherID == MattermostAPI.MyID)
                    otherID = channel.Name.Substring(channel.Name.Length - 26);

                User user = users.FirstOrDefault(u => otherID == u.ID);

                if (user != null)
                    channel.DisplayName = user.DisplayName;
                else
                    channel.DisplayName = "Unknown User";
            }

            PublicChannels = new ObservableCollection<ChannelViewModel>();
            PrivateGroups = new ObservableCollection<ChannelViewModel>();
            DirectMessages = new ObservableCollection<ChannelViewModel>();

            string activeChannel = Preferences.Instance.GetPreference("last", "channel");

            foreach (Channel channel in channels)
            {
                ChannelViewModel vm = new ChannelViewModel(channel);

                switch (channel.Type)
                {
                    case ChannelType.Public:
                        PublicChannels.Add(vm);

                        if (channel.ID == activeChannel)
                            SelectedPublicChannel = vm;
                        break;
                    case ChannelType.Private:
                        PrivateGroups.Add(vm);

                        if (channel.ID == activeChannel)
                            SelectedPrivateGroup = vm;
                        break;
                    case ChannelType.Direct:
                        DirectMessages.Add(vm);

                        if (channel.ID == activeChannel)
                            SelectedDirectMessage = vm;
                        break;
                }
            }

            if (activeChannel == "")
                SelectedPublicChannel = PublicChannels.First(c => c.Name == "town-square");

            websocket = new WebSocket(MattermostAPI.APIBaseURL.ToString().Replace("http","ws")+"users/"+ "websocket", customHeaderItems: new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Authorization", "Bearer " + MattermostAPI.Token) });

            websocket.Opened += WebSocketOpen;
            websocket.Error += WebSocketError;
            websocket.Closed += WebSocketClosed;
            websocket.MessageReceived += WebSocketMessage;

            websocket.Open();
        }

        ChannelViewModel GetChannelByID(string id)
        {
            ChannelViewModel channel = PublicChannels.FirstOrDefault(c => c.ID == id);

            if (channel != null)
                return channel;

            channel = PrivateGroups.FirstOrDefault(c => c.ID == id);

            if (channel != null)
                return channel;

            return DirectMessages.FirstOrDefault(c => c.ID == id);
        }

        async void LoadAvatar()
        {
            MyAvatar = new BitmapImage(new Uri("pack://application:,,,/Resources/MattermostLogo.png"));
            
            APIResponse<Bitmap> response = await MattermostAPI.GetAvatar(MattermostAPI.Me.ID);

            if (!response.Success)
                Console.WriteLine(response.Error);

            using (MemoryStream memory = new MemoryStream())
            {
                response.Value.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                MyAvatar = bitmapImage;
            }
        }

        #region WebSocket stuff
        void WebSocketMessage(object sender, MessageReceivedEventArgs e)
        {
            WebSocketMessage message = JsonConvert.DeserializeObject<WebSocketMessage>(e.Message);

            switch (message.Action)
            {
                case MessageAction.PreferenceChanged:
                    break;
                default:
                    ChannelViewModel channel = GetChannelByID(message.ChannelID);

                    if (channel != null)
                        channel.WebSocketMessage(message);
                    break;
            }
        }

        void WebSocketOpen(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket opened");
        }

        void WebSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
        }

        void WebSocketClosed(object sender, EventArgs e)
        {
        }
        #endregion
    }
}
