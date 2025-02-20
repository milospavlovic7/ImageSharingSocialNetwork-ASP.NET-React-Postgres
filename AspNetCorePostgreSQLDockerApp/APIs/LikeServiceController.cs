using System;
using System.Threading.Tasks;
using AspNetCorePostgreSQLDockerApp.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCorePostgreSQLDockerApp.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikeServiceController : ControllerBase
    {
        private readonly ILikesRepository _likesRepository;

        public LikeServiceController(ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
        }

        [HttpPost("like")]
        public async Task<IActionResult> LikeImage([FromQuery] Guid userId, [FromQuery] Guid imageId)
        {
            bool success = await _likesRepository.LikeImageAsync(userId, imageId);
            if (!success)
                return BadRequest("You have already liked this image.");

            return Ok("Image liked successfully.");
        }

        [HttpDelete("unlike")]
        public async Task<IActionResult> UnlikeImage([FromQuery] Guid userId, [FromQuery] Guid imageId)
        {
            bool success = await _likesRepository.UnlikeImageAsync(userId, imageId);
            if (!success)
                return NotFound("Like not found.");

            return Ok("Image unliked successfully.");
        }

        [HttpGet("isLiked")]
        public async Task<IActionResult> IsImageLiked([FromQuery] Guid userId, [FromQuery] Guid imageId)
        {
            bool isLiked = await _likesRepository.IsImageLikedByUserAsync(userId, imageId);
            return Ok(new { isLiked });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetLikeCount([FromQuery] Guid imageId)
        {
            int likeCount = await _likesRepository.GetLikeCountAsync(imageId);
            return Ok(new { likeCount });
        }

    }
}
