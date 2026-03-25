using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartQueue.Data.Interfaces;
using SmartQueue.Hubs;
using SmartQueue.ViewModels;
using System.Security.Claims;
using static SmartQueue.Hubs.QueueHub;
using SmartQueue.Data.Models;
using System.Reflection.PortableExecutable;

namespace SmartQueue.Controllers
{
    [Authorize]
    public class AdminController(ITicket ticket, IService service, IHubContext<QueueHub> hubContext) : Controller
    {
        private readonly ITicket _ticket = ticket;
        private readonly IService _service = service;
        private readonly IHubContext<QueueHub> _hubContext = hubContext;

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard");
            }

            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (userIp != "127.0.0.1" && userIp != "::1")
                return StatusCode(403);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Login(string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Логин и пароль обязательны");
                return View();
            }

            bool isValidUser = (password == "admin");

            if (isValidUser)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("AdminId", "1") 
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, 
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToLocal(returnUrl ?? "/Admin/Dashboard");
            }

            ModelState.AddModelError("", "Неверный логин или пароль");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
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
                ticket.DateEnd = DateTime.Now;

                await _ticket.UpdateTicketAsync(ticket);
                await _hubContext.Clients.All.SendAsync("OnTicketUpdated", update);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok(new { success = true });

            return RedirectToAction("Dashboard");
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> RecallTicket(int id)
        {
            var ticket = _ticket.Tickets.FirstOrDefault(t => t.Id == id);
            if (ticket != null && ticket.Status == Ticket.StatusType.Active)
            {
                await _hubContext.Clients.All.SendAsync("OnTicketUpdated", new
                {
                    id = ticket.Id,
                    number = ticket.Number,
                    serviceName = ticket.Service?.Name,
                    status = "Active"
                });
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult Service()
        {
            var services = _service.Services.ToList();
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "Название услуги обязательно");
                return View(model);
            }

            model.Code = GetLetter(_service.Services.Count() + 1);

            await _service.AddServiceAsync(model);
            return RedirectToAction(nameof(Service));
        }
        public IActionResult Edit(int id)
        {
            var service = _service.Services.FirstOrDefault(s => s.Id == id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "Название услуги обязательно");
                return View(model);
            }

            var existing = _service.Services.FirstOrDefault(s => s.Id == model.Id);
            if (existing != null)
            {
                existing.Name = model.Name;
                existing.Description = model.Description;
                await _service.UpdateServiceAsync(existing);
            }

            return RedirectToAction(nameof(Service));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Service model)
        {
            var service = _service.Services.FirstOrDefault(s => s.Id == model.Id);
            if (service != null)
            {
                await _service.RemoveServiceAsync(service);
            }
            return RedirectToAction(nameof(Service));
        }

        private string GetLetter(int index)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс должен быть от 1");

            const string letters = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

            if (index <= 33) 
            {
                return letters[index - 1].ToString();
            }
            else 
            {
                int firstLetterIndex = ((index - 1) / 33) - 1; 
                int secondLetterIndex = (index - 1) % 33;   

                return letters[firstLetterIndex].ToString() + letters[secondLetterIndex];
            }
        }
    }
}
