using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.DeleteUser;
using Application.Features.Users.Commands.UpdateUser;
using Application.Features.Users.Queries.GetAllUsers;
using Application.Features.Users.Queries.GetUserById;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrapper;

namespace Isatis.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<User>>>> GetAll()
    {
        var result = await mediator.Send(new GetAllUsersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<User>>> GetById(Guid id)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.User, request.Password);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<Result<Guid>>> Update([FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(request.User, request.NewPassword);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<Guid>>> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteUserCommand(id));
        return Ok(result);
    }
}

public class CreateUserRequest
{
    public required User User { get; set; }
    public required string Password { get; set; }
}

public class UpdateUserRequest
{
    public required User User { get; set; }
    public string? NewPassword { get; set; }
}