using Microsoft.AspNetCore.SignalR;

namespace SmartQueue.Hubs
{
    
    public class QueueHub: Hub
    {
        public class TicketUpdate
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string ServiceName { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? CompledetAt { get; set; }
        }

        public const string AdminGroup = "admin";
        public const string DisplayGroup = "displays";
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("OnConnected", groupName);
        }
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        public async Task NotifyAdmins(TicketUpdate update)
        {
            await Clients.Group(AdminGroup).SendAsync("OnTicketUpdated", update);
        }
        public async Task NotifyDisplays(TicketUpdate update)
        {
            await Clients.Group(DisplayGroup).SendAsync("OnTicketUpdated", update);
        }

        public async Task NotifyAll(TicketUpdate update)
        {
            await Clients.All.SendAsync("OnTicketUpdated", update);
        }
    }
}
