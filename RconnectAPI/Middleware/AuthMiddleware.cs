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
            await _next(context);
            return;
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        string? authHeader = context.Request.Headers.Authorization;
        if (authHeader != null)
        {
            if (tokenHandler.CanReadToken(authHeader))
            {
                var decodedToken = tokenHandler.ReadJwtToken(authHeader);
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
                throw new Exception("Invalid token");
            }
            
        }
        else
        {
            throw new Exception("No token");
        }
    }
}