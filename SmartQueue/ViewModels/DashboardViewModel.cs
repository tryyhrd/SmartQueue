using SmartQueue.Data.Models;

namespace SmartQueue.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<Ticket> WaitingTickets { get; set; } 
        public IEnumerable<Ticket> ActiveTickets { get; set; } 
        public IEnumerable<Ticket> CompletedTickets { get; set; }
    }
}
