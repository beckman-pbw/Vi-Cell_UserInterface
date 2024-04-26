using System.Linq;
using System.Windows.Media.Imaging;
using NUnit.Framework;
using ScoutDomains.DataTransferObjects;

namespace ScoutOpcUaTests
{
    public class ImageDtoToImageMapTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void ImageDtoToImage()
        {
            var dto = new ImageDto
            {
                Cols = 100,
                Rows = 100,
                ImageSource = BitmapSourceByteStringMapTests.GetBitmapSource(),
                Step = 8,
                Type = 0 // OpenCV type ex: CV_8UC1 == 0 (see "types_c.h" from OpenCV)
            };

            var map = Mapper.Map<GrpcService.Image>(dto);
            
            Assert.IsNotNull(map);
            Assert.AreEqual(dto.Cols, map.Cols);
            Assert.AreEqual(dto.Rows, map.Rows);
            Assert.AreEqual(dto.Step, map.Step);
            Assert.AreEqual(dto.Type, map.Type.FirstOrDefault());
            Assert.IsNotNull(map.ImageSource);
            Assert.AreEqual(16384, map.ImageSource.Length);
        }
    }
}