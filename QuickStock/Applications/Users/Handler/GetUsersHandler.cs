using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Users.Queries;
using QuickStock.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Users.Handler
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, IEnumerable<object>>
    {
        private readonly AppDbContext _context;

        public GetUsersHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Accounts
                .Select(a => new
                {
                    a.Id,
                    a.Username,
                    a.Email,
                    a.Role,
                    Status = a.Status ?? "Active",
                    FirstName = a.Profile != null ? a.Profile.FirstName : "",
                    LastName = a.Profile != null ? a.Profile.LastName : "",
                    a.CanAccessITAssets,
                    a.CanAccessApparel,
                    a.CanAccessMessages,
                    a.CanAccessLibrary,
                    a.CanAccessHomeEconomics,
                    a.CanAccessConsumables,
                    Campuses = a.AccountCampuses.Select(ac => new
                    {
                        ac.CampusId,
                        Name = ac.Campus != null ? ac.Campus.Name : "Unknown",
                        ac.IsBlocked
                    }).ToList()
                })
                .ToListAsync<object>(cancellationToken);
        }
    }
}
