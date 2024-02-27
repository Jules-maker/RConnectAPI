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
    public async Task<List<User>> Get(int limit = 10, int page = 1)
    {
        return await _userService.GetAsync(limit, page);
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<IActionResult> Post(User newUser)
    {
        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, User updatedUser)
    {
        var user = await _userService.GetAsync(id);

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
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        await _userService.RemoveAsync(id);

        return NoContent();
    }
    
    // AUTH ----
    
    [HttpPost("login")]
    public async Task<IActionResult> Connexion([FromBody]UserLoginData loginData)
    {
        var user = await _userService.Login(loginData.Email, loginData.Password);
        if (user == null)
        {
            return Unauthorized("Identifiants incorrects.");
        }
        Console.WriteLine("user");
        Console.WriteLine(user.Username);

        var token = _userService.GenerateJwt(user);
        return Ok(new { token });   
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Inscription([FromBody] UserRegisterData newUser)
    {
        var user = await _userService.RegisterAsync(newUser.Username, newUser.Email, newUser.Password, newUser.Birthdate, newUser.Firstname, newUser.Lastname);
        if (user == null)
        {
            return BadRequest();
        }
        return Ok(user);
    }
}