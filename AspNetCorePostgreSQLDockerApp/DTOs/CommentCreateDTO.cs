using System;

namespace AspNetCorePostgreSQLDockerApp.DTOs
{
    public class CommentCreateDTO
    {
        public string Content { get; set; }
        public Guid UserId { get; set; } // ID korisnika koji postavlja komentar
        public Guid ImageId { get; set; } // ID slike na koju se postavlja komentar
    }

}
