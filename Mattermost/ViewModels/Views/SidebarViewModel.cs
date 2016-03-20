using Mattermost.Models;
using Mattermost.ViewModels.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mattermost.ViewModels.Views
{
    class SidebarViewModel : BaseViewModel
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
                    messageVM.ActiveChannel = value;
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
                    messageVM.ActiveChannel = value;
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
                    messageVM.ActiveChannel = value;
                    SelectedPublicChannel = null;
                    SelectedPrivateGroup = null;
                }
            }
        }

        MessageViewModel messageVM;

        public SidebarViewModel(MessageViewModel messageVM, List<User> users, List<Channel> channels)
        {
            this.messageVM = messageVM;

            foreach (Channel channel in channels.Where(c => c.Type == ChannelType.Direct))
            {
                User user = users.FirstOrDefault(u => channel.Name.StartsWith(u.ID));

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
        }
    }
}
