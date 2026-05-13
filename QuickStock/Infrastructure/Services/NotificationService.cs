using Microsoft.AspNetCore.SignalR;
using QuickStock.Controllers;
using System.Threading.Tasks;

namespace QuickStock.Infrastructure.Services
{
    public interface INotificationService
    {
        Task NotifyCampusActivity(int campusId, string title, string message, string type = "Info");
        Task NotifyUser(string username, string title, string message, string type = "Chat");
        Task NotifyGroup(int groupId, string title, string message, string type = "GroupChat");
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyCampusActivity(int campusId, string title, string message, string type = "Info")
        {
            await _hubContext.Clients.Group("Campus_" + campusId).SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = System.DateTime.UtcNow
            });
        }

        public async Task NotifyUser(string username, string title, string message, string type = "Chat")
        {
            await _hubContext.Clients.User(username).SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = System.DateTime.UtcNow
            });
        }

        public async Task NotifyGroup(int groupId, string title, string message, string type = "GroupChat")
        {
            // Usually we'd look up members, but for simplicity we can use a group in NotificationHub
            await _hubContext.Clients.Group("Group_" + groupId).SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = System.DateTime.UtcNow
            });
        }
    }
}
