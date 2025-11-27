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

    // POST: api/users
    // ورودی مستقیم: CreateUserCommand
    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreateUserCommand command)
    {
        var result = await mediator.Send(command);

        if (result.Succeeded)
            return Ok(result);

        return BadRequest(result);
    }

    // PUT: api/users
    // ورودی مستقیم: UpdateUserCommand (User + NewPassword)
    [HttpPut]
    public async Task<ActionResult<Result<Guid>>> Update([FromBody] UpdateUserCommand command)
    {
        var result = await mediator.Send(command);

        if (result.Succeeded)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<Guid>>> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteUserCommand(id));
        return Ok(result);
    }
}
