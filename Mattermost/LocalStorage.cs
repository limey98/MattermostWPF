using LiteDB;
using Mattermost.Models;
using System;
using System.IO;

namespace Mattermost
{
    static class LocalStorage
    {
        static LiteDatabase db;

        public static void Initialise()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Aeries", "Mattermost");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            db = new LiteDatabase(Path.Combine(path, "Database.db"));
        }

        public static void Close()
        {
            db.Dispose();
        }

        public static LocalConfig GetConfig()
        {
            if (!db.CollectionExists("configs"))
                return null;

            LiteCollection<LocalConfig> configs = db.GetCollection<LocalConfig>("configs");

            return configs.FindOne(Query.All());
        }

        public static void StoreConfig(LocalConfig config)
        {
            LiteCollection<LocalConfig> configs = db.GetCollection<LocalConfig>("configs");

            configs.Insert(config);
        }
    }
}
