namespace JwtAuthDotnetEight.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<UserRole>? UserRoles { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<TaskItem>? AssignedTasks { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<TaskItem>? CreatedTasks { get; set; }

    }
}
