using Backup_Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Backup_Service.Services;

namespace Backup_Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IArchiveService _archiveService;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment, IArchiveService archiveService)
        {
            _archiveService = archiveService;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            // Get files from the server
            var model = new FilesViewModel();
            foreach (var item in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload")))
            {
                model.Files.Add(
                    new FileDetails { Name = Path.GetFileName(item), Path = item });
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(IFormFile[] files)
        {
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                using (var localFile = System.IO.File.OpenWrite(filePath))
                using (var uploadedFile = file.OpenReadStream())
                {
                    uploadedFile.CopyTo(localFile);
                }
            }
            ViewBag.Message = "Files are successfully uploaded";

            // Get files from the server
            var model = new FilesViewModel();
            foreach (var item in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload")))
            {
                model.Files.Add(
                    new FileDetails { Name = Path.GetFileName(item), Path = item });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult DownloadArchive(int compressionLevel)
        {
            var FolderPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPath);
            var PathToFiles = Path.Combine(FolderPath + FilePaths);

            using ZipOutputStream zipOutputStream = new ZipOutputStream(System.IO.File.Create(Path.Combine(PathToFiles)));
            {
                zipOutputStream.SetLevel(compressionLevel);

                byte[] buffer = new byte[4094];

                foreach (var FilePath in FilePaths)
                {
                    var FileName = Path.GetFileName(FilePath);
                    var CleanName = ZipEntry.CleanName(FileName);
                    ZipEntry entry = new ZipEntry(CleanName);

                    entry.DateTime = DateTime.Now;
                    entry.IsUnicodeText = true;
                    zipOutputStream.PutNextEntry(entry);
                    using (FileStream fileStream = System.IO.File.OpenRead(FilePath))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fileStream.Read(buffer, 0, buffer.Length);
                            zipOutputStream.Write(buffer, 0, sourceBytes);

                        } while (sourceBytes > 0);
                    }
                    zipOutputStream.CloseEntry();
                }
                zipOutputStream.Finish();
                zipOutputStream.Flush();
                zipOutputStream.Close();
            }
            DeletingTemp();
            CreatingBackup();
            var archives = _archiveService.GetArchives();
            return View("Backups", archives);
        }

        public IActionResult GetBackups()
        {
            var archives = _archiveService.GetArchives();
            return View("Backups", archives);
        }
        public void CreatingBackup()
        {
            var fileName = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip";
            var FolderPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPath);
            var PathToFiles = Path.Combine(FolderPath + FilePaths);

            byte[] finalResult = System.IO.File.ReadAllBytes(PathToFiles);
            if (System.IO.File.Exists(PathToFiles))
            {
                System.IO.File.Delete(PathToFiles);
            }
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups", fileName);
            System.IO.File.WriteAllBytes(filePath, finalResult);    //Creating backup
        }
        public void DeletingTemp()
        {
            var FolderPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPath);

            foreach (string file in FilePaths)
                System.IO.File.Delete(file);    //Deleting Uploads
        }

        public void OpenFolder()
        {
            var filePath = @"D:\Projects\Backup_Service\Backup_Service\Backup_Service\wwwroot\Backups";

            Process.Start("explorer.exe", string.Format("\"{0}\"", filePath));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}