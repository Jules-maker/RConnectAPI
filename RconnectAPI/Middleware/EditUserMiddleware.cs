using System.IdentityModel.Tokens.Jwt;
using RconnectAPI.Services;

namespace RconnectAPI.Middleware;

public class EditUserMiddleware
{
    private readonly UserService _userService;
    private readonly RequestDelegate _next;
   
    public EditUserMiddleware(RequestDelegate next, UserService userService)
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
        if (context.Request.Method != "PUT" || !context.Request.Path.StartsWithSegments("/api/User"))
        {
            await _next(context); 
            return;
        }
        var sentId = context.GetRouteData().Values["id"].ToString();
        var tokenHandler = new JwtSecurityTokenHandler();
        string? authHeader = context.Request.Headers.Authorization;
        
        var decodedToken = tokenHandler.ReadJwtToken(authHeader);
        string userId = decodedToken.Claims.First(claim => claim.Type == "nameid").Value;
        if (!sentId.Equals(userId))
        {
            throw new Exception("Unauthorized");
        }
        await _next(context);
    }
}