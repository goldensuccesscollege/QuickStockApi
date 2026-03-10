using QuickStock.Applications.Profile.DTO_s;
using QuickStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace QuickStock.Applications.Profile.Handler
{
    public class UpdateProfileHandler
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UpdateProfileHandler(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<string> Handle(int accountId, UpdateProfileDto dto)
        {
            var profile = await _context.Profiles
                .FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (profile == null)
                return "Profile not found";

            // ---------- Update fields ----------
            profile.FirstName = dto.FirstName;
            profile.LastName = dto.LastName;
            profile.Birthday = dto.Birthday;
            profile.Address = dto.Address;
            profile.PhoneNumber = dto.PhoneNumber;
            profile.Updated = DateTime.UtcNow;

            // ---------- Upload Image ----------
            if (dto.ImageProfile != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "Upload", "Picture");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageProfile.FileName);

                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageProfile.CopyToAsync(stream);
                }

                profile.ImageProfilePath = "/Upload/Picture/" + fileName;
            }

            await _context.SaveChangesAsync();

            return "Profile updated successfully";
        }
    }
}
