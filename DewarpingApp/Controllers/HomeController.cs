using DewarpingApp.Context;
using Microsoft.AspNetCore.Http;
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

        public IActionResult AddFile(IFormFile file)
        {
            if(file != null)
            {
                context.ImageFiles.Add(new Domain.Models.ImageFile { Name = file.FileName, Path = $"/Files/{file.FileName}" });
                RedirectToAction("Index");
            }
            return View();
        }
    }
}
