using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCorePostgreSQLDockerApp.Models
{
    public class Like
    {
        [Key, Column(Order = 1)]
        public Guid UserId { get; set; }

        [Key, Column(Order = 2)]
        public Guid ImageId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
