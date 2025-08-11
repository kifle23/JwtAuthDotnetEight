using JwtAuthDotnetEight.Models;
using JwtAuthDotnetEight.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace JwtAuthDotnetEight.Services
{
    public class TaskService(IUserRepository userRepository, ITaskRepository taskRepository, Microsoft.AspNetCore.SignalR.IHubContext<JwtAuthDotnetEight.Hubs.TaskHub> hub) : ITaskService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<JwtAuthDotnetEight.Hubs.TaskHub> _hub = hub;

        public Task<List<TaskItem>> QueryAsync(JwtAuthDotnetEight.Models.TaskStatus? status, int? assigneeId, string? search)
            => _taskRepository.QueryAsync(status, assigneeId, search);

        public async Task<TaskItem> CreateAsync(string creatorUsername, string title, string? description, JwtAuthDotnetEight.Models.TaskPriority priority, int? assigneeId)
        {
            var creator = await _userRepository.GetUserByUsernameAsync(creatorUsername);
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                Priority = priority,
                CreatorId = creator.Id,
                Creator = creator,
                AssigneeId = assigneeId
            };
            task.UpdatedAt = DateTime.UtcNow;
            var created = await _taskRepository.AddAsync(task);
            await _hub.Clients.All.SendAsync("task:created", new { created.Id, created.Title, created.Description, created.Status, created.Priority, created.AssigneeId, created.UpdatedAt });
            return created;
        }

        public async Task<TaskItem?> UpdateAsync(int id, string? title, string? description, JwtAuthDotnetEight.Models.TaskStatus? status, JwtAuthDotnetEight.Models.TaskPriority? priority, int? assigneeId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return null;

            if (!string.IsNullOrWhiteSpace(title)) task.Title = title;
            if (description != null) task.Description = description;
            if (priority.HasValue) task.Priority = priority.Value;
            if (assigneeId.HasValue) task.AssigneeId = assigneeId.Value;
            if (status.HasValue)
            {
                if (!IsValidTransition(task.Status, status.Value))
                    throw new InvalidOperationException("Invalid status transition");
                task.Status = status.Value;
            }
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task);
            await _hub.Clients.All.SendAsync("task:updated", new { task.Id, task.Title, task.Description, task.Status, task.Priority, task.AssigneeId, task.UpdatedAt });
            return task;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return false;
            await _taskRepository.DeleteAsync(task);
            await _hub.Clients.All.SendAsync("task:deleted", new { id = id });
            return true;
        }

        private static bool IsValidTransition(JwtAuthDotnetEight.Models.TaskStatus from, JwtAuthDotnetEight.Models.TaskStatus to)
        {
            return (from == JwtAuthDotnetEight.Models.TaskStatus.TODO && to == JwtAuthDotnetEight.Models.TaskStatus.IN_PROGRESS)
                || (from == JwtAuthDotnetEight.Models.TaskStatus.IN_PROGRESS && to == JwtAuthDotnetEight.Models.TaskStatus.DONE)
                || (from == to);
        }
    }
}
