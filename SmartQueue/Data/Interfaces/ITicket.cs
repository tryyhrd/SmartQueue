using SmartQueue.Data.Models;

namespace SmartQueue.Data.Interfaces
{
    public interface ITicket
    {
        public IEnumerable<Ticket> Ticket { get; }
    }
}
