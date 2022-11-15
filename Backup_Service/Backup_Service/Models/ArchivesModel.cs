using System.Collections.Generic;

namespace Backup_Service.Models
{
    public class ArchivesParams
    {
        public string ArchiveName { get; set; }
        public string ArchivePath { get; set; }
    }
    public class ArchivesModel
    {
        public List<ArchivesParams> Archives { get; set; }
            = new List<ArchivesParams>();
    }
}
