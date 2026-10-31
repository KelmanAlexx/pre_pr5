using System.Diagnostics;
using CampusDormitory.Api.Data;
using CampusDormitory.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusDormitory.Api.Controllers;

[ApiController]
[Route("api/dormitories")]
public class DormitoriesController : ControllerBase
{
    private static readonly ActivitySource ActivitySource =
        new("CampusDormitory.Api");

    private readonly CampusDbContext _dbContext;
    private readonly ILogger<DormitoriesController> _logger;

    public DormitoriesController(
        CampusDbContext dbContext,
        ILogger<DormitoriesController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Dormitories.GetAll");

        activity?.SetTag("jira.issue", "CAMPUS-108");
        Activity.Current?.SetTag("jira.issue", "CAMPUS-108");

        var dormitories = await _dbContext.Dormitories
            .Include(item => item.Building)
            .ToListAsync(cancellationToken);

        activity?.SetTag("campus.dormitory_count", dormitories.Count);

        return Ok(dormitories.Select(item => new
        {
            item.Id,
            item.Name,
            Address = item.Building == null ? item.Address : item.Building.Address
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDormitoryRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Dormitories.Create");

        activity?.SetTag("jira.issue", "CAMPUS-108");
        Activity.Current?.SetTag("jira.issue", "CAMPUS-108");

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest("Название общежития и адрес обязательны.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Двойная запись нужна на переходном этапе Expand:
        // адрес записывается в новую таблицу Buildings.
        var building = new Building
        {
            Address = request.Address.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Buildings.Add(building);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dormitory = new Dormitory
        {
            Name = request.Name.Trim(),
            Address = request.Address.Trim(), // Временная двойная запись для старых клиентов.
            BuildingId = building.Id
        };

        _dbContext.Dormitories.Add(dormitory);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Пробрасываем доменный идентификатор в текущую Activity.
        activity?.SetTag("campus.dormitory_id", dormitory.Id);
        activity?.SetTag("campus.building_id", building.Id);
        Activity.Current?.SetTag("campus.dormitory_id", dormitory.Id);
        Activity.Current?.SetTag("campus.building_id", building.Id);

        _logger.LogInformation(
            "Создано общежитие {DormitoryId}, здание {BuildingId}, Jira {JiraIssue}",
            dormitory.Id,
            building.Id,
            "CAMPUS-108");

        return Created($"/api/dormitories/{dormitory.Id}", new
        {
            dormitory.Id,
            dormitory.Name,
            BuildingId = building.Id,
            building.Address
        });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Dormitories.Delete");
        activity?.SetTag("jira.issue", "CAMPUS-108");
        activity?.SetTag("campus.dormitory_id", id);
        Activity.Current?.SetTag("campus.dormitory_id", id);

        var dormitory = await _dbContext.Dormitories
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (dormitory == null)
        {
            return NotFound();
        }

        var buildingId = dormitory.BuildingId;
        _dbContext.Dormitories.Remove(dormitory);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (buildingId.HasValue)
        {
            var building = await _dbContext.Buildings
                .FirstOrDefaultAsync(item => item.Id == buildingId.Value, cancellationToken);

            if (building != null)
            {
                _dbContext.Buildings.Remove(building);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return NoContent();
    }
}
