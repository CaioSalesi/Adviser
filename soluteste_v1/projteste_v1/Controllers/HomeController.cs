using Microsoft.AspNetCore.Mvc;
using projteste_v1.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace projteste_v1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Index()
        {
            if (TempData["sucesso"] == null) { TempData["sucesso"] = false; }
            return View();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["erro"] = true;
                return RedirectToAction("Index");
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            var logsPath = Path.Combine(_environment.WebRootPath, "logs");

            // Ensure the upload and log directories exist
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            // Determine the next file name
            var files = Directory.GetFiles(uploadsPath);
            int maxNumber = files.Select(f => Path.GetFileNameWithoutExtension(f))
                                 .Where(name => int.TryParse(name, out _))
                                 .Select(int.Parse)
                                 .DefaultIfEmpty(0)
                                 .Max();
            int nextNumber = maxNumber + 1;
            var fileName = $"{nextNumber:D8}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(uploadsPath, fileName);

            // Save the uploaded file
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update the files list and max number for logging success
            files = Directory.GetFiles(uploadsPath);
            maxNumber = files.Select(f => Path.GetFileNameWithoutExtension(f))
                             .Where(name => int.TryParse(name, out _))
                             .Select(int.Parse)
                             .DefaultIfEmpty(0)
                             .Max();

            if (nextNumber == maxNumber)
            {
                TempData["sucesso"] = true;
            }
            else
            {
                TempData["sucesso"] = false;
            }

            // Create a log entry
            var logFilePath = Path.Combine(logsPath, "upload_log.txt");
            var logMessage = $"File uploaded: {fileName} at {DateTime.Now}\n";

            // Write the log entry
            await System.IO.File.AppendAllTextAsync(logFilePath, logMessage);

            return RedirectToAction("Index");
        }
    }
}
