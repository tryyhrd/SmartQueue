using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;

namespace SmartQueue.Controllers
{
    //[Route("Service")]
    public class ServiceController: Controller
    {
        private readonly IService _allServices;
        public ServiceController(IService allServices)
        {
            this._allServices = allServices;
        }

        //[HttpGet("List")]
        public ViewResult List()
        {
            ViewBag.Title = "Предоставляемые услуги";

            var services = _allServices.Services;
            return View(services);
        }
    }
}
