using Backup_Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using ICSharpCode.SharpZipLib.Zip;
using Backup_Service.Services;

namespace Backup_Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IArchiveService _archiveService;
        private readonly ILoginService _loginService;
        private readonly IUserService _userService;


        public HomeController(
            IWebHostEnvironment hostEnvironment,
            IArchiveService archiveService,
            ILoginService loginService,
            IUserService userService)
        {
            _archiveService = archiveService;
            _hostEnvironment = hostEnvironment;
            _loginService = loginService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            string token = null;
            if (!Request.Cookies.TryGetValue("token", out token))
                return RedirectToAction("Login", "Authentication");

            var user = await _loginService.CheckToken(token);
            if (user == null)
                return RedirectToAction("Login", "Authentication");
            DeletingTemp();

            var model = new FilesViewModel();
            foreach (var item in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload")))
            {
                model.Files.Add(
                    new FileDetails { Name = Path.GetFileName(item), Path = item });
            }
            return View(model);
        }

        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824, ValueLengthLimit = Int32.MaxValue)]
        [HttpPost]
        public IActionResult Index(IList<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (file.FileName != null)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload", fileName);

                    long FileSizeKb = GetFilesSizeUpload();
                    long FileSizeMb = FileSizeKb / (1024 * 1024);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    using (var localFile = System.IO.File.OpenWrite(filePath))
                    using (var uploadedFile = file.OpenReadStream())
                    {
                        uploadedFile.CopyTo(localFile);
                    }
                    if (FileSizeMb > 1024)
                        return RedirectToAction("SizeYourFilesBiggerThanStorage");
                    ViewBag.MessageOK = "Files are successfully uploaded";
                }
                else { ViewBag.MessageNOT = "Files not found"; }
            }

            // Get files from the server
            var model = new FilesViewModel();
            foreach (var item in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload")))
            {
                model.Files.Add(
                    new FileDetails { Name = Path.GetFileName(item), Path = item });
            }
            return View(model);
        }
        public IActionResult FilesNotFound()
        {
            return View("FilesNotFound");
        }
        public void StorageIsFull()
        {
            DeletingTemp();
            ViewBag.MessageBackup = "Storage Is Full";
        }
        public long GetFilesSizeUpload()
        {
            var FolderPathBackup = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPathBackup);
            long FileSizeSum = 0;

            foreach (var FilePath in FilePaths)
            {
                long FileSize = new FileInfo(FilePath).Length;
                FileSizeSum += FileSize;
            }
            return FileSizeSum;
        }
        public long GetFilesSizeBackup()
        {
            var FolderPathBackup = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Backups");
            var FilePaths = Directory.GetFiles(FolderPathBackup);
            long FileSizeSum = 0;

            foreach (var FilePath in FilePaths)
            {
                long FileSize = new FileInfo(FilePath).Length;
                FileSizeSum += FileSize;
            }
            return FileSizeSum;
        }
        [HttpGet]
        public IActionResult CreatingArchive(int compressionLevel)
        { 
            var FolderPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPath);
            var PathToFiles = Path.Combine(FolderPath + FilePaths);

            if (Directory.GetFileSystemEntries(FolderPath).Length == 0)
                return RedirectToAction("FilesNotFound");

            long FileSizeKb = GetFilesSizeBackup();
            long FileSizeMb = FileSizeKb / (1024 * 1024);

            if (FileSizeMb > 1024)
                return RedirectToAction("StorageIsFull");

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

            return RedirectToAction("GetBackups");
        }
        public ArchivesModel GetArchives()
        {
            string token = null;
            Request.Cookies.TryGetValue("token", out token);
            var user = _userService.GetUserById(_loginService.GetUserId(token));
            long FileSizeSum = 0;
            long FileSizeMb = 0;
            var model = new ArchivesModel();
            model.Archives = new List<ArchivesParams>();

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups", user.Username));
            foreach (var item in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups", user.Username)))
            {
                long FileSize = new FileInfo(item).Length;
                FileSizeSum += FileSize;
                FileSizeMb = FileSizeSum / (1024 * 1024);

                model.Archives.Add(
                    new ArchivesParams { ArchiveName = Path.GetFileName(item), ArchivePath = item, ArchivesSize = FileSizeMb });
            }
            return model;
        }
        public IActionResult GetBackups()
        {
            var archives = GetArchives();
            return View("Backups", archives);
        }
        public void CreatingBackup()
        {
            string token = null;
            Request.Cookies.TryGetValue("token", out token);
            var user = _userService.GetUserById(_loginService.GetUserId(token));
            var fileName = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip";
            var FolderPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPath);
            var PathToFiles = Path.Combine(FolderPath + FilePaths);

            byte[] finalResult = System.IO.File.ReadAllBytes(PathToFiles);
            if (System.IO.File.Exists(PathToFiles))
            {
                System.IO.File.Delete(PathToFiles);
            }
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups", user.Username));
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups", user.Username, fileName);
            System.IO.File.WriteAllBytes(filePath, finalResult);    //Creating backup
        }
        [HttpGet]
        public async Task<IActionResult> Download(string filename)
        {
            string token = null;
            Request.Cookies.TryGetValue("token", out token);
            var user = _userService.GetUserById(_loginService.GetUserId(token));
            if (filename == null)
                return Content("filename is not availble");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups", user.Username, filename);

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
                {".zip", "application/zip"}
            };
        }
        public void DeletingTemp()
        {
            var FolderPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot/Upload");
            var FilePaths = Directory.GetFiles(FolderPath);

            foreach (string file in FilePaths)
                System.IO.File.Delete(file);    //Deleting Uploads
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