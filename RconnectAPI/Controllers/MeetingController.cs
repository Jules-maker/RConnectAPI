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
    public async Task<List<Meeting>> Get(int limit = 10, int page = 1)
    {
        return await _meetingService.GetAsync();
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Meeting>> Get(string id)
    {
        var user = await _meetingService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return user;
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
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the meeting.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Meeting updatedBook)
    {
        var book = await _meetingService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedBook.Id = book.Id;

        await _meetingService.UpdateAsync(id, updatedBook);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _meetingService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _meetingService.RemoveAsync(id);

        return NoContent();
    }

}
