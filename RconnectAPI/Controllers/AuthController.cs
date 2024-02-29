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
}