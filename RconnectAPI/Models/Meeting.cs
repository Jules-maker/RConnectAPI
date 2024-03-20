using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RconnectAPI.Models
{
    public class AddUserData
    {
        public string UserToAdd { get; }
        public string? Notification { get; }

        public AddUserData(string userToAdd, string? notification = null)
        {
            UserToAdd = userToAdd;
            Notification = notification;
        }
    }
    public class Meeting
    {
        public Meeting(string host, string condition, string createdBy, DateTime date, int maxUsers)
        {
            Host = host;
            Date = date;
            MaxUsers = maxUsers;
            Condition = condition;
            CreatedBy = createdBy;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Host { get; set; }
        public DateTime Date { get; set; }
        public string CreatedBy { get; set; }
        public int MaxUsers { get; set; }
        public List<string> Users { get; set; } = new List<string>();
        public List<string> Notifications { get; set; } = new List<string>();
        public string Condition { get; set; }
    }
}

