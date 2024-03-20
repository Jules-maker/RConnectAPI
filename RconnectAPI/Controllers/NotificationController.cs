using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;

namespace RconnectAPI.Controllers;

[Controller]
[Route("api/[controller]")]
public class NotificationController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ResponseData<Notification>> Get(string fields = "", int limit = 10, int page = 1)
    {
        var data = await _notificationService.GetAsync(fields, limit, page);
        var count = await _notificationService.GetCountAsync();
        return new ResponseData<Notification>(data, count);
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Notification>> Get(string id, string fields = "")
    {
        var notification = await _notificationService.GetAsync(id, fields);

        if (notification is null)
        {
            return NotFound();
        }

        return notification;
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

            return CreatedAtAction(nameof(Get), newNotification);
        }
        catch
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the notification.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] Notification updatedNotification)
    {
        var notification = await _notificationService.GetAsync(id, "");

        if (notification is null)
        {
            return NotFound();
        }

        updatedNotification.Id = notification.Id;

        await _notificationService.UpdateAsync(id, updatedNotification);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var notification = await _notificationService.GetAsync(id, "");

        if (notification is null)
        {
            return NotFound();
        }

        await _notificationService.RemoveAsync(id);

        return NoContent();
    }

}
