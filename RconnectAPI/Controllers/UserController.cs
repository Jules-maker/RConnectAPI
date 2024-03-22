using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;

namespace RconnectAPI.Controllers;

[Controller]
[Route("api/[controller]")]
public class UserController: Controller {
    
    private readonly UserService _userService;

    public UserController(UserService userService) {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ListResponseData<User>> Get(string fields = "", int limit = 10, int page = 1)
    {
        var data = await _userService.GetAsync(null, fields, limit, page);
        var count = await _userService.GetCountAsync();
        return new ListResponseData<User>(data, count);
    }
    
    [HttpGet("profile/{id:length(24)}")]
    public async Task<ActionResult<User>> GetProfile(string id)
    {
        try
        {
            var data = await _userService.GetProfileInfo(id);
            var response = new ResponseData<UserProfileData>(data);
            return Ok(response);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    [HttpGet("restaurant/{restoId:length(24)}")]
    public async Task<ActionResult<User>> GetFromRestaurant(string restoId, int limit = 10, int page = 1)
    {
        try
        {
            var fields = "";
            var users = await _userService.GetAsync(u => u.Favouritehosts != null && u.Favouritehosts.Contains(restoId), fields, limit, page);
            var count = await _userService.GetCountAsync(u => u.Favouritehosts != null && u.Favouritehosts.Contains(restoId));
            var response = new ListResponseData<User>(users, count);
            return Ok(response);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    [HttpGet("add_hobby/{userId:length(24)}")]
    public async Task<ActionResult<User>> AddHobby(string hobbyId, string userId)
    {
        try
        {
            var response = new ResponseData<User>(await _userService.AddHobby(userId, hobbyId));
            return Ok(response);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetOne(string id, string fields = "")
    {
        var user = await _userService.GetOneAsync(id, fields);

        if (user is null)
        {
            return StatusCode(404);
        }

        return Ok(new ResponseData<User>(user));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]  User newUser)
    {

        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] User updatedUser)
    {
        var user = await _userService.GetOneAsync(id, "");

        if (user is null)
        {
            return StatusCode(404);
        }

        updatedUser.Id = user.Id;

        await _userService.UpdateAsync(id, updatedUser);

        return Ok(new ResponseData<User>(updatedUser));
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetOneAsync(id, "");

        if (user is null)
        {
            return StatusCode(404);
        }

        await _userService.RemoveAsync(id);

        return Ok(new ResponseData<string>(id + " deleted"));
    }
}