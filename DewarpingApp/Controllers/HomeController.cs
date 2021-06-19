using DewarpingApp.Context;
using DewarpingApp.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DewarpingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext context;
        private readonly IWebHostEnvironment environment;

        public HomeController(AppDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index() => View(context.ImageFiles);

        public IActionResult AddFile() => View();

        [HttpPost]
        public IActionResult AddFile(IFormFile file)
        {
            if(file != null)
            {
                var img = FileService.SaveAndTransformFileAsync(file, environment);
                context.ImageFiles.Add(img);
                context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult MoreInfo(int id)
        {
            return View(context.ImageFiles.Where(x => x.Id == id).FirstOrDefault());
        }
    }
}
