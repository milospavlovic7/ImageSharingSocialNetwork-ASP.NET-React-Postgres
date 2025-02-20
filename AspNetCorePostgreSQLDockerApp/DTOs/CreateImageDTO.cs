using System;

namespace AspNetCorePostgreSQLDockerApp.DTOs
{
    public class CreateImageDTO
    {
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
    }
}
