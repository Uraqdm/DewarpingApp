using DewarpingApp.Context;
using Microsoft.AspNetCore.Mvc;

namespace DewarpingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext context;

        public HomeController(AppDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index() => View();
    }
}
