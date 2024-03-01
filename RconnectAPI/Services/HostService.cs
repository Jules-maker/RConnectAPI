using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RconnectAPI.Models;
using Host = RconnectAPI.Models.Host;

namespace RconnectAPI.Services;

public class HostService
{
    private readonly IMongoCollection<Host> _hostCollection;
    
    private IFindFluent<Host, Host> GetFind(Expression<Func<Host, bool>> filter, string fields = "")
    {
        var find = _hostCollection
            .Find(filter);
        if (fields is { Length: > 0 })
        {
            var fieldArray = fields.Split(',');
            Console.WriteLine(fields);
            ProjectionDefinition<Host> projection = Builders<Host>.Projection.Include("_id");
            projection = fieldArray.Aggregate(projection, (current, field) => current.Include(field));
            find = find
                .Project<Host>(projection);
        }

        return find;
    }

    public HostService(IOptions<MongoDbSettings> mongoDBSettings) {
        MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_URI"));
        IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME"));
        _hostCollection = database.GetCollection<Host>("hosts");
    }

    public async Task<List<Host>> GetAsync(string fields = "",int limit = 10, int page = 1, string searchValue = "") => 
        await GetFind(h => h.Name.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase), fields)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

    public async Task<long> GetCountAsync(string? searchValue = null)
    {
        if (searchValue != null)
        {
            var countMdb = await _hostCollection
                .CountDocumentsAsync(h => h.Name.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase));
            return countMdb;
        }
        return await _hostCollection.CountDocumentsAsync(_ => true);
    }


    public async Task<Host?> GetAsync(string id, string fields = "") =>
        await GetFind(x => x.Id == id, fields).FirstOrDefaultAsync();

    public async Task CreateAsync(Host newHost) =>
        await _hostCollection.InsertOneAsync(newHost);

    public async Task UpdateAsync(string id, Host updatedHost) =>
        await _hostCollection.ReplaceOneAsync(x => x.Id == id, updatedHost);

    public async Task RemoveAsync(string id) =>
        await _hostCollection.DeleteOneAsync(x => x.Id == id);
}