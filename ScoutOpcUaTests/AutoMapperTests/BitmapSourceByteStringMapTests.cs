using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Google.Protobuf;
using NUnit.Framework;
using ScoutUI.Views._1___Home;

namespace ScoutOpcUaTests
{
    public class BitmapSourceByteStringMapTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void CreateBitmapSanityCheckTest()
        {
            var bitmap = GetBitmapSource();
            Assert.IsNotNull(bitmap);
            Assert.AreEqual(128, bitmap.Height);
            Assert.AreEqual(128, bitmap.Width);
            Assert.AreEqual(96, bitmap.DpiX);
            Assert.AreEqual(96, bitmap.DpiY);
        }

        [Test]
        public void BitmapSourceToByteString()
        {
            var bitmap = GetBitmapSource();
            var bytes = Mapper.Map<ByteString>(bitmap);
            Assert.IsNotNull(bytes);
            Assert.AreEqual(false, bytes.IsEmpty);
            Assert.AreEqual(16384, bytes.Length);
        }

        public static BitmapSource GetBitmapSource()
        {
            var bitsPerPixel = 8; // matches PixelFormat below
            var width = 128;
            var height = width;
            var stride = (width * bitsPerPixel) / 8;
            var pixels = new byte[height * stride];

            // Try creating a new image with a custom palette.
            var colors = new List<Color>();
            colors.Add(Colors.Red);
            colors.Add(Colors.Blue);
            colors.Add(Colors.Green);
            var myPalette = new BitmapPalette(colors);

            // Creates a new empty image with the pre-defined palette
            var image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed8,
                myPalette,
                pixels,
                stride);
            
            return image;
        }
    }
}