using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isatis.Api.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly IsatisDbContext _db;

    public HealthController(IsatisDbContext db)
    {
        _db = db;
    }

    [HttpGet("ping")]
    public ActionResult<string> Ping() => Ok("pong");

    [HttpGet("ready")]
    public async Task<ActionResult<bool>> Ready()
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync();
            if (canConnect) return Ok(true);
            return StatusCode(503, false);
        }
        catch
        {
            return StatusCode(503, false);
        }
    }

    // Debug: return DB connection info (DataSource and Database name)
    [HttpGet("dbinfo")]
    public ActionResult<object> DbInfo()
    {
        try
        {
            var conn = _db.Database.GetDbConnection();
            return Ok(new
            {
                DataSource = conn.DataSource,
                Database = conn.Database,
                ConnectionString = "***masked***" // avoid exposing password
            });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }

    [HttpGet("projects/count")]
    public async Task<ActionResult<int>> ProjectCount()
    {
        var count = await _db.Projects.CountAsync();
        return Ok(count);
    }

    [HttpGet("projects/{projectId:guid}/exists")]
    public async Task<ActionResult<bool>> ProjectExists(Guid projectId)
    {
        var exists = await _db.Projects.AnyAsync(p => p.ProjectId == projectId);
        return exists ? Ok(true) : NotFound(false);
    }
}