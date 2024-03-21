using System.Linq.Expressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RconnectAPI.Models;

namespace RconnectAPI.Services;

public class NotificationService
{
    private readonly IMongoCollection<Notification> _notificationCollection;
    
    private IFindFluent<Notification, Notification> GetFind(Expression<Func<Notification, bool>> filter, string fields = "")
    {
        var find = _notificationCollection
            .Find(filter);
        if (fields is { Length: > 0 })
        {
            var fieldArray = fields.Split(',');
            ProjectionDefinition<Notification> projection = Builders<Notification>.Projection.Include("_id");
            projection = fieldArray.Aggregate(projection, (current, field) => current.Include(field));
            find = find
                .Project<Notification>(projection);
        }

        return find;
    }

    public NotificationService(IOptions<MongoDbSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_URI"));
        IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME"));
        _notificationCollection = database.GetCollection<Notification>("notifications");
    }

    public async Task<List<Notification>> GetAsync(string fields = "", int limit = 10, int page = 1) =>
        await GetFind(_ => true, fields).Skip((page - 1) * limit).Limit(limit).ToListAsync();
    
    public async Task<List<Notification>> GetForUserAsync(string userId, string fields = "", int limit = 10, int page = 1) =>
        await GetFind(n => n.User == userId, fields).Skip((page - 1) * limit).Limit(limit).ToListAsync();
    
    public async Task<Notification> FindWithUserAndMeeting(List<string> notifsToSearch, string userId)
    {
        var builder = Builders<Notification>.Filter;
        foreach (var item in notifsToSearch)
        {
            var filter = builder.In(s => s.Id, notifsToSearch) & builder.Eq(s => s.User, userId);
            var notif = await _notificationCollection.Aggregate()
                .Match(filter).FirstOrDefaultAsync();
            if (notif.User == userId)
            {
                return notif;
            }
        }
        throw new Exception("not found");
    }
    public async Task<Notification> GetOneAsync(string id) =>
        await _notificationCollection.Find(s=>s.Id == id).FirstOrDefaultAsync();
    
    public async Task<long> GetCountAsync(Expression<Func<Notification,bool>>? filter = null)
    {
        FilterDefinition<Notification> filterFunction;
        if (filter != null)
        {
            filterFunction = Builders<Notification>.Filter.Where(filter);
        }
        else
        {
            filterFunction = Builders<Notification>.Filter.Where(_ => true);
        }
        
        return await _notificationCollection.CountDocumentsAsync(filterFunction);
    }

    public async Task<string> CreateAsync(Notification newNotification)
    {
        await _notificationCollection.InsertOneAsync(newNotification);
        return newNotification.Id;
    }

    public async Task UpdateAsync(string id, Notification updatedNotification) =>
        await _notificationCollection.ReplaceOneAsync(x => x.Id == id, updatedNotification);
    public async Task RemoveAsync(string id) =>
        await _notificationCollection.DeleteOneAsync(x => x.Id == id);
}