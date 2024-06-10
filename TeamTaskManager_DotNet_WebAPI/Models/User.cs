namespace TeamTaskManager_DotNet_WebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int UserTypeId { get; set; }
        public string? UserType { get; set; }
        public List<Tasks>? TaskList { get; set; }
    }
}
