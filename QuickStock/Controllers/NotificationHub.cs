using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QuickStock.Controllers
{
    public class NotificationHub : Hub
    {
        public async Task JoinCampus(int campusId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Campus_" + campusId);
        }

        public async Task LeaveCampus(int campusId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Campus_" + campusId);
        }
    }
}
