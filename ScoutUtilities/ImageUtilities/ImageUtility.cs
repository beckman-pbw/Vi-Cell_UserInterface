// ***********************************************************************
// <copyright file="ImageUtility.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;


namespace ScoutUtilities.ImageUtilities
{
   
    public static class ImageUtility
    {
       
        public static Bitmap BitmapSourceToBitmap(BitmapSource srs)
        {
            var width = srs.PixelWidth;
            var height = srs.PixelHeight;
            var stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            var ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed,
                    ptr))
                {
                    return new Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}