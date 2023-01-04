using Backup_Service.Data.DataModels;

namespace Backup_Service.Services
{
    public interface IUserService
    {
        public User GetUserById(int id);

    }
}
