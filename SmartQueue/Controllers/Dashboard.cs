using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;
using SmartQueue.ViewModels;

namespace SmartQueue.Controllers
{
    public class Dashboard(ITicket ticket) : Controller
    {
        private readonly ITicket _ticket = ticket;

        public IActionResult Display()
        {
            var tickets = _ticket.Tickets.ToList();

            var model = new DashboardViewModel
            {
                WaitingTickets = tickets.Where(t => t.Status == Data.Models.Ticket.StatusType.Waiting)
                    .OrderBy(t => t.CreatedAt),
                ActiveTickets = tickets.Where(t => t.Status == Data.Models.Ticket.StatusType.Active)
                    .OrderByDescending(t => t.CreatedAt),
                CompletedTickets = tickets.Where(t => t.Status == Data.Models.Ticket.StatusType.Completed)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
            };

            return View(model);
        }
    }
}
