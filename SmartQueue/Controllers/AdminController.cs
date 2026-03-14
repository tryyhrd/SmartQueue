using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;
using SmartQueue.ViewModels;

namespace SmartQueue.Controllers
{
    public class AdminController: Controller
    {
        private readonly ITicket _ticket;
        private readonly IService _service;
        public AdminController(ITicket ticket, IService service)
        {
            _ticket = ticket;
            _service = service;
        }
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
            var ticket = _ticket.Tickets.FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(new { error = "Талон не найден" });
                return NotFound("Талон не найден");
            }

            ticket.Status = Data.Models.Ticket.StatusType.Active;
            await _ticket.UpdateTicketAsync(ticket);

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
    }
}
