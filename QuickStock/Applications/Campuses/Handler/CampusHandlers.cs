using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Campuses.Command;
using QuickStock.Applications.Campuses.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuickStock.Common.Exceptions;

namespace QuickStock.Applications.Campuses.Handler
{
    public class GetCampusesHandler : IRequestHandler<GetCampusesQuery, IEnumerable<Campus>>
    {
        private readonly AppDbContext _context;
        public GetCampusesHandler(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Campus>> Handle(GetCampusesQuery request, CancellationToken cancellationToken)
            => await _context.Campuses.ToListAsync(cancellationToken);
    }

    public class GetCampusByIdHandler : IRequestHandler<GetCampusByIdQuery, Campus?>
    {
        private readonly AppDbContext _context;
        public GetCampusByIdHandler(AppDbContext context) => _context = context;

        public async Task<Campus?> Handle(GetCampusByIdQuery request, CancellationToken cancellationToken)
            => await _context.Campuses.FindAsync(new object[] { request.Id }, cancellationToken);
    }

    public class CreateCampusHandler : IRequestHandler<CreateCampusCommand, Campus>
    {
        private readonly AppDbContext _context;
        public CreateCampusHandler(AppDbContext context) => _context = context;

        public async Task<Campus> Handle(CreateCampusCommand request, CancellationToken cancellationToken)
        {
            _context.Campuses.Add(request.Campus);
            await _context.SaveChangesAsync(cancellationToken);
            return request.Campus;
        }
    }

    public class UpdateCampusHandler : IRequestHandler<UpdateCampusCommand, bool>
    {
        private readonly AppDbContext _context;
        public UpdateCampusHandler(AppDbContext context) => _context = context;

        public async Task<bool> Handle(UpdateCampusCommand request, CancellationToken cancellationToken)
        {
            var existingCampus = await _context.Campuses.FindAsync(new object[] { request.Campus.CampusId }, cancellationToken);
            if (existingCampus == null) return false;

            existingCampus.Name = request.Campus.Name;
            existingCampus.Address = request.Campus.Address;
            existingCampus.Description = request.Campus.Description;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteCampusHandler : IRequestHandler<DeleteCampusCommand, bool>
    {
        private readonly AppDbContext _context;
        public DeleteCampusHandler(AppDbContext context) => _context = context;

        public async Task<bool> Handle(DeleteCampusCommand request, CancellationToken cancellationToken)
        {
            var campus = await _context.Campuses.FindAsync(new object[] { request.Id }, cancellationToken);
            if (campus == null) return false;

            // Check for associated Rooms
            if (await _context.Rooms.AnyAsync(r => r.CampusId == request.Id, cancellationToken))
            {
                throw new BadRequestException("Cannot delete campus because it has associated rooms.");
            }

            // Check for associated IT Assets
            if (await _context.Itassets.AnyAsync(a => a.CampusId == request.Id, cancellationToken))
            {
                throw new BadRequestException("Cannot delete campus because it has associated IT assets.");
            }

            // Check for associated Apparel
            if (await _context.ApparelList.AnyAsync(ap => ap.CampusId == request.Id, cancellationToken))
            {
                throw new BadRequestException("Cannot delete campus because it has associated apparel.");
            }

            _context.Campuses.Remove(campus);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
