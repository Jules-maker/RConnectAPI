using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RconnectAPI.Models;
using BCrypt.Net;


namespace RconnectAPI.Services;

public class UserService
{
    private readonly IMongoCollection<User> _userCollection;
    
    private readonly IConfiguration _configuration;
    
    private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
    
    public UserService(IOptions<MongoDbSettings> mongoDBSettings, IConfiguration configuration) {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionUri);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<User>("users");
        _configuration = configuration;
    }
    
    public async Task<List<User>> GetAsync(int limit = 10, int page = 1) =>
        await _userCollection.Find(_ => true).Skip((page - 1) * limit).Limit(limit).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    public async Task<User?> GetByEmailAsync(string email) =>
        await _userCollection.Find(x => x.Email == email).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) =>
        await _userCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _userCollection.DeleteOneAsync(x => x.Id == id);
    
    
    private byte[] GetSigningKey()
    { 
        var base64Key = _configuration["Jwt:SecretKey"];
        return Convert.FromBase64String(base64Key);
    }

    
    public async Task<User> RegisterAsync(string username, string email, string password, DateTime dob, string firstname, string lastname)
    {
        var newUser = new User(username, BCrypt.Net.BCrypt.HashPassword(password), email, firstname, lastname, dob, new List<string>(), new List<string>(), new List<string>(), new List<string>());

        await CreateAsync(newUser);
        return newUser;
    }

    public async Task<User?> Login(string email, string providedPassword)
    {
        var user = await GetByEmailAsync(email);
        if (user == null) return null;
        var result = BCrypt.Net.BCrypt.Verify(providedPassword, user.Password);
        return result ? user : null;
    }

    public string GenerateJwt(User user)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSigningKey();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email)
                    }
                ),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            // Gérer l'exception ou la journaliser
            throw new InvalidOperationException("Une erreur s'est produite lors de la génération du token JWT.", ex);
        }
    }
}
