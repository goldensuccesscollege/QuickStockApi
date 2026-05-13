using Microsoft.AspNetCore.SignalR;
using QuickStock.Domain;
using QuickStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
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

namespace QuickStock.Controllers
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public ChatHub(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var groupIds = await _context.ChatGroupMembers
                    .Where(gm => gm.Account.Username == username)
                    .Select(gm => gm.ChatGroupId.ToString())
                    .ToListAsync();

                foreach (var groupId in groupIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Group_" + groupId);
                }
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string message, string? receiverUsername = null, int? groupId = null)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername)) return;

            // Viewer restriction: Cannot create (send) messages
            if (Context.User.IsInRole("Viewer")) return;

            var sender = await _context.Accounts
                .Include(a => a.Profile)
                .FirstOrDefaultAsync(a => a.Username == senderUsername);
            if (sender == null) return;

            var senderFullName = sender.Profile != null 
                ? $"{sender.Profile.FirstName} {sender.Profile.LastName}" 
                : sender.Username;
            
            var senderImagePath = sender.Profile?.ImageProfilePath;

            Account? receiver = null;
            if (!string.IsNullOrEmpty(receiverUsername))
            {
                receiver = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == receiverUsername);
            }

            var chatMessage = new ChatMessage
            {
                SenderAccountId = sender.Id,
                ReceiverAccountId = receiver?.Id,
                GroupId = groupId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var formattedTime = chatMessage.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

            if (groupId.HasValue)
            {
                // Group message
                await Clients.Group("Group_" + groupId.Value).SendAsync("ReceiveMessage", senderUsername, senderFullName, message, formattedTime, false, senderImagePath, groupId);
                
                // Notify group members
                await _notificationService.NotifyGroup(groupId.Value, $"New Message in group from {senderFullName}", message);
            }
            else if (receiver != null)
            {
                // Private message
                await Clients.User(receiver.Username).SendAsync("ReceiveMessage", senderUsername, senderFullName, message, formattedTime, true, senderImagePath);
                
                // Notify the receiver via NotificationHub
                await _notificationService.NotifyUser(receiver.Username, $"New Message from {senderFullName}", message);

                if (senderUsername != receiver.Username)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", senderUsername, senderFullName, message, formattedTime, true, senderImagePath);
                }
            }
            else
            {
                // Global message
                await Clients.All.SendAsync("ReceiveMessage", senderUsername, senderFullName, message, formattedTime, false, senderImagePath);
            }
        }
    }
}
