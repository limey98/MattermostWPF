using System.Collections.Generic;

namespace Mattermost
{
    class Preferences
    {
        public static Preferences Instance
        {
            get
            {
                if (instance == null)
                    instance = new Preferences();

                return instance;
            }
        }

        static Preferences instance;

        Dictionary<string, Dictionary<string, string>> preferences = new Dictionary<string,Dictionary<string,string>>();

        public Preferences()
        {
            instance = this;
        }

        public void SetPreference(string category, string key, string value)
        {
            if (!preferences.ContainsKey(category))
                preferences.Add(category, new Dictionary<string, string>());

            if (!preferences[category].ContainsKey(key))
                preferences[category].Add(key, value);
            else
                preferences[category][key] = value;
        }

        public string GetPreference(string category, string key)
        {
            if (!preferences.ContainsKey(category))
                return "";

            if (!preferences[category].ContainsKey(key))
                return "";

            return preferences[category][key];
        }
    }
}
