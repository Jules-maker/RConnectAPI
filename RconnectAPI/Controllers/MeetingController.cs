using Microsoft.AspNetCore.Http.HttpResults;
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
    public async Task<IActionResult> Get(string fields = "", int limit = 10, int page = 1)
    {
        var data = await _meetingService.GetAsync(fields, limit, page);
        var count = await _meetingService.GetCountAsync();
        return Ok(new ListResponseData<Meeting>(data, count));
    }
    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> Get(string id, string fields = "")
    {
        var meeting = await _meetingService.GetAsync(id, fields);

        if (meeting is null)
        {
            return StatusCode(404);
        }

        return Ok(new ResponseData<Meeting>(meeting));
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

            return Ok(new ResponseData<Meeting>(newMeeting));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    [HttpPost("add_user_to_meeting/{id:length(24)}")]
    public async Task<IActionResult> AddPost(string id, [FromBody] AddUserData bodyData)
    {
        try
        {
            var data = await _meetingService.AddUser(id, bodyData.UserToAdd, bodyData.Notification);
            return Ok(new ResponseData<string>(data));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    [HttpPost("invite_user_to_meeting/{id:length(24)}")]
    public async Task<IActionResult> InvitePost(string id, [FromBody] string userToInvite)
    {
        Console.WriteLine(userToInvite);
        Console.WriteLine(id);
        try
        {
            var data = await _meetingService.InviteUser(id, userToInvite);
            return Ok(new ResponseData<Meeting>(data));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] Meeting updatedMeeting)
    {
        var meeting = await _meetingService.GetAsync(id, "");

        if (meeting is null)
        {
            return StatusCode(404);
        }

        updatedMeeting.Id = meeting.Id;

        await _meetingService.UpdateAsync(id, updatedMeeting);
        return Ok(new ResponseData<Meeting>(updatedMeeting));
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var meeting = await _meetingService.GetAsync(id, "");

        if (meeting is null)
        {
            return StatusCode(404);
        }

        await _meetingService.RemoveAsync(id);

        return Ok(new ResponseData<string>(id + " deleted"));
    }

}
