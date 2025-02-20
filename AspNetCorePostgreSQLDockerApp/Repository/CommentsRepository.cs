using AspNetCorePostgreSQLDockerApp.Models;
using AspNetCorePostgreSQLDockerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public class CommentsRepository : ICommentsRepository
    {
        private readonly DatabaseDbContext _context;
        private readonly ILogger<CommentsRepository> _logger;

        public CommentsRepository(DatabaseDbContext context, ILogger<CommentsRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CommentDTO>> GetCommentsByImageIdAsync(Guid imageId)
        {
            try
            {
                var comments = await _context.Comments
                                             .Include(c => c.User)  // Učitaj korisnika
                                             .Include(c => c.Image) // Učitaj sliku
                                             .Where(c => c.ImageId == imageId) // Filtriraj komentare po imageId
                                             .OrderByDescending(c => c.CreatedAt) // Sortiraj po vremenu kreiranja
                                             .ToListAsync();

                if (comments == null || !comments.Any())
                {
                    _logger.LogWarning($"No comments found for image with ID {imageId}.");
                }

                return comments.Select(c => new CommentDTO
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    User = c.User == null ? null : new UserDTO
                    {
                        UserId = c.User.UserId,
                        Name = c.User.Name,
                        Email = c.User.Email,
                        ProfilePicture = c.User.ProfilePicture
                    },
                    Image = c.Image == null ? null : new ImageDTO
                    {
                        ImageId = c.Image.ImageId,
                        ImageUrl = c.Image.ImageUrl,
                        Description = c.Image.Description,
                        CreatedAt = c.Image.CreatedAt,
                        User = c.Image.User == null ? null : new UserDTO
                        {
                            UserId = c.Image.User.UserId,
                            Name = c.Image.User.Name,
                            Email = c.Image.User.Email,
                            ProfilePicture = c.Image.User.ProfilePicture
                        }
                    }
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving comments for image with ID {imageId}.");
                throw;
            }
        }


        public async Task<CommentDTO> GetCommentByIdAsync(Guid commentId)
        {
            try
            {
                var comment = await _context.Comments
                                            .Include(c => c.User)
                                            .Include(c => c.Image)
                                            .FirstOrDefaultAsync(c => c.CommentId == commentId);

                if (comment == null)
                {
                    _logger.LogWarning($"Comment with ID {commentId} not found.");
                    return null;
                }

                return new CommentDTO
                {
                    CommentId = comment.CommentId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    User = comment.User == null ? null : new UserDTO
                    {
                        UserId = comment.User.UserId,
                        Name = comment.User.Name,
                        Email = comment.User.Email,
                        ProfilePicture = comment.User.ProfilePicture
                    },
                    Image = comment.Image == null ? null : new ImageDTO
                    {
                        ImageId = comment.Image.ImageId,
                        ImageUrl = comment.Image.ImageUrl,
                        Description = comment.Image.Description,
                        CreatedAt = comment.Image.CreatedAt,
                        User = comment.Image.User == null ? null : new UserDTO
                        {
                            UserId = comment.Image.User.UserId,
                            Name = comment.Image.User.Name,
                            Email = comment.Image.User.Email,
                            ProfilePicture = comment.Image.User.ProfilePicture
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving comment by ID: {commentId}");
                throw;
            }
        }

        public async Task AddCommentAsync(Comment comment)
        {
            try
            {
                if (comment == null)
                {
                    _logger.LogWarning("Attempted to add a null comment.");
                    throw new ArgumentNullException(nameof(comment));
                }

                await _context.Comments.AddAsync(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment.");
                throw;
            }
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            try
            {
                if (comment == null)
                {
                    _logger.LogWarning("Attempted to update a null comment.");
                    throw new ArgumentNullException(nameof(comment));
                }

                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment.");
                throw;
            }
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(commentId);
                if (comment == null)
                {
                    _logger.LogWarning($"Comment with ID {commentId} not found.");
                    return;
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting comment with ID {commentId}.");
                throw;
            }
        }
    }
}
