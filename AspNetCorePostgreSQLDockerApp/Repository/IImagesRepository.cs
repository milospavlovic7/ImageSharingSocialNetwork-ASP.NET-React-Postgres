using AspNetCorePostgreSQLDockerApp.Models;
using AspNetCorePostgreSQLDockerApp.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public interface IImagesRepository
    {
        Task<ImageDTO> GetImageByIdAsync(Guid imageId); // Vraća sliku po ID-u
        Task<IEnumerable<ImageDTO>> GetImagesByUserIdAsync(Guid userId); // Vraća slike za određenog korisnika
        Task<IEnumerable<ImageDTO>> GetImagesByPageAsync(int page, int pageSize); //Vrace sva slike sa paginacijom
        Task AddImageAsync(Image image); // Dodaje novu sliku
        Task UpdateImageDescriptionAsync(Guid imageId, string description); // Ažurira postojeću sliku
        Task DeleteImageAsync(Guid imageId); // Briše sliku
    }

}
