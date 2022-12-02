using System.Threading.Tasks;
using Backup_Service.Data.DataModels;

namespace Backup_Service.Services
{
    public interface ILoginService
    {
        public Task<User> CheckLogin(string login, string password);
        public Task<User> Register(string login, string password);
        public Task<User> CheckToken(string token);
        public string CreateToken(int userId, string password);
    }
}
