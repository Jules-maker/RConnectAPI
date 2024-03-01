using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;

namespace RconnectAPI.Controllers;

[Controller]
[Route("api/[controller]")]
public class MeetingController : Controller
{
    private readonly MeetingService _meetingService;

    public MeetingController(MeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    [HttpGet]
    public async Task<ResponseData<Meeting>> Get(string fields = "", int limit = 10, int page = 1)
    {
        var data = await _meetingService.GetAsync(fields, limit, page);
        var count = await _meetingService.GetCountAsync();
        return new ResponseData<Meeting>(data, count);
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Meeting>> Get(string id, string fields = "")
    {
        var meeting = await _meetingService.GetAsync(id, fields);

        if (meeting is null)
        {
            return NotFound();
        }

        return meeting;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Meeting newMeeting)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _meetingService.CreateAsync(newMeeting);

            return CreatedAtAction(nameof(Get), newMeeting);
        }
        catch
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the meeting.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Meeting updatedMeeting)
    {
        var meeting = await _meetingService.GetAsync(id, "");

        if (meeting is null)
        {
            return NotFound();
        }

        updatedMeeting.Id = meeting.Id;

        await _meetingService.UpdateAsync(id, updatedMeeting);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var meeting = await _meetingService.GetAsync(id, "");

        if (meeting is null)
        {
            return NotFound();
        }

        await _meetingService.RemoveAsync(id);

        return NoContent();
    }

}
