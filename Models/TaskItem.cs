namespace JwtAuthDotnetEight.Models
{
    public enum TaskStatus
    {
        TODO = 0,
        IN_PROGRESS = 1,
        DONE = 2
    }

    public enum TaskPriority
    {
        LOW = 0,
        MEDIUM = 1,
        HIGH = 2
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.TODO;
        public TaskPriority Priority { get; set; } = TaskPriority.MEDIUM;
        public int? AssigneeId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public User? Assignee { get; set; }
        public required int CreatorId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public required User Creator { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
