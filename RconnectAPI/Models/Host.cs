using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RconnectAPI.Models
{
    public class Host
    {
        public Host(string name, string description, string address, string city, string phone, string mainphoto, Boolean isverified, List<string>? openinghours = null)

        {
            Name = name;
            Description = description;
            Address = address;
            City = city;
            Phone = phone;
            Mainphoto = mainphoto;
            Isverified = isverified;
            Openinghours = openinghours;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Mainphoto { get; set; }
        public Boolean Isverified { get; set; }
        public List<string> Openinghours { get; set; } = new List<string>();
    }
}
