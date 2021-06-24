using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gabriel.Cat.S.Extension;

namespace CheckFenix
{
    public static class BitmapExtension
    {
        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            BitmapImage imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = bmp.ToStream();
            imageSource.EndInit();
            return imageSource;
        }
    }
}
