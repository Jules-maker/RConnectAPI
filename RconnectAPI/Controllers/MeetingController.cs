﻿using Microsoft.AspNetCore.Http.HttpResults;
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
    
    [HttpPost("add_user_to_meeting/{id:length(24)}")]
    public async Task<IActionResult> AddPost(string id, [FromBody] AddUserData bodyData)
    {
        try
        {
            var data = await _meetingService.AddUser(id, bodyData.UserToAdd, bodyData.Notification);
            return Ok(data);
        }
        catch
        {
            // Log the exception
            return StatusCode(500, "An error occurred while adding the user.");
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
            return Ok(data);
        }
        catch
        {
            // Log the exception
            return StatusCode(500, "An error occurred while adding the user.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] Meeting updatedMeeting)
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
