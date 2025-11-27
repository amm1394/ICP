using Application.Features.Roles.Commands.CreateRole;
using Application.Features.Roles.Commands.DeleteRole;
using Application.Features.Roles.Commands.UpdateRole;
using Application.Features.Roles.Queries.GetRoleById;
using Application.Features.Roles.Queries.GetAllRoles; // 👈 این using جدید اضافه شد
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrapper;

namespace Isatis.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// دریافت همه نقش‌ها
    /// GET: api/roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<List<Role>>>> GetAll()
    {
        var result = await mediator.Send(new GetAllRolesQuery());
        return Ok(result);
    }

    /// <summary>
    /// دریافت اطلاعات یک نقش بر اساس Id
    /// GET: api/roles/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<Role>>> GetById(Guid id)
    {
        var result = await mediator.Send(new GetRoleByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// ایجاد نقش جدید
    /// POST: api/roles
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreateRoleCommand command)
    {
        var result = await mediator.Send(command);

        if (result.Succeeded)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// ویرایش نقش
    /// PUT: api/roles
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<Result<Guid>>> Update([FromBody] UpdateRoleCommand command)
    {
        var result = await mediator.Send(command);

        if (result.Succeeded)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// حذف نقش
    /// DELETE: api/roles/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result<Guid>>> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteRoleCommand(id));
        return Ok(result);
    }
}
