using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RconnectAPI.Models;

namespace RconnectAPI.Services;

public class MeetingService
{
    private readonly IMongoCollection<Meeting> _meetingCollection;
    
    private IFindFluent<Meeting, Meeting> GetFind(Expression<Func<Meeting, bool>> filter, string fields = "")
    {
        Console.WriteLine(filter);
        var find = _meetingCollection
            .Find(filter);
        if (fields is { Length: > 0 })
        {
            var fieldArray = fields.Split(',');
            Console.WriteLine(fields);
            ProjectionDefinition<Meeting> projection = Builders<Meeting>.Projection.Include("_id");
            projection = fieldArray.Aggregate(projection, (current, field) => current.Include(field));
            find = find
                .Project<Meeting>(projection);
        }

        return find;
    }

    public MeetingService(IOptions<MongoDbSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_URI"));
        IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME"));
        _meetingCollection = database.GetCollection<Meeting>("meetings");
    }

    public async Task<List<Meeting>> GetAsync(string fields = "", int limit = 10, int page = 1) =>
        await GetFind(_ => true, fields).Skip((page - 1) * limit).Limit(limit).ToListAsync();
    
    public async Task<long> GetCountAsync() =>
        await _meetingCollection.CountDocumentsAsync(_ => true);

    public async Task<Meeting?> GetAsync(string id, string fields = "") =>
        await GetFind(x => x.Id == id, fields).FirstOrDefaultAsync();

    public async Task CreateAsync(Meeting newUser) =>
        await _meetingCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, Meeting updatedBook) =>
        await _meetingCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _meetingCollection.DeleteOneAsync(x => x.Id == id);
}