using DewarpingApp.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DewarpingApp.Service
{
    public static class FileService
    {
        public static ImageFile SaveAndTransformFileAsync(IFormFile file, IWebHostEnvironment environment)
        {
            string path = environment.WebRootPath + "\\Files\\" + file.FileName;

            using var fileStream = new FileStream(path, FileMode.Create);
            file.CopyTo(fileStream);

            Bitmap img = (Bitmap)Image.FromStream(fileStream);

            Bitmap result = BarrelDistortion(img, true, Color.White);
            string distortedPath = environment.WebRootPath + "\\Files\\" + "dst_" + file.FileName;
            result.Save(distortedPath, ImageFormat.Jpeg);

            return new ImageFile() { DistortedPath = distortedPath, Path = path, Name = file.Name };
        }

        private static Bitmap BarrelDistortion(Bitmap sourceImage, bool autoCrop, Color backgroundColor)
        {
            Bitmap StartImage = null;
            BitmapData srcBitmapData = null;
            byte[] srcPixels = null;
            byte[] dstPixels = null;
            Bitmap NewImage = null;
            BitmapData dstBitmapData = null;

            double factor = -0.5;

            try
            {

                // Убедитесь бит на пиксель равен 8, 24 или 32
                int Depth = Image.GetPixelFormatSize(sourceImage.PixelFormat);
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Возвращает количество цветовых компонентов
                int cCount = Depth / 8;

                Size baseSize = new Size(sourceImage.Width, sourceImage.Height);

                //проверьте, не слишком ли низкое изображение и его нужно изменить, чтобы улучшить качество.
                //и не генерировать псевдонимы изображений
                int maxSize = Math.Max(sourceImage.Width, sourceImage.Height);
                if (maxSize < 3000)
                {
                    float percent = 3000F / maxSize;
                    baseSize = new Size((int)(sourceImage.Width * percent), (int)(sourceImage.Height * percent));
                }

                StartImage = new Bitmap(baseSize.Width, baseSize.Height, sourceImage.PixelFormat);
                StartImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

                //Создает дизайн-объект и белый фон
                Graphics g = Graphics.FromImage(StartImage);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(sourceImage, new Rectangle(-1, -1, baseSize.Width + 1, baseSize.Height + 1), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel);
                g.Dispose();
                // Заблокируйте исходное изображение и скопируйте его в массив байтов и отпустите исходное изображение.
                srcBitmapData = StartImage.LockBits(new Rectangle(0, 0, StartImage.Width, StartImage.Height), ImageLockMode.ReadOnly, StartImage.PixelFormat);
                srcPixels = new byte[StartImage.Width * StartImage.Height * (Depth / 8)];
                Marshal.Copy(srcBitmapData.Scan0, srcPixels, 0, srcPixels.Length);
                StartImage.UnlockBits(srcBitmapData);
                srcBitmapData = null;

                //Создать массив байтов целевого изображения
                dstPixels = new byte[srcPixels.Length];

                //Заполняет весь кадр выбранным цветом фона
                int index = ((1 * StartImage.Width) + 1) * cCount; //index = ((Y * Width) + X) * cCount
                do
                {
                    if (Depth == 32) // Для 32 бит на пиксель выберите красный, зеленый, синий и альфа-канал.
                    {
                        dstPixels[index++] = backgroundColor.B;
                        dstPixels[index++] = backgroundColor.G;
                        dstPixels[index++] = backgroundColor.R;
                        dstPixels[index++] = backgroundColor.A; // a
                    }
                    if (Depth == 24) // Для 24 бит на пиксель выберите красный, зеленый, синий и альфа-канал.
                    {
                        dstPixels[index++] = backgroundColor.B;
                        dstPixels[index++] = backgroundColor.G;
                        dstPixels[index++] = backgroundColor.R;
                    }
                    if (Depth == 8)
                    // Для 8 бит на пиксель устанавливает значение цвета (красный, зеленый и синий - одно и то же)
                    {
                        dstPixels[index++] = backgroundColor.B;
                    }

                } while (index < srcPixels.Length);
                //Вычисляет максимально возможную амплитуду изображения и умножает ее на желаемый коэффициент.
                double amp = 0;
                double ang = Math.PI * 0.5;
                for (int a = 0; a < StartImage.Height; a++)
                {
                    int y = (int)((StartImage.Height / 2) - amp * Math.Sin(ang));
                    if ((y < 0) || (y > StartImage.Height))
                        break;
                    amp = a;
                }
                amp = (amp - 2) * (factor < -1 ? -1 : (factor > 1 ? 1 : factor));
                //Определяет переменные, которые вычисляют точки отсечения (если есть)
                int x1, y1, x2, y2;
                x1 = StartImage.Width;
                y1 = StartImage.Height;
                x2 = 0;
                y2 = 0;

                //Копирует пиксель за пикселем в новые позиции
                index = ((1 * StartImage.Width) + 1) * cCount;
                do
                {

                    int y = (index / cCount) / StartImage.Width;
                    int x = (index / cCount) - (y * StartImage.Width);

                    Point pt = NewPoint(new Point(x, y), StartImage.Width, StartImage.Height, amp, factor < 0);

                    //Значения для обрезки
                    if (factor >= 0)
                    {
                        if (x == StartImage.Width / 2)
                        {
                            if (pt.Y < y1)
                                y1 = pt.Y;

                            if (pt.Y > y2)
                                y2 = pt.Y;
                        }

                        if (y == StartImage.Height / 2)
                        {
                            if (pt.X < x1)
                                x1 = pt.X;

                            if (pt.X > x2)
                                x2 = pt.X;
                        }
                    }
                    else
                    {
                        if ((x == 1) && (y == 1))
                        {
                            y1 = pt.Y;
                            x1 = pt.X;
                        }

                        if ((x == StartImage.Width - 1) && (y == StartImage.Height - 1))
                        {
                            y2 = pt.Y;
                            x2 = pt.X;
                        }
                    }

                    //Индекс байта, где будет применен пиксель
                    int dstIndex = ((pt.Y * StartImage.Width) + pt.X) * cCount;

                    if (Depth == 32)
                    {
                        dstPixels[dstIndex] = srcPixels[index++];
                        dstPixels[dstIndex + 1] = srcPixels[index++];
                        dstPixels[dstIndex + 2] = srcPixels[index++];
                        dstPixels[dstIndex + 3] = srcPixels[index++]; // a
                    }
                    if (Depth == 24)
                    {
                        dstPixels[dstIndex] = srcPixels[index++];
                        dstPixels[dstIndex + 1] = srcPixels[index++];
                        dstPixels[dstIndex + 2] = srcPixels[index++];
                    }
                    if (Depth == 8)
                    {
                        dstPixels[dstIndex] = srcPixels[index++];
                    }

                } while (index < srcPixels.Length);

                //Создает новое изображение на основе ранее созданного байтового массива
                NewImage = new Bitmap(StartImage.Width, StartImage.Height, StartImage.PixelFormat);
                NewImage.SetResolution(StartImage.HorizontalResolution, StartImage.VerticalResolution);
                dstBitmapData = NewImage.LockBits(new Rectangle(0, 0, StartImage.Width, StartImage.Height), ImageLockMode.WriteOnly, StartImage.PixelFormat);
                Marshal.Copy(dstPixels, 0, dstBitmapData.Scan0, dstPixels.Length);
                NewImage.UnlockBits(dstBitmapData);

                /*
                 // Для наглядности нарисуйте квадрат, в котором будет выполняться разрез.
                 Graphics g2 = Graphics.FromImage(NewImage);
                 g2.SmoothingMode = SmoothingMode.AntiAlias;
                 g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                 g2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                 g2.DrawRectangle(new Pen(new SolidBrush(Color.Red), 3), new Rectangle(x1, y1, x2 - x1, y2 - y1));
                 g2.Dispose();*/

                //Создает окончательное изображение с кадрированием или реальным изменением размера
                Bitmap FinalImage = new Bitmap(sourceImage.Width, sourceImage.Height, StartImage.PixelFormat);
                NewImage.SetResolution(StartImage.HorizontalResolution, StartImage.VerticalResolution);

                Graphics g1 = Graphics.FromImage(FinalImage);
                g1.SmoothingMode = SmoothingMode.AntiAlias;
                g1.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g1.PixelOffsetMode = PixelOffsetMode.HighQuality;

                //Выполняет резку, если включена автоматическая резка и есть необходимость резать
                if ((autoCrop) && ((x1 > 0) || (y1 > 0) || (x2 < NewImage.Height) || (y2 < NewImage.Height)))
                {
                    Rectangle cropRect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                    g1.DrawImage(NewImage, new Rectangle(-1, -1, FinalImage.Width + 1, FinalImage.Height + 1), cropRect.X, cropRect.Y, cropRect.Width, cropRect.Height, GraphicsUnit.Pixel);
                }
                else
                {
                    g1.DrawImage(NewImage, new Rectangle(-1, -1, FinalImage.Width + 1, FinalImage.Height + 1), 0, 0, NewImage.Width, NewImage.Height, GraphicsUnit.Pixel);
                }

                g1.Dispose();
                g1 = null;

                NewImage = null;
                return FinalImage;
            }
            finally
            {
                srcBitmapData = null;
                srcPixels = null;
                dstPixels = null;
                dstBitmapData = null;
            }

        }

        private static Point NewPoint(Point AtualPoint, int Width, int Height, double Aplitude, bool inverse)
        {
            Point uP = AtualPoint;

            int pY, pX;
            double aY, aX;

            aY = aX = 0;

            double angX = Math.PI * 1 * uP.X / Width;
            double caX = Aplitude * ((((double)Height / 2F) - uP.Y) / ((double)Height / 2F));

            double angY = Math.PI * 1 * uP.Y / Height;
            double caY = Aplitude * ((((double)Width / 2F) - uP.X) / ((double)Width / 2F));

            if (inverse)
            {
                double iAng = Math.PI * -1 * 0.5;
                aX = (caX * Math.Sin(iAng));
                aY = (caY * Math.Sin(iAng));
            }

            pY = (int)(uP.Y + aX + caX * Math.Sin(angX));
            pX = (int)(uP.X + aY + caY * Math.Sin(angY));

            return new Point(pX, pY);
        }
    }
}
