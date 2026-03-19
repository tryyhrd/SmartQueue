using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;

namespace SmartQueue.Controllers
{
    
    public class ServiceController: Controller
    {
        private readonly IService _allServices;
        public ServiceController(IService allServices)
        {
            this._allServices = allServices;
        }

        [Route("Services")]
        public ViewResult List()
        {
            ViewBag.Title = "Все услуги";

            var services = _allServices.Services;
            return View(services);
        }
    }
}
