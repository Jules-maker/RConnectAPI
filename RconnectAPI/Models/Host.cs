using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RconnectAPI.Models
{
    public class LatLng(double lat, double lng)
    {
        public double Lat { get; set; } = lat;
        public double Lng { get; set; } = lng;
    }
    public class Host
    {
        public Host(string name, string description, string address, string city, string phone, string mainphoto, Boolean isverified, LatLng latLng, List<string>? openinghours = null)

        {
            Name = name;
            Description = description;
            Address = address;
            City = city;
            Phone = phone;
            Mainphoto = mainphoto;
            Isverified = isverified;
            Openinghours = openinghours;
            LatLng = latLng;
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
        public LatLng LatLng { get; set; }
        public DateTime Createdat { get; set; }
    }
}
