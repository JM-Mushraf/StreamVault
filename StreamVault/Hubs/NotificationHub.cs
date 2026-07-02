using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ProjectFileStructure.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userGuid = Context.User?.FindFirst("UserGuid")?.Value;
            if (!string.IsNullOrEmpty(userGuid))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userGuid);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var userGuid = Context.User?.FindFirst("UserGuid")?.Value;
            if (!string.IsNullOrEmpty(userGuid))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userGuid);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
