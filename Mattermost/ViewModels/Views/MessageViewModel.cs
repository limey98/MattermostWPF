using Mattermost.Models;
using Mattermost.ViewModels.Models;
using Newtonsoft.Json;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            PublicChannels = new ObservableCollection<ChannelViewModel>(channels.Where(c => c.Type == ChannelType.Public).Select(c => new ChannelViewModel(c)));
            PrivateGroups = new ObservableCollection<ChannelViewModel>(channels.Where(c => c.Type == ChannelType.Private).Select(c => new ChannelViewModel(c)));
            DirectMessages = new ObservableCollection<ChannelViewModel>(channels.Where(c => c.Type == ChannelType.Direct).Select(c => new ChannelViewModel(c)));

            string activeChannel = Preferences.Instance.GetPreference("last", "channel");

            if (activeChannel != "")
            {
                ChannelViewModel active = PublicChannels.FirstOrDefault(c => c.ID == activeChannel);

                if (active != null)
                {
                    SelectedPublicChannel = active;
                    return;
                }

                active = PrivateGroups.FirstOrDefault(c => c.ID == activeChannel);

                if (active != null)
                {
                    SelectedPrivateGroup = active;
                    return;
                }

                active = DirectMessages.FirstOrDefault(c => c.ID == activeChannel);

                if (active != null)
                {
                    SelectedDirectMessage = active;
                    return;
                }
            }
            else
                SelectedPublicChannel = PublicChannels.First(c => c.Name == "town-square");

            websocket = new WebSocket(MattermostAPI.APIBaseURL.ToString().Replace("http", "ws") + "websocket", customHeaderItems: new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Authorization", "Bearer " + MattermostAPI.Token) });

            websocket.Opened += WebSocketOpen;
            websocket.Error += WebSocketError;
            websocket.Closed += WebSocketClosed;
            websocket.MessageReceived += WebSocketMessage;

            websocket.Open();
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
                    ActiveChannel.WebSocketMessage(message);
                    break;
            }
        }

        void WebSocketOpen(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket opened");
        }

        void WebSocketError(object sender, ErrorEventArgs e)
        {
        }

        void WebSocketClosed(object sender, EventArgs e)
        {
        }
        #endregion
    }
}
