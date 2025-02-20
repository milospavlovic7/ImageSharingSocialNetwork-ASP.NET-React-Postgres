using System;

namespace AspNetCorePostgreSQLDockerApp.DTOs
{
    public class ImageDTO
    {
        public Guid ImageId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDTO User { get; set; }
    }
}
