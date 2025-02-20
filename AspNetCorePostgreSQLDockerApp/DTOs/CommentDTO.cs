using System;

namespace AspNetCorePostgreSQLDockerApp.DTOs
{
    public class CommentDTO
    {
        public Guid CommentId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDTO User { get; set; }
        public ImageDTO Image { get; set; }
    }
}
