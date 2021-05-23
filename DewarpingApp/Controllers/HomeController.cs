using Microsoft.AspNetCore.Mvc;

namespace DewarpingApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
