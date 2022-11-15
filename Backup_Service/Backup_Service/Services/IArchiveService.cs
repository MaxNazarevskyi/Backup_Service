using Backup_Service.Models;

namespace Backup_Service.Services
{
    public interface IArchiveService
    {
        public ArchivesModel GetArchives();
    }
}
