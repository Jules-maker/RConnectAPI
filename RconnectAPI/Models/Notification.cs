﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RconnectAPI.Models
{
    public class Notification
    {
        public Notification(string user, string message, string? meeting = null)
        {
            User = user;
            Message = message;
            Meeting = meeting;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Message { get; set; }
        public bool? Response { get; set; } = null;
        public bool Available { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? OpenedAt { get; set; } = null;
        public string User { get; }
        public string? Meeting { get; } = null;
    }
}

