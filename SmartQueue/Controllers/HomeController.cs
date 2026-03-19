using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Models;
using SmartQueue.Data.Services;

namespace SmartQueue.Controllers
{
    public class HomeController(IVisitor visitor, ITicket ticket, IService service, IpService ipService) : Controller
    {
        private readonly IVisitor _visitor = visitor;
        private readonly ITicket _ticket = ticket;
        private readonly IService _service = service;
        private readonly IpService _ipService = ipService;

        [Route("Scan")]
        [Route("Home/Scan")]
        public IActionResult Home() 
        {
            ViewBag.Ip = _ipService.GetIpAddress();
            return View();
        }

        [Route("")]
        [Route("Home")]
        [Route("Home/Authorization")]
        public async Task<IActionResult> Authorization()
        {
            var visitor = await GetOrCreateVisitorAsync();
            if (visitor == null) return BadRequest("Не удалось определить IP");

            var activeTicket = _ticket.Tickets.FirstOrDefault(t =>
            t.Visitor.Id == visitor.Id &&
            (t.Status == Ticket.StatusType.Waiting || t.Status == Ticket.StatusType.Active));

            if (activeTicket != null)
                return RedirectToAction("Get", "Ticket", new { id = activeTicket.Id });

            return RedirectToAction("List", "Service");
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
