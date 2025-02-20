using System;
using System.Threading.Tasks;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public interface ILikesRepository
    {
        Task<bool> LikeImageAsync(Guid userId, Guid imageId);
        Task<bool> UnlikeImageAsync(Guid userId, Guid imageId);
        Task<bool> IsImageLikedByUserAsync(Guid userId, Guid imageId);
        Task<int> GetLikeCountAsync(Guid imageId);


    }
}
