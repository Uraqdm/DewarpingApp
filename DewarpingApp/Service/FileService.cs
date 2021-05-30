using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace DewarpingApp.Service
{
    public static class FileService
    {
        public static async void SaveAndTransformFileAsync(IFormFile file, IWebHostEnvironment environment)
        {
            string path = environment.WebRootPath + "/Files/" + file.FileName;

            using var fileStream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fileStream);

            Image<Rgb, byte> img = new Image<Rgb, byte>(path);
            var iAS = new ImgArrayService(img);

            Fisheye.UndistorImage(iAS, iAS, null, null);
        }
    }
}
