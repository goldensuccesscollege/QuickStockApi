using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Users.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Users.Handler
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, object>
    {
        private readonly AppDbContext _context;

        public CreateUserHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            if (await _context.Accounts.AnyAsync(a => a.Username == dto.Username, cancellationToken))
                throw new InvalidOperationException("Username already exists");

            if (await _context.Accounts.AnyAsync(a => a.Email == dto.Email, cancellationToken))
                throw new InvalidOperationException("Email already exists");

            var account = new Account
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = QuickStock.Infrastructure.Security.PasswordHelper.HashPassword(dto.Password),
                Status = "Active",
                Verified = DateTime.UtcNow,
                CanAccessITAssets = dto.CanAccessITAssets,
                CanAccessApparel = dto.CanAccessApparel,
                CanAccessMessages = dto.CanAccessMessages,
                CanAccessLibrary = dto.CanAccessLibrary,
                CanAccessHomeEconomics = dto.CanAccessHomeEconomics,
                CanAccessConsumables = dto.CanAccessConsumables,
                Profile = new QuickStock.Domain.Social.Profile
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                }
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);

            return new { message = "User created successfully" };
        }
    }

    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
 
        public UpdateUserHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && int.TryParse(currentUserId, out int requesterId) && requesterId == request.Id)
            {
                throw new InvalidOperationException("You cannot modify your own core account details from User Management. Please use your Profile page.");
            }

            var account = await _context.Accounts.Include(a => a.Profile)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (account == null) return false;

            var dto = request.Dto;
            account.Email = dto.Email;
            account.Role = dto.Role;
            account.Username = dto.Username;
            account.CanAccessITAssets = dto.CanAccessITAssets;
            account.CanAccessApparel = dto.CanAccessApparel;
            account.CanAccessMessages = dto.CanAccessMessages;
            account.CanAccessLibrary = dto.CanAccessLibrary;
            account.CanAccessHomeEconomics = dto.CanAccessHomeEconomics;
            account.CanAccessConsumables = dto.CanAccessConsumables;

            if (account.Profile != null)
            {
                account.Profile.FirstName = dto.FirstName;
                account.Profile.LastName = dto.LastName;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteUserHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && int.TryParse(currentUserId, out int requesterId) && requesterId == request.Id)
            {
                throw new InvalidOperationException("You cannot delete your own account while logged in.");
            }

            var account = await _context.Accounts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (account == null) return false;

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class ToggleUserStatusHandler : IRequestHandler<ToggleUserStatusCommand, object>
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
 
        public ToggleUserStatusHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<object> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && int.TryParse(currentUserId, out int requesterId) && requesterId == request.Id)
            {
                throw new InvalidOperationException("You cannot disable your own account.");
            }

            var account = await _context.Accounts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (account == null) throw new KeyNotFoundException("User not found.");

            account.Status = account.Status == "Active" ? "Disabled" : "Active";
            await _context.SaveChangesAsync(cancellationToken);
            return new { status = account.Status };
        }
    }

    public class AddCampusAccessHandler : IRequestHandler<AddCampusAccessCommand, bool>
    {
        private readonly AppDbContext _context;

        public AddCampusAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AddCampusAccessCommand request, CancellationToken cancellationToken)
        {
            var exists = await _context.AccountCampuses
                .AnyAsync(ac => ac.AccountId == request.UserId && ac.CampusId == request.CampusId, cancellationToken);

            if (exists) return true;

            _context.AccountCampuses.Add(new AccountCampus
            {
                AccountId = request.UserId,
                CampusId = request.CampusId
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class RemoveCampusAccessHandler : IRequestHandler<RemoveCampusAccessCommand, bool>
    {
        private readonly AppDbContext _context;

        public RemoveCampusAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RemoveCampusAccessCommand request, CancellationToken cancellationToken)
        {
            var mapping = await _context.AccountCampuses
                .FirstOrDefaultAsync(ac => ac.AccountId == request.UserId && ac.CampusId == request.CampusId, cancellationToken);

            if (mapping == null) return false;

            _context.AccountCampuses.Remove(mapping);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class ToggleCampusBlockHandler : IRequestHandler<ToggleCampusBlockCommand, object>
    {
        private readonly AppDbContext _context;

        public ToggleCampusBlockHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(ToggleCampusBlockCommand request, CancellationToken cancellationToken)
        {
            var mapping = await _context.AccountCampuses
                .FirstOrDefaultAsync(ac => ac.AccountId == request.UserId && ac.CampusId == request.CampusId, cancellationToken);

            if (mapping == null) throw new KeyNotFoundException("Campus mapping not found.");

            mapping.IsBlocked = !mapping.IsBlocked;
            await _context.SaveChangesAsync(cancellationToken);
            return new { isBlocked = mapping.IsBlocked };
        }
    }

    public class ToggleITAccessHandler : IRequestHandler<ToggleITAccessCommand, object>
    {
        private readonly AppDbContext _context;

        public ToggleITAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(ToggleITAccessCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (account == null) throw new KeyNotFoundException("User not found.");

            account.CanAccessITAssets = !account.CanAccessITAssets;
            await _context.SaveChangesAsync(cancellationToken);
            return new { canAccessITAssets = account.CanAccessITAssets };
        }
    }

    public class ToggleApparelAccessHandler : IRequestHandler<ToggleApparelAccessCommand, object>
    {
        private readonly AppDbContext _context;

        public ToggleApparelAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(ToggleApparelAccessCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (account == null) throw new KeyNotFoundException("User not found.");

            account.CanAccessApparel = !account.CanAccessApparel;
            await _context.SaveChangesAsync(cancellationToken);
            return new { canAccessApparel = account.CanAccessApparel };
        }
    }

    public class ToggleMessageAccessHandler : IRequestHandler<ToggleMessageAccessCommand, object>
    {
        private readonly AppDbContext _context;

        public ToggleMessageAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(ToggleMessageAccessCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (account == null) throw new KeyNotFoundException("User not found.");

            account.CanAccessMessages = !account.CanAccessMessages;
            await _context.SaveChangesAsync(cancellationToken);
            return new { canAccessMessages = account.CanAccessMessages };
        }
    }

    public class ToggleLibraryAccessHandler : IRequestHandler<ToggleLibraryAccessCommand, object>
    {
        private readonly AppDbContext _context;

        public ToggleLibraryAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(ToggleLibraryAccessCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (account == null) throw new KeyNotFoundException("User not found.");

            account.CanAccessLibrary = !account.CanAccessLibrary;
            await _context.SaveChangesAsync(cancellationToken);
            return new { canAccessLibrary = account.CanAccessLibrary };
        }
    }

    public class ToggleHomeEconomicsAccessHandler : IRequestHandler<ToggleHomeEconomicsAccessCommand, object>
    {
        private readonly AppDbContext _context;

        public ToggleHomeEconomicsAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(ToggleHomeEconomicsAccessCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (account == null) throw new KeyNotFoundException("User not found.");

            account.CanAccessHomeEconomics = !account.CanAccessHomeEconomics;
            await _context.SaveChangesAsync(cancellationToken);
            return new { canAccessHomeEconomics = account.CanAccessHomeEconomics };
        }
    }
}
