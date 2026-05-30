using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Rooms.Command;
using QuickStock.Applications.Rooms.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuickStock.Common.Exceptions;

namespace QuickStock.Applications.Rooms.Handler
{
    public class GetRoomsHandler : IRequestHandler<GetRoomsQuery, IEnumerable<Room>>
    {
        private readonly AppDbContext _context;

        public GetRoomsHandler(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Room>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Room> query = _context.Rooms;

            if (!request.User.IsInRole("Admin"))
                query = query.Where(r => !r.IsDisabled);

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
                query = query.Where(r => r.CampusId == request.CampusId.Value);

            return await query.ToListAsync(cancellationToken);
        }
    }

    public class GetRoomByIdHandler : IRequestHandler<GetRoomByIdQuery, Room?>
    {
        private readonly AppDbContext _context;

        public GetRoomByIdHandler(AppDbContext context) => _context = context;

        public async Task<Room?> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms.FindAsync(new object[] { request.Id }, cancellationToken);
            if (room == null) return null;
            if (room.IsDisabled && !request.User.IsInRole("Admin")) return null;
            return room;
        }
    }

    public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, Room>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public CreateRoomHandler(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Room> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            var room = request.Room;
            var campus = await _context.Campuses.FindAsync(new object[] { room.CampusId }, cancellationToken);
            if (campus == null) throw new InvalidOperationException("Campus not found.");

            if (string.Equals(room.RoomName.Trim(), campus.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Room name cannot be the same as the campus name.");

            var roomExists = await _context.Rooms.AnyAsync(r =>
                r.CampusId == room.CampusId && r.RoomName.ToLower() == room.RoomName.ToLower(), cancellationToken);

            if (roomExists) throw new InvalidOperationException("A room with this name already exists in this campus.");

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "Add", room.RoomId, room.RoomName, $"Created room: {room.RoomName} on {room.RoomFloor}", room.CampusId, room.IsDisabled ? "Disabled" : "Enabled");
            
            await _notificationService.NotifyCampusActivity(room.CampusId, "New Room Added", 
                $"{request.User.Identity?.Name} created room {room.RoomName}", "Success");
            return room;
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Action = action, EntityType = "Room", EntityId = entityId, EntityName = entityName,
                Details = details, UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = user.Identity?.Name, CampusId = campusId, Status = status
            });
            await _context.SaveChangesAsync();
        }
    }

    public class UpdateRoomHandler : IRequestHandler<UpdateRoomCommand, bool>
    {
        private readonly AppDbContext _context;

        public UpdateRoomHandler(AppDbContext context) => _context = context;

        public async Task<bool> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            var room = request.Room;
            var campus = await _context.Campuses.FindAsync(new object[] { room.CampusId }, cancellationToken);
            if (campus == null) throw new InvalidOperationException("Campus not found.");

            if (string.Equals(room.RoomName.Trim(), campus.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Room name cannot be the same as the campus name.");

            var roomExists = await _context.Rooms.AnyAsync(r =>
                r.CampusId == room.CampusId && r.RoomName.ToLower() == room.RoomName.ToLower() && r.RoomId != request.Id,
                cancellationToken);

            if (roomExists) throw new InvalidOperationException("A room with this name already exists in this campus.");

            _context.Entry(room).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "Update", room.RoomId, room.RoomName, "Updated room details", room.CampusId, room.IsDisabled ? "Disabled" : "Enabled");
            return true;
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Action = action, EntityType = "Room", EntityId = entityId, EntityName = entityName,
                Details = details, UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = user.Identity?.Name, CampusId = campusId, Status = status
            });
            await _context.SaveChangesAsync();
        }
    }

    public class ToggleRoomStatusHandler : IRequestHandler<ToggleRoomStatusCommand, object>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public ToggleRoomStatusHandler(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<object> Handle(ToggleRoomStatusCommand request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms.FindAsync(new object[] { request.Id }, cancellationToken);
            if (room == null) throw new KeyNotFoundException("Room not found.");

            room.IsDisabled = !room.IsDisabled;
            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "ToggleStatus", room.RoomId, room.RoomName,
                $"Room status changed to {(room.IsDisabled ? "Disabled" : "Enabled")}", room.CampusId, room.IsDisabled ? "Disabled" : "Enabled");
            
            await _notificationService.NotifyCampusActivity(room.CampusId, "Room Status Updated", 
                $"{request.User.Identity?.Name} {(room.IsDisabled ? "Disabled" : "Enabled")} room {room.RoomName}", "Warning");
            return new { isDisabled = room.IsDisabled };
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Action = action, EntityType = "Room", EntityId = entityId, EntityName = entityName,
                Details = details, UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = user.Identity?.Name, CampusId = campusId, Status = status
            });
            await _context.SaveChangesAsync();
        }
    }

    public class DeleteRoomHandler : IRequestHandler<DeleteRoomCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public DeleteRoomHandler(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms.FindAsync(new object[] { request.Id }, cancellationToken);
            if (room == null) return false;

            // Check for associated IT Assets
            if (await _context.Itassets.AnyAsync(a => a.RoomId == request.Id, cancellationToken))
            {
                throw new BadRequestException("Cannot delete room because it contains IT assets.");
            }

            var roomName = room.RoomName;
            var campusId = room.CampusId;
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "Delete", request.Id, roomName, $"Deleted room: {roomName}", campusId, room.IsDisabled ? "Disabled" : "Enabled");
            
            await _notificationService.NotifyCampusActivity(campusId, "Room Deleted", 
                $"{request.User.Identity?.Name} deleted room {roomName}", "Danger");
            return true;
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Action = action, EntityType = "Room", EntityId = entityId, EntityName = entityName,
                Details = details, UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = user.Identity?.Name, CampusId = campusId, Status = status
            });
            await _context.SaveChangesAsync();
        }
    }
}
