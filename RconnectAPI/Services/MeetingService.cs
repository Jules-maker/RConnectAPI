using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RconnectAPI.Models;

namespace RconnectAPI.Services;

public class MeetingService
{
    private readonly IMongoCollection<Meeting> _meetingCollection;

    public MeetingService(IOptions<MongoDbSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionUri);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _meetingCollection = database.GetCollection<Meeting>("meetings");
    }

    public async Task<List<Meeting>> GetAsync(int limit = 10, int page = 1) =>
        await _meetingCollection.Find(_ => true).Skip((page - 1) * limit).Limit(limit).ToListAsync();
    
    public async Task<long> GetCountAsync() =>
        await _meetingCollection.CountDocumentsAsync(_ => true);

    public async Task<Meeting?> GetAsync(string id) =>
        await _meetingCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Meeting newUser) =>
        await _meetingCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, Meeting updatedBook) =>
        await _meetingCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _meetingCollection.DeleteOneAsync(x => x.Id == id);
}