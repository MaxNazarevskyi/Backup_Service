using Backup_Service.Data.DataModels;
using Backup_Service.Data.Repository;
using Backup_Service.Extensions;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Backup_Service.Services
{
    public class LoginService : ILoginService
    {
        private readonly IRepository<User> _repository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LoginService(IRepository<User> repository, IPasswordHasher<User> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }
        public async Task<User> Register(string username, string password)
        {
            if (await _repository.SingleOrDefaultAsync(u => u.Username == username) is not null)
            {
                return null;
            }
            var user = new User
            {
                Username = username
            };
            user.Password = _passwordHasher.HashPassword(user, password);

            await _repository.AddAsync(user);

            return user;
        }
        public async Task<User> CheckLogin(string username, string password)
        {
            if (await _repository.SingleOrDefaultAsync(u => u.Username == username) is not User user)
            {
                return null;
            }

            if (_passwordHasher.VerifyHashedPassword(user, user.Password, password) != PasswordVerificationResult.Success)
            {
                return null;
            }
            return user;
        }
        public async Task<User> CheckToken(string token)
        {
            var userId = int.Parse(token.Split(":")[0]);
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            if (CreateToken(userId, user.Password) != token)
            {
                return null;
            }

            return user;
        }

        public string CreateToken(int userId, string password)
        {
            return $"{userId}:{password.GenerateSHA256Hash()}";
        }
    }
}
