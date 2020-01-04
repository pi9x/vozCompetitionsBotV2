using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace vozCompetitionsLibrary.Model
{
    public class Competition
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int UserId { get; set; } // ID of the competition owner
        public long ChatId { get; set; } // ID of the group where the competition is held
        public string Hashtag { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } // closed - opening
    }
}
