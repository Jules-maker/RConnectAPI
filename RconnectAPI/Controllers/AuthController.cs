using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;

namespace RconnectAPI.Controllers;

[Controller]
[Route("[controller]")]
public class AuthController: Controller {
    
    private readonly UserService _userService;

    public AuthController(UserService userService) {
        _userService = userService;
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
    
    // FORGOT PASSWORD
    
    [HttpGet("new_token/{email}")]
    public async Task<IActionResult> NewToken(string email)
    {
        var data = await _userService.GenerateToken(email);

        return Ok(data);
    }
    
    [HttpGet("check_token/{token}")]
    public async Task<IActionResult> CheckToken(string token)
    {
        var user = await _userService.GetByTokenAsync(token);
        
        if (user is null || user.TokenTime is null || user.Token is null)
        {
            return NotFound();
        } 
        if (DateTime.Compare((DateTime)user.TokenTime, DateTime.Now) < 0)
        {
            return BadRequest("Date expired");
        }

        user.TokenTime = null;
        user.Token = null;

        await _userService.UpdateAsync(user.Id, user);

        return Ok("ok");
    }
}