using Asp.Versioning;
using human_recognition.Application.Features.Auth.Commands;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace human_recognition.Api.Controllers.v1;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiryDays);

        return Ok(new
        {
            accessToken = result.AccessToken
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken)) return Unauthorized(new
        {
            messsage = "Refresh Token is missing in cookies"
        });

        var command = new RefreshTokenCommand(refreshToken);

        var result = await _mediator.SendAsync(command, cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiryDays);
        return Ok(new
        {
            accessToken = result.AccessToken
        });
    }

    // === Private Function
    private void SetRefreshTokenCookie(string token, int expiryDays)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(expiryDays),
            Secure = true,
            SameSite = SameSiteMode.None
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}