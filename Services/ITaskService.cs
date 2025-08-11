using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Services
{
    public interface ITaskService
    {
        Task<List<TaskItem>> QueryAsync(JwtAuthDotnetEight.Models.TaskStatus? status, int? assigneeId, string? search);
        Task<TaskItem> CreateAsync(string creatorUsername, string title, string? description, JwtAuthDotnetEight.Models.TaskPriority priority, int? assigneeId);
        Task<TaskItem?> UpdateAsync(int id, string? title, string? description, JwtAuthDotnetEight.Models.TaskStatus? status, JwtAuthDotnetEight.Models.TaskPriority? priority, int? assigneeId);
        Task<bool> DeleteAsync(int id);
    }
}
