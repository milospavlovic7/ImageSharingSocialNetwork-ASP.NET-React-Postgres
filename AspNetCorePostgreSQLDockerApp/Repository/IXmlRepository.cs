using System.Threading.Tasks;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public interface IXmlRepository
    {
        Task<string> GetXmlDataAsync(string key);
        Task SaveXmlDataAsync(string key, string xmlData);
    }
}
