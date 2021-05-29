using DewarpingApp.Domain.Models;
using IronPython.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Scripting.Hosting;
using System.IO;

namespace DewarpingApp.Service
{
    public class FileService
    {
        public async void SaveFileAsync(IFormFile file, IWebHostEnvironment environment)
        {
            using var fileStream = new FileStream(environment.WebRootPath + "/Files/" + file.FileName, FileMode.Create);
            await file.CopyToAsync(fileStream);
        }

        public ImageFile TransformFile(ImageFile file)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();

            scope.SetVariable("imageFile", file);
            engine.ExecuteFile("TransformImage.py", scope);

            return scope.GetVariable<ImageFile>("imageFile");
        }
    }
}
