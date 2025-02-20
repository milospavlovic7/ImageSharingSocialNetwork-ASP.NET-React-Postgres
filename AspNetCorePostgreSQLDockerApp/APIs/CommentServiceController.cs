using AspNetCorePostgreSQLDockerApp.Models;
using AspNetCorePostgreSQLDockerApp.DTOs;
using AspNetCorePostgreSQLDockerApp.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AspNetCorePostgreSQLDockerApp.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentServiceController : ControllerBase
    {
        private readonly ILogger<CommentServiceController> _logger;
        private readonly ICommentsRepository _commentsRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IImagesRepository _imagesRepository;

        public CommentServiceController(ICommentsRepository commentsRepository, IUsersRepository usersRepository, IImagesRepository imagesRepository, ILogger<CommentServiceController> logger)
        {
            _commentsRepository = commentsRepository;
            _usersRepository = usersRepository;
            _imagesRepository = imagesRepository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentsByImage([FromQuery] Guid imageId)
        {
            try
            {
                var comments = await _commentsRepository.GetCommentsByImageIdAsync(imageId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{commentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDTO>> GetCommentById(Guid commentId)
        {
            try
            {
                var comment = await _commentsRepository.GetCommentByIdAsync(commentId);

                if (comment == null)
                {
                    _logger.LogWarning($"Comment with ID {commentId} not found.");
                    return NotFound(new { message = "Comment not found." });
                }

                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment by ID");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentDTO>> AddComment([FromBody] CommentCreateDTO commentDto)
        {
            try
            {
                // Proverite korisnika i sliku
                var user = await _usersRepository.GetUserByIdAsync(commentDto.UserId);
                var image = await _imagesRepository.GetImageByIdAsync(commentDto.ImageId);

                if (user == null || image == null)
                {
                    return BadRequest("User or Image not found.");
                }

                // Kreirajte komentar
                var comment = new Comment
                {
                    CommentId = Guid.NewGuid(),
                    Content = commentDto.Content,
                    CreatedAt = DateTime.UtcNow,
                    UserId = user.UserId,
                    ImageId = image.ImageId
                };

                // Dodajte komentar u bazu
                await _commentsRepository.AddCommentAsync(comment);

                // Vratite odgovor sa svim podacima o komentaru, korisniku i slici
                var commentDtoResponse = new CommentDTO
                {
                    CommentId = comment.CommentId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    User = new UserDTO
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        ProfilePicture = user.ProfilePicture
                    },
                    Image = new ImageDTO
                    {
                        ImageId = image.ImageId,
                        ImageUrl = image.ImageUrl,
                        Description = image.Description,
                        CreatedAt = image.CreatedAt
                    }
                };

                // Vratite 201 Created status sa odgovorom o komentaru
                return CreatedAtAction(nameof(GetCommentById), new { commentId = comment.CommentId }, commentDtoResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding comment");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentDTO commentDto)
        {
            try
            {
                var existingComment = await _commentsRepository.GetCommentByIdAsync(commentId);
                if (existingComment == null)
                {
                    return NotFound(new { message = "Comment not found." });
                }

                existingComment.Content = commentDto.Content;

                await _commentsRepository.UpdateCommentAsync(new Comment
                {
                    CommentId = existingComment.CommentId,
                    Content = existingComment.Content,
                    CreatedAt = existingComment.CreatedAt,
                    UserId = existingComment.User.UserId,
                    ImageId = existingComment.Image.ImageId
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating comment");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<ActionResult> DeleteComment(Guid commentId)
        {
            try
            {
                var comment = await _commentsRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                {
                    return NotFound("Comment not found.");
                }

                await _commentsRepository.DeleteCommentAsync(commentId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting comment");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
