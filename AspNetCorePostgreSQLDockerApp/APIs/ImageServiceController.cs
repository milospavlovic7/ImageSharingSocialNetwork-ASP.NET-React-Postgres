using AspNetCorePostgreSQLDockerApp.Models;
using AspNetCorePostgreSQLDockerApp.DTOs;
using AspNetCorePostgreSQLDockerApp.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AspNetCorePostgreSQLDockerApp.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageServiceController : ControllerBase
    {
        private readonly ILogger<ImageServiceController> _logger;
        private readonly IImagesRepository _imagesRepository;
        private readonly IUsersRepository _usersRepository;

        public ImageServiceController(IImagesRepository imagesRepository, IUsersRepository usersRepository, ILogger<ImageServiceController> logger)
        {
            _logger = logger;
            _imagesRepository = imagesRepository;
            _usersRepository = usersRepository;
        }

        // Get all images (feed pagginated)
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetImagesByPageAsync(int page = 1, int pageSize = 10)
        {
            var images = await _imagesRepository.GetImagesByPageAsync(page, pageSize);

            if (images == null || !images.Any())
            {
                return NoContent(); // Vraća 204 ako nema slika
            }

            return Ok(images); // Vraća slike ako postoji
        }


        // Get images by user ID
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ImageDTO>>> GetImagesByUserId(Guid userId)
        {
            try
            {
                // Proveriti da li korisnik postoji
                var user = await _usersRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Dohvati slike korisnika
                var images = await _imagesRepository.GetImagesByUserIdAsync(userId);
                if (images == null || !images.Any())
                {
                    return NotFound(new { message = "No images found for this user." });
                }

                var imageDtos = images.Select(image => new ImageDTO
                {
                    ImageId = image.ImageId,
                    ImageUrl = image.ImageUrl,
                    Description = image.Description,
                    CreatedAt = image.CreatedAt,
                    User = new UserDTO
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        ProfilePicture = user.ProfilePicture
                    }
                }).ToList();

                return Ok(imageDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving images for user");
                return StatusCode(500, "Internal server error");
            }
        }

        // Get image by ID
        [HttpGet("{imageId}")]
        [Authorize]
        public async Task<ActionResult<ImageDTO>> GetImageById(Guid imageId)
        {
            try
            {
                var image = await _imagesRepository.GetImageByIdAsync(imageId);
                if (image == null)
                {
                    return NotFound();
                }
                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image by ID");
                return StatusCode(500, "Internal server error");
            }
        }

        // Add a new image
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ImageDTO>> AddImage([FromForm] IFormFile imageFile, [FromForm] Guid userId, [FromForm] string description)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new { message = "No image file uploaded." });
                }

                var user = await _usersRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { message = "User not found." });
                }

                // Log incoming data
                _logger.LogInformation($"Received image: {imageFile.FileName}, Size: {imageFile.Length}, Description: {description}");

                var imageUrl = await SaveImageToStorage(imageFile);

                var image = new Image
                {
                    ImageId = Guid.NewGuid(),
                    ImageUrl = imageUrl,
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    UserId = user.UserId
                };

                await _imagesRepository.AddImageAsync(image);

                var resultDto = new ImageDTO
                {
                    ImageId = image.ImageId,
                    ImageUrl = image.ImageUrl,
                    Description = image.Description,
                    CreatedAt = image.CreatedAt,
                    User = new UserDTO
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        ProfilePicture = user.ProfilePicture
                    }
                };

                return CreatedAtAction(nameof(GetImageById), new { imageId = resultDto.ImageId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        // Privatna funkcija za čuvanje slike na serveru
        private static async Task<string> SaveImageToStorage(IFormFile imageFile)
        {
            Console.WriteLine($"Saving image: {imageFile.FileName}, Size: {imageFile.Length} bytes");
            var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(imagesDirectory))
            {
                Directory.CreateDirectory(imagesDirectory);
            }

            var filePath = Path.Combine(imagesDirectory, imageFile.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                Console.WriteLine($"Image saved successfully at: {filePath}");

                // Generiši apsolutni URL
                var baseUrl = "http://localhost:5000"; // Možeš kasnije uzeti iz konfiguracije
                return $"{baseUrl}/images/{imageFile.FileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                throw new Exception("Error saving image.");
            }
        }




        // Update image description
        [HttpPut("{imageId}")]
        [Authorize]
        public async Task<ActionResult> UpdateImage(Guid imageId, [FromBody] UpdateImageDTO imageDto)
        {
            try
            {
                await _imagesRepository.UpdateImageDescriptionAsync(imageId, imageDto.Description);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating image");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        // Delete an image
        [HttpDelete("{imageId}")]
        [Authorize]
        public async Task<ActionResult> DeleteImage(Guid imageId)
        {
            try
            {
                var image = await _imagesRepository.GetImageByIdAsync(imageId);
                if (image == null)
                {
                    return NotFound("Image not found.");
                }

                await _imagesRepository.DeleteImageAsync(imageId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting image");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
