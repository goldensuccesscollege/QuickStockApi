using Microsoft.AspNetCore.Mvc;
using QuickStock.Infrastructure.Services;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                var imageId = await _imageService.UploadImageAsync(file);
                return Ok(new { id = imageId, message = "Image uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var image = await _imageService.GetImageAsync(id);
            if (image == null) return NotFound();

            return File(image.Data, image.ContentType);
        }
    }
}
