using Microsoft.AspNetCore.Mvc;
using SmartQueue.Data.Interfaces;

namespace SmartQueue.Controllers
{
    public class ServiceController: Controller
    {
        private IService IAllServices;
        public ServiceController(IService allServices)
        {
            this.IAllServices = allServices;
        }
        public ViewResult List()
        {
            ViewBag.Title = "Предоставляемые услуги";

            var services = IAllServices.Services;
            return View(services);
        }
    }
}
