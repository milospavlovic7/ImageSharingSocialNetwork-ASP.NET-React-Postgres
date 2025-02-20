using System;
using System.Collections.Generic;

namespace AspNetCorePostgreSQLDockerApp.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigacija do slika koje je korisnik postavio
        public ICollection<Image> Images { get; set; }

        // Navigacija do komentara koje je korisnik postavio
        public ICollection<Comment> Comments { get; set; }
    }

}
