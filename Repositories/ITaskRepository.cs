using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(int id);
        Task<List<TaskItem>> QueryAsync(JwtAuthDotnetEight.Models.TaskStatus? status, int? assigneeId, string? search);
        Task<TaskItem> AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
    }
}
