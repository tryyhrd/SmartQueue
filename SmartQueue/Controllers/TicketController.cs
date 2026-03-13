using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Models;

namespace SmartQueue.Controllers
{
    public class TicketController(ITicket tickets, IVisitor visitor, IService service) : Controller
    {
        private readonly ITicket _tickets = tickets;
        private readonly IVisitor _visitor = visitor;
        private readonly IService _service = service;

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
                    return RedirectToAction("GetTicket", "Ticket", new { id = activeTicket.Id });
                }
            }

            var ticket = new Ticket
            {
                Number = $"A{tickets.Count() + 1:000}",
                Visitor = visitor,
                Service = service,
                CreatedAd = DateTime.Now,
                Status = Ticket.StatusType.Waiting
            };

            await _tickets.AddTicketAsync(ticket);

            var model = new ViewModels.QueueTicketViewModel
            {
                Service = service,
                Ticket = ticket
            };

            return View("Create", model);
        }

        public IActionResult GetTicket(int id)
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
                Service = service
            };

            return View("Create", model);
        }
    }
}
