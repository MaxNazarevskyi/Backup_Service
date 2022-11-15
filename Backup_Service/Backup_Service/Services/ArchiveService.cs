using Backup_Service.Models;
using System.Collections.Generic;
using System.IO;

namespace Backup_Service.Services
{
    public class ArchiveService : IArchiveService
    {
        public ArchivesModel GetArchives()
        {
            var model = new ArchivesModel();
            model.Archives = new List<ArchivesParams>();

            foreach (var item in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups")))
            {
                model.Archives.Add(
                    new ArchivesParams { ArchiveName = Path.GetFileName(item), ArchivePath = item });
            }
            return model;
        }
    }
}
