using Backup_Service.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backup_Service.Services
{
    public interface IUploadFiles
    {
        public Task<List<FileModel>> AddFiles(IFormFileCollection uploads, string fileName, string path);
    }
}
