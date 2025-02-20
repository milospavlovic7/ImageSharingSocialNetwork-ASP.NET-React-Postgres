using System;
using System.Collections.Generic;

namespace AspNetCorePostgreSQLDockerApp.Models
{
    public class Image
    {
        public Guid ImageId { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigaciona osobina
        public User User { get; set; } // Dodaj ovo

        // Navigacija do komentara vezanih za ovu sliku
        public ICollection<Comment> Comments { get; set; }
    }

}
