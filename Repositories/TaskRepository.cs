using JwtAuthDotnetEight.Data;
using JwtAuthDotnetEight.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotnetEight.Repositories
{
    public class TaskRepository(DataContext context) : ITaskRepository
    {
        private readonly DataContext _context = context;

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

    public async Task<List<TaskItem>> QueryAsync(JwtAuthDotnetEight.Models.TaskStatus? status, int? assigneeId, string? search)
        {
            var query = _context.Tasks.AsQueryable();

            if (status.HasValue)
            {
                var s = status.Value;
                query = query.Where(t => t.Status == s);
            }
            if (assigneeId.HasValue)
                query = query.Where(t => t.AssigneeId == assigneeId);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            return await query
                .OrderByDescending(t => t.UpdatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TaskItem> AddAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
