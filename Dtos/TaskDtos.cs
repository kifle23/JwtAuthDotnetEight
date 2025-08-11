using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Dtos
{
    public class TaskCreateDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
    public JwtAuthDotnetEight.Models.TaskPriority Priority { get; set; } = JwtAuthDotnetEight.Models.TaskPriority.MEDIUM;
        public int? AssigneeId { get; set; }
    }

    public class TaskUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    public JwtAuthDotnetEight.Models.TaskStatus? Status { get; set; }
    public JwtAuthDotnetEight.Models.TaskPriority? Priority { get; set; }
        public int? AssigneeId { get; set; }
    }

    public class TaskQueryDto
    {
    public JwtAuthDotnetEight.Models.TaskStatus? Status { get; set; }
        public int? AssigneeId { get; set; }
        public string? Search { get; set; }
    }
}
