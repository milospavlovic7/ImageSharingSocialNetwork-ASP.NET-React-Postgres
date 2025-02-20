using System;
using System.Threading.Tasks;
using AspNetCorePostgreSQLDockerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DatabaseDbContext _context;

        public LikesRepository(DatabaseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LikeImageAsync(Guid userId, Guid imageId)
        {
            if (await _context.Likes.AnyAsync(l => l.UserId == userId && l.ImageId == imageId))
                return false; // Korisnik je već lajkovao

            var like = new Like
            {
                UserId = userId,
                ImageId = imageId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeImageAsync(Guid userId, Guid imageId)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.ImageId == imageId);
            if (like == null)
                return false; // Lajk ne postoji

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsImageLikedByUserAsync(Guid userId, Guid imageId)
        {
            return await _context.Likes.AnyAsync(l => l.UserId == userId && l.ImageId == imageId);
        }

        public async Task<int> GetLikeCountAsync(Guid imageId)
        {
            return await _context.Likes.CountAsync(l => l.ImageId == imageId);
        }
    }
}
