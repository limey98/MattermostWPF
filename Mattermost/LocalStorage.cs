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

        public static T GetFirst<T>(string collectionName) where T : new()
        {
            if (!db.CollectionExists(collectionName))
                return default(T);

            LiteCollection<T> collection = db.GetCollection<T>(collectionName);

            return collection.FindOne(Query.All());
        }

        public static void Store<T>(string collectionName, T document) where T : new()
        {
            LiteCollection<T> collection = db.GetCollection<T>(collectionName);

            collection.Insert(document);
        }
    }
}
