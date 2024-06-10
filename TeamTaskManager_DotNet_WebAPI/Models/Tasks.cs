namespace TeamTaskManager_DotNet_WebAPI.Models
{
    public class Tasks
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public DateTimeOffset Modified { get; set; }
        public int UserId { get; set; }
        public int TaskStatusId { get; set; }
        public string? TaskStatus {  get; set; }
    }
}
