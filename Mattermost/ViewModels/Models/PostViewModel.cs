using Mattermost.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Mattermost.ViewModels.Models
{
    class PostViewModel : BaseViewModel
    {
        public PostDisplayType Type
        {
            get { return GetValue<PostDisplayType>(); }
            set { SetValue(value); }
        }
        public User User
        {
            get { return post.User; }
        }
        public DateTime Timestamp
        {
            get
            {
                if (Type == PostDisplayType.Date)
                    return date;
                else
                    return post.Created;
            }
        }
        public string Message
        {
            get { return post.Message; }
        }
        public PostType PostType
        {
            get { return post.Type; }
        }
        public BitmapImage Avatar
        {
            get
            {
                if (GetValue<BitmapImage>() == null)
                    LoadAvatar();

                return GetValue<BitmapImage>();
            }
            set { SetValue(value); }
        }
        public bool IsMouseOver
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        Post post;
        DateTime date;

        public PostViewModel(Post thisPost, Post lastPost, bool newDay, ChannelViewModel channel)
        {
            post = thisPost;

            if (lastPost == null)
                Type = PostDisplayType.MessageNameAndAvatar;
            else
                DetermineType(lastPost.User, lastPost.Created, newDay, channel);
        }

        public PostViewModel(Post thisPost, PostViewModel lastPost, bool newDay, ChannelViewModel channel)
        {
            post = thisPost;

            if (lastPost == null)
                Type = PostDisplayType.MessageNameAndAvatar;
            else
                DetermineType(lastPost.User, lastPost.Timestamp, newDay, channel);
        }

        public PostViewModel(DateTime date)
        {
            Type = PostDisplayType.Date;
            this.date = date;
        }

        void DetermineType(User otherUser, DateTime otherTime, bool newDay, ChannelViewModel channel)
        {
            if (User != otherUser || newDay)
                Type = PostDisplayType.MessageNameAndAvatar;
            else if ((Timestamp - otherTime).TotalHours > 1)
                Type = PostDisplayType.MessageAndName;
            else
                Type = PostDisplayType.MessageOnly;
        }

        async void LoadAvatar()
        {
            Avatar = new BitmapImage(new Uri("pack://application:,,,/Resources/MattermostLogo.png"));

            if (User.ID != "system")
            {
                APIResponse<Bitmap> response = await MattermostAPI.GetAvatar(User.ID);

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

                    Avatar = bitmapImage;
                }
            }
        }
    }

    enum PostDisplayType
    {
        Undetermined,
        Date,
        MessageOnly,
        MessageAndName,
        MessageNameAndAvatar
    }
}
