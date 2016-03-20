using Mattermost.Models;
using Mattermost.ViewModels.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocket4Net;

namespace Mattermost.ViewModels.Views
{
    class MessageViewModel : BaseViewModel
    {
        public SidebarViewModel SidebarVM { get; private set; }
        public ChannelViewModel ActiveChannel
        {
            get { return GetValue<ChannelViewModel>(); }
            set { SetValue(value); }
        }

        WebSocket websocket;

        public MessageViewModel(MainWindowViewModel mainVM, List<User> users, List<Channel> channels)
        {
            SidebarVM = new SidebarViewModel(this, users, channels);

            websocket = new WebSocket(MattermostAPI.APIBaseURL.ToString().Replace("http", "ws") + "websocket", customHeaderItems: new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Authorization", "Bearer " + MattermostAPI.Token) });

            websocket.Opened += WebSocketOpen;
            websocket.Error += WebSocketError;
            websocket.Closed += WebSocketClosed;
            websocket.MessageReceived += WebSocketMessage;

            websocket.Open();
        }

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

        void WebSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        void WebSocketClosed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
