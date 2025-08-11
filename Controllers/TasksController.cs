using JwtAuthDotnetEight.Attributes;
using JwtAuthDotnetEight.Dtos;
using JwtAuthDotnetEight.Services;
using Microsoft.AspNetCore.Mvc;
using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController(ITaskService taskService) : ControllerBase
    {
        private readonly ITaskService _taskService = taskService;

        [HttpGet]
        [RolesAuthorize("User", "Admin")]
        public async Task<IActionResult> Get([FromQuery] JwtAuthDotnetEight.Models.TaskStatus? status, [FromQuery] int? assignee, [FromQuery] string? search)
        {
            var tasks = await _taskService.QueryAsync(status, assignee, search);
            var result = tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.AssigneeId,
                t.UpdatedAt
            });
            return Ok(result);
        }

        [HttpPost]
        [RolesAuthorize("User", "Admin")]
        public async Task<IActionResult> Create([FromBody] TaskCreateDto dto)
        {
            var username = User.Identity?.Name ?? throw new UnauthorizedAccessException();
            var task = await _taskService.CreateAsync(username, dto.Title, dto.Description, dto.Priority, dto.AssigneeId);
            return Ok(new
            {
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.AssigneeId,
                task.UpdatedAt
            });
        }

        [HttpPut("{id:int}")]
        [RolesAuthorize("User", "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
        {
            var updated = await _taskService.UpdateAsync(id, dto.Title, dto.Description, dto.Status, dto.Priority, dto.AssigneeId);
            if (updated == null) return NotFound();
            return Ok(new
            {
                updated.Id,
                updated.Title,
                updated.Description,
                updated.Status,
                updated.Priority,
                updated.AssigneeId,
                updated.UpdatedAt
            });
        }

        [HttpDelete("{id:int}")]
        [RolesAuthorize("Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _taskService.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
