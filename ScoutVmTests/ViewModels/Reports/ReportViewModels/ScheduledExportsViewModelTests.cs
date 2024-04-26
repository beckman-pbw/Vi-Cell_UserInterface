using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ScoutDomains.Reports.ScheduledExports;
using ScoutServices.Interfaces;

namespace ScoutViewModels.ViewModels.Reports.ReportViewModels.Tests
{
    [TestFixture]
    public class ScheduledExportsViewModelTests
    {
        private Mock<IScheduledExportsService> _mockExportService;
        
        [SetUp]
        public void Setup()
        {
            _mockExportService = new Mock<IScheduledExportsService>();
            _mockExportService.Setup(m => m.GetSampleResultsScheduledExports()).Returns(new List<SampleResultsScheduledExportDomain>());
        }

        [Test]
        public void CanAddScheduledExportTest_Success()
        {
            var vm = new ScheduledExportsViewModel(_mockExportService.Object);
            Assert.IsTrue(vm.AddScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanEditScheduledExportTest_Success()
        {
            var vm = new ScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = new SampleResultsScheduledExportDomain();
            Assert.IsTrue(vm.EditScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanEditScheduledExportTest_Failure()
        {
            var vm = new ScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = null;
            Assert.IsFalse(vm.EditScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanDeleteScheduledExportTest_Success()
        {
            var vm = new ScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = new SampleResultsScheduledExportDomain();
            Assert.IsTrue(vm.DeleteScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanDeleteScheduledExportTest_Failure()
        {
            var vm = new ScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = null;
            Assert.IsFalse(vm.DeleteScheduledExport.CanExecute(null));
        }
    }
}