using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;

namespace RconnectAPI.Controllers;

[Controller]
[Route("api/[controller]")]
public class NotificationController : Controller
{
    private readonly NotificationService _notificationService;
    private readonly MeetingService _meetingService;

    public NotificationController(NotificationService notificationService, MeetingService meetingService)
    {
        _notificationService = notificationService;
        _meetingService = meetingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(string fields = "", int limit = 10, int page = 1)
    {
        var data = await _notificationService.GetAsync(fields, limit, page);
        var count = await _notificationService.GetCountAsync();
        return Ok(new ListResponseData<Notification>(data, count));
    }
    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetOne(string id)
    {
        var notification = await _notificationService.GetOneAsync(id);

        if (notification is null)
        {
            return StatusCode(404);
        }

        return Ok(new ResponseData<Notification>(notification));
    }
    
    [HttpGet("user/{userId:length(24)}")]
    public async Task<IActionResult> GetForUser(string userId, int limit = 10, int page = 1)
    {
        try
        {
            var notifications = await _notificationService.GetForUserAsync(userId, "", limit, page);
            var count = await _notificationService.GetCountAsync(n => n.User == userId);

            return Ok(new ListResponseData<Notification>(notifications, count));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Notification newNotification)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _notificationService.CreateAsync(newNotification);

            return Ok(new ResponseData<Notification>(newNotification));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] Notification updatedNotification)
    {
        try
        {
            var notification = await _notificationService.GetOneAsync(id);

            if (notification is null)
            {
                return StatusCode(404);
            }

            updatedNotification.Id = notification.Id;

            await _notificationService.UpdateAsync(id, updatedNotification);

            return Ok(new ResponseData<Notification>(updatedNotification));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _notificationService.RemoveAsync(id);

            return Ok(new ResponseData<string>(id + " deleted"));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public class NotifResponse
    {
        public NotifResponse(bool response, string notifId)
        {
            Response = response;
            NotifId = notifId;
        }

        public bool Response { get; }
        public string NotifId { get; }
    }
    
    [HttpPost("respond")]
    public async Task<IActionResult> Respond([FromBody] NotifResponse data)
    {
        try
        {
            var notification = await _meetingService.RespondToInviteAsync(data.Response, data.NotifId);

            if (notification is null)
            {
                return StatusCode(404);
            }
            
            return Ok(new ResponseData<Notification>(notification));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

}
