using AspNetCorePostgreSQLDockerApp.DTOs;
using AspNetCorePostgreSQLDockerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public class ImagesRepository : IImagesRepository
    {
        private readonly DatabaseDbContext _context;

        public ImagesRepository(DatabaseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ImageDTO>> GetImagesByPageAsync(int page, int pageSize)
        {
            var totalImages = await _context.Images.CountAsync();
            var totalPages = (int)Math.Ceiling(totalImages / (double)pageSize);

            if (page > totalPages)
            {
                return new List<ImageDTO>(); // Vraća prazan niz ako stranica ne postoji
            }

            var images = await _context.Images
                .OrderByDescending(image => image.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(image => new ImageDTO
                {
                    ImageId = image.ImageId,
                    Description = image.Description,
                    ImageUrl = image.ImageUrl,
                    CreatedAt = image.CreatedAt,
                    User = new UserDTO { UserId = image.UserId, Name = image.User.Name }
                })
                .ToListAsync();

            return images;
        }




        // Vraća sliku po ID-u
        public async Task<ImageDTO> GetImageByIdAsync(Guid imageId)
        {
            var image = await _context.Images
                                      .Include(i => i.User)
                                      .FirstOrDefaultAsync(i => i.ImageId == imageId);

            if (image == null)
                return null;

            return new ImageDTO
            {
                ImageId = image.ImageId,
                ImageUrl = image.ImageUrl,
                Description = image.Description,
                CreatedAt = image.CreatedAt,
                User = new UserDTO
                {
                    UserId = image.User.UserId,
                    Name = image.User.Name,
                    Email = image.User.Email,
                    ProfilePicture = image.User.ProfilePicture
                }
            };
        }

        // Vraća slike za određenog korisnika
        public async Task<IEnumerable<ImageDTO>> GetImagesByUserIdAsync(Guid userId)
        {
            var images = await _context.Images
                                       .Where(i => i.UserId == userId) // Filtriraj slike po userId
                                       .Include(i => i.User) // Učitaj korisnika
                                       .OrderByDescending(i => i.CreatedAt) // Sortiraj po datumu kreiranja
                                       .ToListAsync();

            return images.Select(i => new ImageDTO
            {
                ImageId = i.ImageId,
                ImageUrl = i.ImageUrl,
                Description = i.Description,
                CreatedAt = i.CreatedAt,
                User = new UserDTO
                {
                    UserId = i.User.UserId,
                    Name = i.User.Name,
                    Email = i.User.Email,
                    ProfilePicture = i.User.ProfilePicture
                }
            }).ToList();
        }

        // Dodaje novu sliku
        public async Task AddImageAsync(Image image)
        {
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        //Azurira opis
        public async Task UpdateImageDescriptionAsync(Guid imageId, string description)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
            {
                throw new Exception("Image not found.");
            }
            image.Description = description;
            await _context.SaveChangesAsync();
        }


        // Briše sliku
        public async Task DeleteImageAsync(Guid imageId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image != null)
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
    }
}
