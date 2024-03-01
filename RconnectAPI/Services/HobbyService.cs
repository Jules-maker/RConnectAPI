using System.Linq.Expressions;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RconnectAPI.Models;

namespace RconnectAPI.Services;

public class HobbyService
{
    private readonly IMongoCollection<Hobby> _hobbyCollection;

    private IFindFluent<Hobby, Hobby> GetFind(Expression<Func<Hobby, bool>> filter, string fields = "")
    {
        Console.WriteLine(filter);
        var find = _hobbyCollection
            .Find(filter);
        if (fields is { Length: > 0 })
        {
            var fieldArray = fields.Split(',');
            Console.WriteLine(fields);
            ProjectionDefinition<Hobby> projection = Builders<Hobby>.Projection.Include("_id");
            projection = fieldArray.Aggregate(projection, (current, field) => current.Include(field));
            find = find
                .Project<Hobby>(projection);
        }

        return find;
    }

    public HobbyService(IOptions<MongoDbSettings> mongoDBSettings) {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionUri);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _hobbyCollection = database.GetCollection<Hobby>("hobbies");
    }

    public async Task<List<Hobby>> GetAsync(string fields = "", int limit = 10, int page = 1)
    {
        return await GetFind(_ => true, fields)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<long> GetCountAsync() =>
        await _hobbyCollection.CountDocumentsAsync(_ => true);

    public async Task<Hobby?> GetAsync(string id, string fields = "")
    {
        return await GetFind(x => x.Id == id, fields)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Hobby newHobby) =>
        await _hobbyCollection.InsertOneAsync(newHobby);

    public async Task UpdateAsync(string id, Hobby updatedHobby) =>
        await _hobbyCollection.ReplaceOneAsync(x => x.Id == id, updatedHobby);

    public async Task RemoveAsync(string id) =>
        await _hobbyCollection.DeleteOneAsync(x => x.Id == id);
}