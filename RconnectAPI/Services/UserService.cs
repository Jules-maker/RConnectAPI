using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RconnectAPI.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Http.HttpResults;


namespace RconnectAPI.Services;

public class UserService
{
    private readonly IMongoCollection<User> _userCollection;
    
    private readonly IConfiguration _configuration;
    
    private IFindFluent<User, User> GetFind(Expression<Func<User, bool>> filter, string fields = "")
    {
        Console.WriteLine(filter);
        var find = _userCollection
            .Find(filter);
        if (fields is { Length: > 0 })
        {
            var fieldArray = fields.Split(',');
            Console.WriteLine(fields);
            ProjectionDefinition<User> projection = Builders<User>.Projection.Include("_id");
            projection = fieldArray.Aggregate(projection, (current, field) => current.Include(field));
            find = find
                .Project<User>(projection);
        }

        return find;
    }
    
    public UserService(IOptions<MongoDbSettings> mongoDBSettings, IConfiguration configuration) {

        MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_URI"));
        IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME"));
        _userCollection = database.GetCollection<User>("users");
        _configuration = configuration;
    }
    
    public async Task<List<User>> GetAsync(string fields = "", int limit = 10, int page = 1) =>
        await GetFind(_ => true, fields).Skip((page - 1) * limit).Limit(limit).ToListAsync();
    
    public async Task<long> GetCountAsync() =>
        await _userCollection.CountDocumentsAsync(_ => true);

    public async Task<User?> GetAsync(string id,string fields = "" ) =>
        await GetFind(x => x.Id == id, fields).FirstOrDefaultAsync();
    public async Task<User?> GetByEmailAsync(string email) =>
        await _userCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
    public async Task<User?> GetByTokenAsync(string token) =>
        await _userCollection.Find(x => x.Token == token).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) =>
        await _userCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _userCollection.DeleteOneAsync(x => x.Id == id);
    
    
    private byte[] GetSigningKey()
    {
        var base64Key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
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
    
    public async Task<string[]> GenerateToken(string email)
    {
        try
        {
            var user = await GetByEmailAsync(email);
            
            string allowed = "ABCDEFGHIJKLMONOPQRSTUVWXYZabcdefghijklmonopqrstuvwxyz0123456789";
            int strlen = 32;
            char[] randomChars = new char[strlen];

            for (int i = 0; i < strlen; i++)
            {
                randomChars[i] = allowed[RandomNumberGenerator.GetInt32(0, allowed.Length)];
            }

            string random = new string(randomChars);

            user.Token = random;
            var now = DateTime.Now;
            // GENERE UN TOKEN VALIDE 2H
            user.TokenTime = new DateTime(now.Year, now.Month, now.Day, now.Hour + 2, now.Minute, now.Second);
            await UpdateAsync(user.Id, user);
            // return le token et l'username de l'utilisateur
            return [random, user.Username];
        }
        catch (Exception ex)
        {
            // Gérer l'exception ou la journaliser
            throw new InvalidOperationException("Une erreur s'est produite lors de la génération du token.", ex);
        }
    }
}
