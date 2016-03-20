using Mattermost.ViewModels.Models;
using Mattermost.ViewModels.Views;
using System.Windows;
using System.Windows.Controls;

namespace Mattermost.Utils
{
    class MessageTypeSelector : DataTemplateSelector
    {
        public DataTemplate Progress { get; set; }
        public DataTemplate ServerURLError { get; set; }
        public DataTemplate BoundTextError { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            MessageType messageType = (MessageType)item;

            switch (messageType)
            {
                case MessageType.Progress:
                    return Progress;
                case MessageType.ServerURLError:
                    return ServerURLError;
                case MessageType.BoundTextError:
                    return BoundTextError;
            }

            return null;
        }
    }

    class PostTypeSelector : DataTemplateSelector
    {
        public DataTemplate Date { get; set; }
        public DataTemplate MessageOnly { get; set; }
        public DataTemplate MessageAndName { get; set; }
        public DataTemplate MessageNameAndAvatar { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            PostViewModel post = item as PostViewModel;

            switch (post?.Type)
            {
                case PostDisplayType.Date:
                    return Date;
                case PostDisplayType.MessageOnly:
                    return MessageOnly;
                case PostDisplayType.MessageAndName:
                    return MessageAndName;
                case PostDisplayType.MessageNameAndAvatar:
                    return MessageNameAndAvatar;
            }

            return null;
        }
    }
}
