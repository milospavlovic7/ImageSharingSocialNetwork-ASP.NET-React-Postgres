using System;

namespace AspNetCorePostgreSQLDockerApp.DTOs
{
    public class RegisterUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
    }
}
