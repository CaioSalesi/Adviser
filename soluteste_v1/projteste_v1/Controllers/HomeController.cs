using Microsoft.AspNetCore.Mvc;
using projteste_v1.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

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
            return View();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Content("Arquivo não selecionado");
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");

            var files = Directory.GetFiles(uploadsPath);
            int maxNumber = files.Select(f => Path.GetFileNameWithoutExtension(f))
                                 .Where(name => int.TryParse(name, out _))
                                 .Select(int.Parse)
                                 .DefaultIfEmpty(0)
                                 .Max();
            int nextNumber = maxNumber + 1;
            var fileName = $"{nextNumber:D8}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return RedirectToAction("Index"); // ou outra ação relevante
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
