using ScoutUtilities.Services;
using Moq;
using Ninject;
using Ninject.Extensions.Logging;
using NUnit.Framework;
using ScoutUtilities.UIConfiguration;
using TestSupport;

namespace ScoutUtilities.Services.Tests
{
    [TestFixture]
    public class FileSystemServiceTests : BaseTest
    {
        private Mock<ILogger> _loggerMock;
        
        [OneTimeSetUp]
        public void ClassInit()
        {
            _loggerMock = new Mock<ILogger>();
            Kernel = new StandardKernel(new ScoutUtilitiesNinjectModule());
            Kernel.Bind<ILogger>().ToConstant(_loggerMock.Object);
        }

        [TestCase(@"\\svusftcfs1\Temp\Jeremiah", ExpectedResult = true)]
        [TestCase(@"\\svusftcfs1\Temp\Jeremiah", ExpectedResult = true)]
        [TestCase(@"T:\Jeremiah", ExpectedResult = true)]
        [TestCase(@"T:\Jeremiah\", ExpectedResult = true)]
        [TestCase(@"D:\", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah\", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah\Test", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah\Test\Valid", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah", ExpectedResult = true)]
        [TestCase(@"D:\Instrument\Export\Jeremiah", ExpectedResult = true)]
        [TestCase(@"d@ fU^$& eeeezzz D1zzZ", ExpectedResult = false)]

        [TestCase(@"C:",  ExpectedResult = false)]
        [TestCase(@"C:",  ExpectedResult = false)]
        [TestCase(@"C:\",  ExpectedResult = false)]
        [TestCase(@"C:\",  ExpectedResult = false)]
        [TestCase(@"C:\Jeremiah",  ExpectedResult = false)]
        [TestCase(@"C:\Windows",  ExpectedResult = false)]
        [TestCase(@"C:\Instrument",  ExpectedResult = false)]
        [TestCase(@"C:\Instrument\Logs",  ExpectedResult = false)]
        [TestCase(@"C:\Instrument\Logs\",  ExpectedResult = false)]
        [TestCase(@"C:\Instrument\Logs\Export", ExpectedResult = false)]
        [TestCase(@"C:\Instrument\E\xport", ExpectedResult = false)]
        [TestCase(@"banana time", ExpectedResult = false)]
        [TestCase(@"d@ fU^$& eeeezzz D1zzZ", ExpectedResult = false)]
        public bool FolderIsValidForScheduledExportTest(string pathToCheck)
        {
            var originalHW = UISettings.IsFromHardware;
            var originalVcr = UISettings.EnableSampleSetVcrControlsInOfflineMode;
            UISettings.IsFromHardware = true;
            UISettings.EnableSampleSetVcrControlsInOfflineMode = true;

            var result = ScoutUtilities.Common.FileSystem.IsFolderValidForExport(pathToCheck);

            UISettings.IsFromHardware = originalHW;
            UISettings.EnableSampleSetVcrControlsInOfflineMode = originalVcr;

            return result;
        }

        [TestCase(@"\\svusftcfs1\Temp\Jeremiah", ExpectedResult = true)]
        [TestCase(@"\\svusftcfs1\Temp\Jeremiah", ExpectedResult = true)]
        [TestCase(@"T:\Jeremiah", ExpectedResult = true)]
        [TestCase(@"T:\Jeremiah", ExpectedResult = true)]
        [TestCase(@"D:\", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah\", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah\Test", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah\Test\Valid", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Export\Jeremiah", ExpectedResult = true)]
        [TestCase(@"D:\Instrument\Export\Jeremiah", ExpectedResult = true)]
        [TestCase(@"S:\\d fU^$& eeeezzz D1zzZ", ExpectedResult = true)]

        [TestCase(@"C:", ExpectedResult = true)]
        [TestCase(@"C:\", ExpectedResult = true)]
        [TestCase(@"C:\", ExpectedResult = true)]
        [TestCase(@"C:\Jeremiah", ExpectedResult = true)]
        [TestCase(@"C:\Windows", ExpectedResult = true)]
        [TestCase(@"C:\Instrument", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Logs", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Logs\", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\Logs\Export", ExpectedResult = true)]
        [TestCase(@"C:\Instrument\E\xport", ExpectedResult = true)]

        [TestCase(@"banana time", ExpectedResult = false)]
        [TestCase(@"d@ fU^$& eeeezzz D1zzZ", ExpectedResult = false)]
        public bool FolderIsValidForScheduledExportTestOffline(string pathToCheck)
        {
            var originalHW = UISettings.IsFromHardware;
            var originalVcr = UISettings.EnableSampleSetVcrControlsInOfflineMode;
            UISettings.IsFromHardware = false;
            UISettings.EnableSampleSetVcrControlsInOfflineMode = false;

            var result = ScoutUtilities.Common.FileSystem.IsFolderValidForExport(pathToCheck);

            UISettings.IsFromHardware = originalHW;
            UISettings.EnableSampleSetVcrControlsInOfflineMode = originalVcr;

            return result;
        }

        [TestCase("Space Filename", ExpectedResult = true)]
        [TestCase("1NumberFilename", ExpectedResult = true)]
        [TestCase("_Filename", ExpectedResult = true)]
        [TestCase("_Filename _", ExpectedResult = true)]
        [TestCase("AnotherFilename", ExpectedResult = true)]
        [TestCase("AnotherFilename.ext", ExpectedResult = true)]
        [TestCase("C:\\Instrument\\Export\\AnotherFilename", ExpectedResult = true)]
        [TestCase("C:\\Instrument\\Export\\AnotherFilename.ext", ExpectedResult = true)]
        [TestCase("AnotherFilename.ext.csv", ExpectedResult = true)]
        [TestCase("AnotherFilename.csv.csv", ExpectedResult = true)]
        [TestCase("Another&Filename.ext", ExpectedResult = true)]
        [TestCase("Another'Filename.ext", ExpectedResult = true)]
        [TestCase("Another^Filename.ext", ExpectedResult = true)]

        [TestCase("C:\\Instrument\\Export\\Another*Filename.ext", ExpectedResult = false)]
        [TestCase("C:\\Instrument\\Export\\BA<?>D\\AnotherFilename.ext", ExpectedResult = false)]
        [TestCase("Instrument\\Export\\AnotherFilename", ExpectedResult = false)]
        [TestCase("..\\Export\\AnotherFilename", ExpectedResult = false)]
        [TestCase("Instrument\\Export\\AnotherFilename.ext", ExpectedResult = false)]
        [TestCase("..\\Export\\AnotherFilename.ext", ExpectedResult = false)]
        [TestCase("Another\"Filename.ext", ExpectedResult = false)]
        [TestCase("Another\\Filename.ext", ExpectedResult = false)]
        [TestCase("Another/Filename.ext", ExpectedResult = false)]
        [TestCase("Another<Filename.ext", ExpectedResult = false)]
        [TestCase("Another>Filename.ext", ExpectedResult = false)]
        [TestCase("Another*Filename.ext", ExpectedResult = false)]
        public bool FileNameIsValidTest(string filename)
        {
            return ScoutUtilities.Common.FileSystem.IsFileNameValid(filename);
        }
    }
}