using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class FilesController : Controller
    {
        private readonly FileStorageService _fileStorageService;

        public FilesController(
            FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var files =
                await _fileStorageService.GetFilesAsync();

            return View(files);
        }

        public async Task<IActionResult> Download(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            var stream =
                await _fileStorageService
                    .DownloadFileAsync(fileName);

            if (stream == null)
            {
                return NotFound();
            }

            return File(
                stream,
                "text/plain",
                fileName);
        }
    }
}