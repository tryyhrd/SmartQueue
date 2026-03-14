using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartQueue.Data.Interfaces;
using SmartQueue.Hubs;
using SmartQueue.ViewModels;
using static SmartQueue.Hubs.QueueHub;

namespace SmartQueue.Controllers
{
    public class AdminController(ITicket ticket, IService service, IHubContext<QueueHub> hubContext) : Controller
    {
        private readonly ITicket _ticket = ticket;
        private readonly IService _service = service;
        private readonly IHubContext<QueueHub> _hubContext = hubContext;

        public IActionResult Dashboard()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTicket(int id)
        {
            if (_ticket.Tickets.Any(x => x.Status == Data.Models.Ticket.StatusType.Active))
                return BadRequest(new {message = "Уже есть активный посетитель"});

            var ticket = _ticket.Tickets.FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(new { error = "Талон не найден" });
                return NotFound("Талон не найден");
            }

            var update = new TicketUpdate
            {
                Id = ticket.Id,
                Number = ticket.Number,
                ServiceName = ticket.Service?.Name,
                Status = "Active",  
                CreatedAt = ticket.CreatedAt
            };

            ticket.Status = Data.Models.Ticket.StatusType.Active;
            await _ticket.UpdateTicketAsync(ticket);

            await _hubContext.Clients.All.SendAsync("OnTicketUpdated", update);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Ok(new
                {
                    success = true,
                    ticketId = id,
                    message = "Талон принят"
                });
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTicket(int id)
        {
            var ticket = _ticket.Tickets.FirstOrDefault(t => t.Id == id);

            if (ticket != null)
            {
                var update = new TicketUpdate
                {
                    Id = ticket.Id,
                    Number = ticket.Number,
                    ServiceName = ticket.Service?.Name,
                    Status = "Completed",
                    CreatedAt = ticket.CreatedAt
                };

                ticket.Status = Data.Models.Ticket.StatusType.Completed;
                await _ticket.UpdateTicketAsync(ticket);

                await _hubContext.Clients.All.SendAsync("OnTicketUpdated", update);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok(new { success = true });

            return RedirectToAction("Dashboard");
        }
    }
}
