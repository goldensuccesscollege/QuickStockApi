using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Profile.DTO_s;
using QuickStock.Domain.ITassets;
using QuickStock.Infrastructure.Data;

namespace QuickStock.Applications.Profile.Handler
{
    public class GetProfileHandler
    {
        private readonly AppDbContext _context;

        public GetProfileHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileResponseDto?> Handle(int accountId)
        {
            var user = await _context.Accounts
                .Include(a => a.Profile)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (user == null) return null;

            return MapToDto(user);
        }

        public async Task<ProfileResponseDto?> GetByUsername(string username)
        {
            var user = await _context.Accounts
                .Include(a => a.Profile)
                .FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower());

            if (user == null) return null;

            return MapToDto(user);
        }

        private ProfileResponseDto MapToDto(Account user)
        {
            return new ProfileResponseDto
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.Profile?.FirstName ?? string.Empty,
                LastName = user.Profile?.LastName ?? string.Empty,
                Birthday = user.Profile?.Birthday,
                Address = user.Profile?.Address,
                PhoneNumber = user.Profile?.PhoneNumber,
                ImageProfilePath = user.Profile?.ImageProfilePath
            };
        }
    }
}
