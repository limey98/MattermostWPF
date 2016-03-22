using MaterialDesignThemes.Wpf;
using Mattermost.Models;
using Mattermost.Utils;
using Mattermost.Views.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Mattermost.ViewModels.Models
{
    class ChannelViewModel : BaseViewModel
    {
        public string ID
        {
            get { return channel.ID; }
        }
        public string Name
        {
            get { return channel.Name; }
        }
        public string DisplayName
        {
            get { return channel.DisplayName; }
        }
        public string Header
        {
            get { return channel.Header; }
        }
        public bool IsSelected
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }
        public ICommand SendMessageCommand
        {
            get
            {
                if (sendMessage == null)
                    sendMessage = new RelayCommand(SendMessage, () =>
                    {
                        return !string.IsNullOrWhiteSpace(Message);
                    });

                return sendMessage;
            }
        }
        public double ScrollableHeight
        {
            get { return GetValue<double>(); }
            set
            {
                double oldVal = GetValue<double>();

                if (SetValue(value))
                {
                    if (oldVal == ScrollPosition)
                        ScrollPosition = value;
                }
            }
        }
        public double ScrollPosition
        {
            get { return GetValue<double>(); }
            set
            {
                if (SetValue(value) && value < 50 && !loadingMore)
                {
                    loadingMore = true;
                }
            }
        }

        public string Message
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public ObservableCollection<PostViewModel> Posts
        {
            get { return GetValue<ObservableCollection<PostViewModel>>(); }
            private set { SetValue(value); }
        }

        Channel channel;
        RelayCommand sendMessage;
        bool loadingMore;

        public ChannelViewModel(Channel channel)
        {
            this.channel = channel;

            GetPosts();

            ScrollPosition = 10000000;
        }

        public void WebSocketMessage(WebSocketMessage message)
        {
            switch (message.Action)
            {
                case MessageAction.Posted:
                    Post post = JsonConvert.DeserializeObject<Post>(message.Properties.post.ToString());
                    PostViewModel lastPost = Posts.Last();
                    bool datesMatch = lastPost.Timestamp.Date == post.Created.Date;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!datesMatch)
                            Posts.Add(new PostViewModel(post.Created));

                        Posts.Add(new PostViewModel(post, lastPost, !datesMatch, this));
                    });
                    break;
            }
        }

        async void GetPosts()
        {
            APIResponse<List<Post>> response = await MattermostAPI.GetPosts(ID, 0, 30);

            if (response.Success)
            {
                List<PostViewModel> posts = new List<PostViewModel>();
                Post lastPost = null;
                DateTime lastDate = DateTime.MinValue;
                bool newDay = false;

                foreach (Post post in response.Value)
                {
                    newDay = false;

                    if (post.Created.Date != lastDate)
                    {
                        posts.Add(new PostViewModel(post.Created));
                        lastDate = post.Created.Date;
                        newDay = true;
                    }

                    posts.Add(new PostViewModel(post, lastPost, newDay, this));

                    lastPost = post;
                }

                Posts = new ObservableCollection<PostViewModel>(posts);
            }
        }

        async void GetEarlierPosts()
        {

        }

        async void SendMessage()
        {
            APIResponse response = await MattermostAPI.PostMessage(channel, Message);

            /*SimpleMessageDialog view = new SimpleMessageDialog()
            {
                DataContext = "Testing"
            };

            await DialogHost.Show(view, "RootDialog", (object sender, DialogClosingEventArgs args) => { });*/
        }
    }
}
