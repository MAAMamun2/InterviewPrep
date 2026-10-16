using InterviewPrep.Application.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace InterviewPrep.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [EnableRateLimiting] applies the "login" fixed-window policy configured in Program.cs --
    /// a defense-in-depth measure against credential-stuffing/brute-force attacks hitting this
    /// specific endpoint. This is the kind of concrete security detail worth naming unprompted.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }
}
