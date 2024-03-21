using System.Linq.Expressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RconnectAPI.Models;

namespace RconnectAPI.Services;

public class MeetingService
{
    private readonly IMongoCollection<Meeting> _meetingCollection;
    private readonly NotificationService _notificationService;
    private readonly UserService _userService;
    
    public MeetingService(NotificationService notificationService, UserService userService, IOptions<MongoDbSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_URI"));
        IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME"));
        _meetingCollection = database.GetCollection<Meeting>("meetings");
        _notificationService = notificationService;
        _userService = userService;
    }
    
    
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

    public async Task<List<Meeting>> GetAsync(string fields = "", int limit = 10, int page = 1) =>
        await GetFind(_ => true, fields).Skip((page - 1) * limit).Limit(limit).ToListAsync();
    
    public async Task<long> GetCountAsync() =>
        await _meetingCollection.CountDocumentsAsync(_ => true);

    public async Task<Meeting?> GetOneAsync(string id, string fields = "") =>
        await GetFind(x => x.Id == id, fields).FirstOrDefaultAsync();

    public async Task CreateAsync(Meeting newMeeting)
    {
        var CreatedByUser =  await _userService.GetOneAsync(newMeeting.CreatedBy, "Username");
        foreach (var newMeetingUser in newMeeting.Users)
        {
            var newNotif = new Notification(newMeetingUser, "Vous êtes invité par " + CreatedByUser.Username, newMeeting.Id);
            var notifId = await _notificationService.CreateAsync(newNotif);
            newMeeting.Notifications.Add(notifId);
        }
        
        await _meetingCollection.InsertOneAsync(newMeeting);
    }
    
    public async Task<string> AddUser(string meetingId, string userId, string? notification = null)
    {
        var meeting = await _meetingCollection.Find((s) => s.Id == meetingId).FirstOrDefaultAsync();
        var createdBy = await _userService.GetOneAsync(meeting.CreatedBy);
        
        if (meeting == null)
        {
            Console.WriteLine("no meeting");
            throw new Exception("No meeting found");
        }
        
        meeting.Users.Add(userId);
        if (meeting.Users.Count + 1 >= meeting.MaxUsers)
        {
            foreach (var meetingNotification in meeting.Notifications)
            {
                var newNotif = await _notificationService.GetOneAsync(meetingNotification);
                Console.WriteLine("new notif");
                Console.WriteLine(newNotif.Id);
                if (newNotif.Id != notification){
                    newNotif.Available = false;
                }
                else
                {
                    newNotif.Response = true;
                    newNotif.OpenedAt = DateTime.UtcNow;
                }
                await _notificationService.UpdateAsync(meetingNotification, newNotif);
            }

        }
        else
        {
            if (notification != null)
            {
                var newNotif = await _notificationService.GetOneAsync(notification);
                newNotif.Response = true;
                newNotif.OpenedAt = DateTime.UtcNow;
                await _notificationService.UpdateAsync(notification, newNotif);
            }
        }
        var newNotifForCreator = new Notification(createdBy.Id, createdBy.Username + " a répondu a ton invitation !", meetingId);
        await _notificationService.CreateAsync(newNotifForCreator);
        return meeting.Id;
    }
    
    public async Task<Meeting> InviteUser(string meetingId, string userId)
    {
        Console.WriteLine("service - ");
        Console.WriteLine(meetingId);
        Console.WriteLine(userId);
        var meeting = await _meetingCollection.Find(s => s.Id == meetingId).FirstOrDefaultAsync();
        
        if (meeting == null)
        {
            Console.WriteLine("no meeting");
            throw new Exception("No meeting found");
        }

        foreach (var meetingNotification in meeting.Notifications)
        {
            var notif = await _notificationService.GetOneAsync(meetingNotification);
            if (notif.User == userId)
            {
                throw new Exception("already invited");
            }
        }
        var CreatedByUser =  await _userService.GetOneAsync(meeting.CreatedBy, "Username");
        var newNotif = new Notification(userId, "Vous êtes invité par " + CreatedByUser.Username, meeting.Id);
        var newNotifId = await _notificationService.CreateAsync(newNotif);
        meeting.Notifications.Add(newNotifId);
        await UpdateAsync(meetingId, meeting);
        return meeting;
    }
        
    public async Task<Notification> RespondToInviteAsync(bool response, string notifId)
    {
        var notif = await _notificationService.GetOneAsync(notifId);
        if (response)
        {
            if (notif.Meeting == null)
            {
                throw new Exception("No meeting, invalid notification");
            }
            await AddUser(notif.Meeting, notif.User, notifId);
        }
        else
        {
            notif.Response = response;
            notif.OpenedAt = DateTime.UtcNow;
            await _notificationService.UpdateAsync(notifId, notif);
        }

        return notif;
    }

    public async Task UpdateAsync(string id, Meeting updatedMeeting) =>
        await _meetingCollection.ReplaceOneAsync(x => x.Id == id, updatedMeeting);

    public async Task RemoveAsync(string id) =>
        await _meetingCollection.DeleteOneAsync(x => x.Id == id);
}