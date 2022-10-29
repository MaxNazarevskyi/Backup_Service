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

namespace Backup_Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment hostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            this.hostEnvironment = hostEnvironment;
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
            // Iterate each files
            foreach (var file in files)
            {
                // Get the file name from the browser
                var fileName = Path.GetFileName(file.FileName);
        
                // Get file path to be uploaded
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload", fileName);
        
                // Check If file with same name exists and delete it
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
        
                // Create a new local file and copy contents of uploaded file
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
        public async Task DownloadArchive()
        {
            //string zipName = String.Format("Zip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            Response.ContentType = "application/octet-stream";
            Response.Headers.Add("Content-Disposition","attachment; filename=\"MyFiles.zip\"");

            var FolderPath = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPath);
            using (ZipArchive archive = new ZipArchive(Response.BodyWriter.AsStream(), ZipArchiveMode.Create))
            {
                foreach (var FilePath in FilePaths)
                {
                    var botFileName = Path.GetFileName(FilePath);
                    var entry = archive.CreateEntry(botFileName);
                    using (var entryStream = entry.Open())
                    using (var fileStream = System.IO.File.OpenRead(FilePath))
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }
        }

        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null)
                return Content("filename is not availble");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".mp3", "music/mp3"}
            };
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
