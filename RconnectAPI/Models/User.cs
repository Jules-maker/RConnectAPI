using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RconnectAPI.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        public User(string username, string password, string email, string firstname, string lastname, DateTime birthdate, List<string>? hobbies = null, List<string>? favouritehosts = null, List<string>? rating = null, List<string>? missedmeetings = null)
        {
            Username = username;
            Password = password;
            Email = email;
            Firstname = firstname;
            Lastname = lastname;
            Birthdate = birthdate;
            Hobbies = hobbies;
            Favouritehosts = favouritehosts;
            Rating = rating;
            Missedmeetings = missedmeetings;
        }


        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime Birthdate { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenTime { get; set; }
        public List<string>? Hobbies { get; set; } = new List<string>();
        public List<string>? Favouritehosts { get; set; } = new List<string>();
        public List<string>? Rating { get; set; } = new List<string>();
        public List<string>? Missedmeetings { get; set; } = new List<string>();
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }


    }
    public class UserRegisterData
    {
        public UserRegisterData(string username, string password, string email, string firstname, string lastname, DateTime birthdate)
        {
            Username = username;
            Password = password;
            Email = email;
            Firstname = firstname;
            Lastname = lastname;
            Birthdate = birthdate;
        }
        
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime Birthdate { get; set; }
        public List<string>? Hobbies { get; set; } = new List<string>();
        public List<string>? Favouritehosts { get; set; } = new List<string>();
        public List<string>? Rating { get; set; } = new List<string>();
        public List<string>? Missedmeetings { get; set; } = new List<string>();

    }
    public class UserLoginData
    {
        public UserLoginData(string password, string email)
        {
            Password = password;
            Email = email;
        }
        public string Password { get; set; }
        public string Email { get; set; }

    }
    public class UserProfileData(List<Host> hosts, List<Hobby> hobbies)
    {
        public List<Host> Hosts { get; set; } = hosts;
        public List<Hobby> Hobbies { get; set; } = hobbies;
    }
}
