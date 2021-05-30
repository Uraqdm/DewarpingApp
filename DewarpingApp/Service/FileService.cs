﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace DewarpingApp.Service
{
    public static class FileService
    {
        public static async void SaveAndTransformFileAsync(IFormFile file, IWebHostEnvironment environment)
        {
            using var fileStream = new FileStream(environment.WebRootPath + "/Files/" + file.FileName, FileMode.Create);
            await file.CopyToAsync(fileStream);
        }
    }
}
