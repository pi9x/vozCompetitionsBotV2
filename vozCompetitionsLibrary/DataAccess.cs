using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace vozCompetitionsLibrary
{
    public class DataAccess
    {
        private IMongoDatabase db;

        public DataAccess(string database)
        {
            var client = new MongoClient(Configuration.MongoDB());
            db = client.GetDatabase(database);
        }

        public void InsertRecord<T>(string table, T record)
        {
            var collection = db.GetCollection<T>(table);
            collection.InsertOne(record);
        }

        public List<T> GetList<T>(string table)
        {
            var collection = db.GetCollection<T>(table);
            return collection.Find(new BsonDocument()).ToList();
        }

        public void UpdateRecordById<T>(string table, ObjectId id, T record)
        {
            var collection = db.GetCollection<T>(table);
            _ = collection.ReplaceOne(
                new BsonDocument("_id", id),
                record);
        }
    }
}
