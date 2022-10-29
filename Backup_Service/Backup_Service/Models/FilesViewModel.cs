using System.Collections.Generic;
using System;

namespace Backup_Service.Models
{
    public class FileDetails
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get; set; }
    }

    public class FilesViewModel
    {
        public List<FileDetails> Files { get; set; }
            = new List<FileDetails>();
    }
}
