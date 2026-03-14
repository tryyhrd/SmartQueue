using Microsoft.EntityFrameworkCore;
using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Models;

namespace SmartQueue.Data.Services
{
    public class DbService: IService, ITicket, IVisitor
    {
        public readonly Common.SmartQueueContext _context;
        public DbService(Common.SmartQueueContext context)
        {
            _context = context;
        }
        public IEnumerable<Service> Services => _context.Services;
        public IEnumerable<Ticket> Tickets => _context.Tickets.Include(x => x.Service);
        public IEnumerable<Visitor> Visitors => _context.Visitors;
        public async Task AddVisitorAsync(Visitor visitor)
        {
            _context.Visitors.Add(visitor);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Талоны
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public async Task AddTicketAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task AddServiceAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }
    }
}
