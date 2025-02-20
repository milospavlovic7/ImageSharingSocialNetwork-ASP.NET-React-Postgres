using System;

namespace AspNetCorePostgreSQLDockerApp.DTOs
{
    public class UpdateUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
    }
}
