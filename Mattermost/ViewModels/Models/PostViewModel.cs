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
        public int ReplyCount
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        [NotifiesOn("IsMouseOver", "ReplyCount")]
        public bool ShowReply
        {
            get { return ReplyCount > 0 || IsMouseOver; }
        }

        Post post;
        DateTime date;

        public PostViewModel(Post thisPost)
        {
            post = thisPost;
            Type = PostDisplayType.MessageNameAndAvatar;
        }

        public PostViewModel (Post thisPost, User lastPostUser, DateTime lastPostTime, bool newDay)
        {
            post = thisPost;

            if (User != lastPostUser || newDay)
                Type = PostDisplayType.MessageNameAndAvatar;
            else if ((Timestamp - lastPostTime).TotalHours > 1)
                Type = PostDisplayType.MessageAndName;
            else
                Type = PostDisplayType.MessageOnly;
        }

        public PostViewModel(DateTime date)
        {
            Type = PostDisplayType.Date;
            this.date = date;
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
