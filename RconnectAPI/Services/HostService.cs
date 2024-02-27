using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RconnectAPI.Models;
using Host = RconnectAPI.Models.Host;

namespace RconnectAPI.Services;

public class HostService
{
    private readonly IMongoCollection<Host> _hostCollection;

    public HostService(IOptions<MongoDbSettings> mongoDBSettings) {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionUri);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _hostCollection = database.GetCollection<Host>("hosts");
    }

    public async Task<List<Host>> GetAsync(int limit = 10, int page = 1) =>
        await _hostCollection.Find(_ => true).Skip((page - 1) * limit).Limit(limit).ToListAsync();

    public async Task<Host?> GetAsync(string id) =>
        await _hostCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Host newHost) =>
        await _hostCollection.InsertOneAsync(newHost);

    public async Task UpdateAsync(string id, Host updatedHost) =>
        await _hostCollection.ReplaceOneAsync(x => x.Id == id, updatedHost);

    public async Task RemoveAsync(string id) =>
        await _hostCollection.DeleteOneAsync(x => x.Id == id);
}