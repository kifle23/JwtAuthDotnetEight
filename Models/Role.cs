namespace JwtAuthDotnetEight.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<UserRole>? UserRoles { get; set; }

    }
}
