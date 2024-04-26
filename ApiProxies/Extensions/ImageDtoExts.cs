using ApiProxies.Generic;
using log4net;
using ScoutDomains.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using ScoutUtilities.Common;

namespace ApiProxies.Extensions
{
    public static class ImageDtoExts
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void SaveImage(this ImageDto image, string filename)
        {
            try
            {
	            if (image == null)
		            return;

                var path = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (File.Exists(filename))
                {
                    // TODO: - PC3527-2845 - This is a temporary workaround to clear the image until 
                    // the longer term in mem bitmap approach is completed. This additional GC force will be removed at that time.
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                    File.Delete(filename);
                }

                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image.ImageSource));
                    encoder.Save(stream);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error attempting to save image '{filename}'", ex);
                ExceptionHelper.LogException(ex, "LID_EXCEPTIONMSG_ERROR_ON_SAVE_IMAGES");
            }
        }

        public static void SaveImages(this List<ImageDto> images, string dirPath,
            string filePrefix)
        {
            int i = 0;
            foreach (var image in images)
            {
                var filename = Path.Combine(dirPath, $"{filePrefix}_{i}{ApplicationConstants.ImageFileExtension}");
                SaveImage(image, filename);
                i++;
            }
        }
    }
}