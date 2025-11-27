using Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrapper;

namespace Isatis.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<Result<string>>> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);

        if (result.Succeeded)
            return Ok(result);

        return Unauthorized(result);
    }
}