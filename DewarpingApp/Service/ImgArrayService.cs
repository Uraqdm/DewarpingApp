using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace DewarpingApp.Service
{
    public class ImgArrayService : IInputArray, IOutputArray
    {
        private readonly Image<Rgb, byte> image;

        public ImgArrayService(Image<Rgb, byte> img)
        {
            image = img;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public InputArray GetInputArray()
        {
            return image.GetInputArray();
        }

        public OutputArray GetOutputArray()
        {
            return image.GetOutputArray();
        }
    }
}
