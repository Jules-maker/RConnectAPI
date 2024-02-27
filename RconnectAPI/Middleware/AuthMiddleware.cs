using System.IdentityModel.Tokens.Jwt;
using RconnectAPI.Services;
using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;

namespace RconnectAPI.Middleware;

public class AuthMiddleware
{
    private readonly UserService _userService;
    private readonly RequestDelegate _next;
   
    public AuthMiddleware(RequestDelegate next, UserService userService)
    {
        _next = next;
        _userService = userService;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            Console.WriteLine("skip");
            await _next(context);
            return;
        }
        Console.WriteLine(context.Request.Path);
        var tokenHandler = new JwtSecurityTokenHandler();
        string? authHeader = context.Request.Headers.Authorization;
        if (authHeader != null)
        {
            if (tokenHandler.CanReadToken(authHeader))
            {
                Console.WriteLine("token");
                Console.WriteLine(authHeader);
                var decodedToken = tokenHandler.ReadJwtToken(authHeader);
                Console.WriteLine(decodedToken);
                string userId = decodedToken.Claims.First(claim => claim.Type == "nameid").Value;

                var user = await _userService.GetAsync(userId);

                if (user is null)
                {
                    throw new Exception("Invalid token");
                }
                
                await _next(context);
            }
            else
            {
                Console.WriteLine("token invalide");
                throw new Exception("Invalid token");
            }
            
        }
        else
        {
            throw new Exception("No token");
        }
    }
}