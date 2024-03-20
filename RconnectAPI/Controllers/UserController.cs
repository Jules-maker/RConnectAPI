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
    public async Task<ResponseData<User>> Get(string fields = "", int limit = 10, int page = 1)
    {
        var data = await _userService.GetAsync(fields, limit, page);
        var count = await _userService.GetCountAsync();
        return new ResponseData<User>(data, count);
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id, string fields = "")
    {
        var user = await _userService.GetOneAsync(id, fields);

        if (user is null)
        {
            return NotFound();
        }

        return user;
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
            return NotFound();
        }

        updatedUser.Id = user.Id;

        await _userService.UpdateAsync(id, updatedUser);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetOneAsync(id, "");

        if (user is null)
        {
            return NotFound();
        }

        await _userService.RemoveAsync(id);

        return NoContent();
    }
}