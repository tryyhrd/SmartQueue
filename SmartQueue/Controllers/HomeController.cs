using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Models;

namespace SmartQueue.Controllers
{
    public class HomeController: Controller
    {
        private readonly IVisitor _visitor;
        private readonly ITicket _ticket;
        private readonly IService _service;
        public HomeController(IVisitor visitor, ITicket ticket, IService service)
        {
            _visitor = visitor;
            _ticket = ticket;
            _service = service;
        }
        public async Task<IActionResult> Autorization()
        {
            //var visitor = await GetOrCreateVisitorAsync();
            //if (visitor == null) return BadRequest("Не удалось определить IP");

            //var activeTicket = _ticket.Tickets.FirstOrDefault(t =>
            //    t.Visitor.Id == visitor.Id &&
            //    (t.Status == Ticket.StatusType.Waiting || t.Status == Ticket.StatusType.Active));

            //if (activeTicket != null)
            //    return RedirectToAction("GetTicket", "Ticket", new { id = activeTicket.Id });

            //return View("~/Views/Service/List.cshtml", _service.Services.ToList());

            return View("~/Views/Admin/Login.cshtml");
        }

        private async Task<Visitor> GetOrCreateVisitorAsync()
        {
            var visitorId = HttpContext.Session.GetInt32("VisitorId");
            if (visitorId.HasValue)
            {
                var visitor = _visitor.Visitors.FirstOrDefault(v => v.Id == visitorId.Value);
                if (visitor != null) return visitor;
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ip)) return null;

            var visitorByIp = _visitor.Visitors.FirstOrDefault(v => v.Ip == ip);

            if (visitorByIp == null)
            {
                visitorByIp = new Visitor { Ip = ip };
                await _visitor.AddVisitorAsync(visitorByIp);
            }

            HttpContext.Session.SetInt32("VisitorId", visitorByIp.Id);
            return visitorByIp;
        }
    }
}
