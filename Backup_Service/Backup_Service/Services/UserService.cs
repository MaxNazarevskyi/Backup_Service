using Backup_Service.Data.DataModels;
using Backup_Service.Data.Repository;

namespace Backup_Service.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;

        public UserService(IRepository<User> repository)
        {
            _repository = repository;
        }
        public User GetUserById(int id)
        {
           return _repository.GetById(id);
        }
    }
}
