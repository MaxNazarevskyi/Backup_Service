using Backup_Service.Models;
using System.Collections.Generic;
using System.IO;

namespace Backup_Service.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly IUserService _userService;
        private readonly ILoginService _loginService;

        public ArchiveService(IUserService userService, ILoginService loginService)
        {
            _userService = userService;
            _loginService = loginService;
        }

        public ArchivesModel GetArchives()
        {
            //string token = null;
            //Request.Cookies.TryGetValue("token", out token);
            //var user = _userService.GetUserById(_loginService.GetUserId(token));
            long FileSizeSum = 0;
            long FileSizeMb = 0;
            var model = new ArchivesModel();
            model.Archives = new List<ArchivesParams>();

            foreach (var item in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups")))
            {
                long FileSize = new FileInfo(item).Length;
                FileSizeSum += FileSize;
                FileSizeMb = FileSizeSum / (1024 * 1024);
            
               model.Archives.Add(
                   new ArchivesParams { ArchiveName = Path.GetFileName(item), ArchivePath = item, ArchivesSize = FileSizeMb });
            }
            return model;
        }
    }
}
