using Backup_Service.Models;
using Backup_Service.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Backup_Service.Data
{
    public class UploadFiles : IUploadFiles
    {
        public async Task<List<FileModel>> AddFiles(IFormFileCollection uploads, string fileName, string path)
        {
            List<FileModel> filesList = new List<FileModel>();
            foreach (var uploadedFile in uploads)
            {
                path = "D:/Projects/Backup_Service/Backup_Service/Backup_Service/wwwroot/Files/" + uploadedFile.FileName;
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path };
                filesList.Add(file);
            }
            return filesList;
        }
    }
}
