using Microsoft.AspNetCore.Mvc;

namespace AppointmentAPI.Controllers
{
    public class AppointmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
