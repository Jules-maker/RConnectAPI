using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RconnectAPI.Models
{
    public class Hobby
    {
        public Hobby(string name, string description)
        {
            Name = name;
            Description = description;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
