using Moq;
using NUnit.Framework;
using ScoutDomains.Reports.ScheduledExports;
using ScoutServices.Interfaces;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels.Reports.Tests
{
    [TestFixture()]
    public class AuditLogScheduledExportsViewModelTests
    {
        private Mock<IScheduledExportsService> _mockExportService;

        [SetUp]
        public void Setup()
        {
            _mockExportService = new Mock<IScheduledExportsService>();
            _mockExportService.Setup(m => m.GetAuditLogScheduledExports()).Returns(new List<AuditLogScheduledExportDomain>());
        }

        [Test]
        public void CanAddScheduledExportTest_Success()
        {
            var vm = new AuditLogScheduledExportsViewModel(_mockExportService.Object);
            Assert.IsTrue(vm.AddScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanEditScheduledExportTest_Success()
        {
            var vm = new AuditLogScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = new AuditLogScheduledExportDomain();
            Assert.IsTrue(vm.EditScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanEditScheduledExportTest_Failure()
        {
            var vm = new AuditLogScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = null;
            Assert.IsFalse(vm.EditScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanDeleteScheduledExportTest_Success()
        {
            var vm = new AuditLogScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = new AuditLogScheduledExportDomain();
            Assert.IsTrue(vm.DeleteScheduledExport.CanExecute(null));
        }

        [Test]
        public void CanDeleteScheduledExportTest_Failure()
        {
            var vm = new AuditLogScheduledExportsViewModel(_mockExportService.Object);
            vm.SelectedScheduledExport = null;
            Assert.IsFalse(vm.DeleteScheduledExport.CanExecute(null));
        }
    }
}