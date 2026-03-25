using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Models;
using SmartQueue.Hubs;
using static SmartQueue.Hubs.QueueHub;

namespace SmartQueue.Controllers
{
    public class TicketController(ITicket tickets, IVisitor visitor, IService service, IHubContext<QueueHub> hubContext) : Controller
    {
        private readonly ITicket _tickets = tickets;
        private readonly IVisitor _visitor = visitor;
        private readonly IService _service = service;
        private readonly IHubContext<QueueHub> _hubContext = hubContext;


        public async Task<ActionResult> Create(int serviceId)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (ip == null)
            {
                return BadRequest("Не удалось определить ваш адрес");
            }

            var tickets = _tickets.Tickets;
            var visitors = _visitor.Visitors;
            var services = _service.Services;

            var service = services.FirstOrDefault(x => x.Id == serviceId);

            if (service == null)
            {
                return NotFound("Услуга не найдена");
            }

            var visitor = visitors.FirstOrDefault(v => v.Ip == ip);

            if (visitor == null)
            {
                visitor = new Visitor { Ip = ip };
                await _visitor.AddVisitorAsync(visitor);
            }

            else
            {
                var activeTicket = tickets.FirstOrDefault(t =>
                    t.Visitor.Id == visitor.Id &&
                    (t.Status == Ticket.StatusType.Waiting || t.Status == Ticket.StatusType.Active));

                if (activeTicket != null)
                {
                    return RedirectToAction("Get", "Ticket", new { id = activeTicket.Id });
                }
            }

            var ticket = new Ticket
            {
                Number = $"{service.Code + tickets.Count(x => x.Service == service) + 1:000}",
                Visitor = visitor,
                Service = service,
                CreatedAt = DateTime.Now,
                Status = Ticket.StatusType.Waiting
            };

            await _tickets.AddTicketAsync(ticket);

            var update = new TicketUpdate
            {
                Id = ticket.Id,
                Number = ticket.Number,
                ServiceName = ticket.Service?.Name,
                Status = "Waiting",
                CreatedAt = ticket.CreatedAt
            };

            await _hubContext.Clients.All.SendAsync("OnTicketUpdated", update);

            var model = new ViewModels.QueueTicketViewModel
            {
                Service = service,
                Ticket = ticket,
                ServiceTickets = tickets
                .Where(x => 
                x.Service == service && 
                x.Status == Ticket.StatusType.Waiting)
                .ToList()
            };

            return View("Index", model);
        }
        public IActionResult Get(int id)
        {
            var ticket = _tickets.Tickets.FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound("Талон не найден");
            }

            var service = _service.Services.FirstOrDefault(s => s.Id == ticket.Service.Id);

            var model = new ViewModels.QueueTicketViewModel
            {
                Ticket = ticket,
                Service = service,
                ServiceTickets = _tickets.Tickets.Where(x =>
                x.Service == service &&
                x.Status == Ticket.StatusType.Waiting)
                .ToList()
            };

            return View("Index", model);
        }
        [HttpGet]
        public IActionResult GetPosition(int ticketId)
        {
            var ticket = _tickets.Tickets.FirstOrDefault(t => t.Id == ticketId);
            var status = ticket?.Status ?? 0;
            if (status != Ticket.StatusType.Waiting) return Content("-");

            var allWaitingTickets = _tickets.Tickets
                .Where(t => t.Status == Ticket.StatusType.Waiting);

            var position = allWaitingTickets
                .Where(t => t.Service == ticket.Service && t.CreatedAt < ticket.CreatedAt)
                .Count() + 1;

            return Content(position.ToString());
        }
    }
}
