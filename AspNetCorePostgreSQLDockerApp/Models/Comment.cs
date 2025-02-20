using System;

namespace AspNetCorePostgreSQLDockerApp.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public Guid ImageId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigational property
        public User User { get; set; }
        public Image Image { get; set; }
    }
}
