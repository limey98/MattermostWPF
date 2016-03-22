using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Mattermost.Models
{
    class User
    {
        static List<User> userList = new List<User>();
        static User system;
        
        public static User System
        {
            get
            {
                if (system == null)
                    system = new User() { ID = "system", FirstName = "System", LastName = "", Nickname = "System", Username = "System" };

                return system;
            }
        }

        public static User GetUserByID(string id)
        {
            return userList.FirstOrDefault(u => u.ID == id);
        }

        public string ID { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("theme_props")]
        public Dictionary<string, string> ThemeProperties { get; set; }

        public string DisplayName
        {
            get
            {
                string nameFormat = Preferences.Instance.GetPreference("display_settings", "name_format");
                string displayName = "";

                switch (nameFormat)
                {
                    case "full_name":
                        displayName = string.Format("{0} {1}", FirstName, LastName).Trim();
                        break;
                    case "nickname_full_name":
                        if (!string.IsNullOrWhiteSpace(Nickname))
                            displayName = Nickname;
                        else
                            displayName = string.Format("{0} {1}", FirstName, LastName).Trim();

                        break;
                }

                if (displayName == "")
                    return Username;
                else
                    return displayName;
            }
        }

        public User()
        {
            userList.Add(this);
        }
    }
}
