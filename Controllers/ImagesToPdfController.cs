using ImagesToPdfApi.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace ImagesToPdfApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesToPdfController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;
        public ImagesToPdfController(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        [HttpPost("convert-to-pdf")]
        public async Task<IActionResult> ConvertToPdf([FromForm] IFormFileCollection files)
        {
            try
            {
                var pdfBytes = await _fileRepository.ConvertImagesToPdfAsync(files);
                return File(pdfBytes, "application/pdf", "generated.pdf");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
