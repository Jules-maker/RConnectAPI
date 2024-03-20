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
    public async Task<IActionResult> Login([FromBody]UserLoginData loginData)
    {
        var user = await _userService.Login(loginData.Email, loginData.Password);
        if (user == null)
        {
            return Unauthorized("Identifiants incorrects.");
        }
        Console.WriteLine("user");
        Console.WriteLine(user.Username);

        var token = _userService.GenerateJwt(user);
        var response = new ResponseData<string>( token );
        return Ok(response);   
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Inscription([FromBody] UserRegisterData newUser)
    {
        try
        {
            var user = await _userService.RegisterAsync(newUser.Username, newUser.Email, newUser.Password,
                newUser.Birthdate, newUser.Firstname, newUser.Lastname);

            var response = new ResponseData<User>(user);
            return Ok(response);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
    }
    
    // FORGOT PASSWORD
    
    [HttpGet("new_token/{email}")]
    public async Task<IActionResult> NewToken(string email)
    {
        var data = await _userService.GenerateToken(email);

        var response = new ResponseData<string>("Token created for " + email);
        
        return Ok(response);
    }
    
    [HttpGet("check_token/{token}")]
    public async Task<IActionResult> CheckToken(string token)
    {
        var user = await _userService.GetByTokenAsync(token);
        
        if (user is null || user.TokenTime is null || user.Token is null)
        {
            return StatusCode(404);
        } 
        if (DateTime.Compare((DateTime)user.TokenTime, DateTime.Now) < 0)
        {
            return BadRequest("Date expired");
        }

        user.TokenTime = null;
        user.Token = null;

        await _userService.UpdateAsync(user.Id, user);

        var response = new ResponseData<string>(user.Id);
        
        return Ok(response);
    }
}