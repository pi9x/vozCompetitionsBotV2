using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace vozCompetitionsLibrary.Model
{
    public class Submission
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string CompetitionHashtag { get; set; }
        public int UserId { get; set; }
        public string UserInfo { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        public int Point { get; set; }
    }
}
