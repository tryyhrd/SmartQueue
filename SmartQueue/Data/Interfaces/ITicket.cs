using SmartQueue.Data.Models;

namespace SmartQueue.Data.Interfaces
{
    public interface ITicket
    {
        IEnumerable<Ticket> Tickets { get; }
        Task AddTicketAsync(Ticket ticket);
    }
}
