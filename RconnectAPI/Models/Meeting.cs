using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RconnectAPI.Models
{
    public class Meeting
    {
        public Meeting(string host, DateTime date, List<string>? users = null, List<string>? billedusers = null)
        {
            Users = users;
            Host = host;
            Date = date;
            Users = users;
            BilledUsers = billedusers;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Host { get; set; }
        public DateTime Date { get; set; }
        public List<string> Users { get; set; } = new List<string>();
        public List<string> BilledUsers { get; set; } = new List<string>();
    }
}

