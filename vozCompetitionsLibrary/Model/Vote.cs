using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace vozCompetitionsLibrary.Model
{
    public class Vote
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
    }
}
