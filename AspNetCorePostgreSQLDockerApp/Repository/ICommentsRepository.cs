using AspNetCorePostgreSQLDockerApp.Models;
using AspNetCorePostgreSQLDockerApp.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public interface ICommentsRepository
    {
        Task<IEnumerable<CommentDTO>> GetCommentsByImageIdAsync(Guid imageId);
        Task<CommentDTO> GetCommentByIdAsync(Guid commentId); // Vraća komentar po ID-u
        Task AddCommentAsync(Comment comment); // Dodaje novi komentar
        Task UpdateCommentAsync(Comment comment); // Ažurira postojeći komentar
        Task DeleteCommentAsync(Guid commentId); // Briše komentar
    }
}
