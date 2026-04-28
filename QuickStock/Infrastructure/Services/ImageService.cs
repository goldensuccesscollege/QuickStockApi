using QuickStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuickStock.Domain.ITassets;

namespace QuickStock.Infrastructure.Services
{
    public interface IImageService
    {
        Task<int> UploadImageAsync(IFormFile file);
        Task<StoredImage?> GetImageAsync(int id);
    }

    public class ImageService : IImageService
    {
        private readonly AppDbContext _context;

        public ImageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var storedImage = new StoredImage
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Data = memoryStream.ToArray(),
                UploadedAt = DateTime.UtcNow
            };

            _context.StoredImages.Add(storedImage);
            await _context.SaveChangesAsync();

            return storedImage.Id;
        }

        public async Task<StoredImage?> GetImageAsync(int id)
        {
            return await _context.StoredImages.FindAsync(id);
        }
    }
}
