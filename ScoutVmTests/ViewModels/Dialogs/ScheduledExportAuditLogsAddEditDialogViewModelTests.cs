using ScoutViewModels.ViewModels.Dialogs;
using HawkeyeCoreAPI.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Reports.ScheduledExports;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutModels.Security;
using ScoutModels.Settings;
using ScoutServices;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Services;
using ScoutUtilities.Structs;
using ScoutViewModels.Interfaces;
using System;
using System.Collections.Generic;
using ScoutUtilities.Events;

namespace ScoutViewModels.ViewModels.Dialogs.Tests
{
    [TestFixture()]
    public class ScheduledExportAuditLogsAddEditDialogViewModelTests
    {
        private StandardKernel _kernel;
        private static InstrumentStatusService _instrumentStatusService;
        private static Mock<IErrorLog> _errorMock;
        private static Mock<ISystemStatus> _instrStatusMock;
        private static Mock<IDialogCaller> _dialogCallerMock;
        private static Mock<IFileSystemService> _fileSystemMock;
        public string Username = "username";
        public string Password = "password";

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel(new ScoutViewModelsModule());
            _kernel.Bind<ILockManager>().To<LockManager>().InSingletonScope();
            _kernel.Bind<IOpcUaCfgManager>().To<OpcUaCfgManager>().InSingletonScope();
            _kernel.Bind<IDbSettingsService>().To<DbSettingsModel>().InSingletonScope();
            _kernel.Bind<ISmtpSettingsService>().To<SmtpSettingsModel>().InSingletonScope();
            _kernel.Bind<IAutomationSettingsService>().To<AutomationSettingsService>().InSingletonScope();
            _kernel.Bind<ISampleProcessingService>().To<SampleProcessingService>().InSingletonScope();
            _kernel.Bind<ISecurityService>().To<SecurityService>().InSingletonScope();

            var ctManagerMock = new Mock<ICellTypeManager>();
            ctManagerMock.Setup(m => m.GetCellTypeDomain(Username, Password, It.IsAny<string>())).Returns(new CellTypeDomain());
            ctManagerMock.Setup(m => m.GetQualityControlDomain(Username, Password, It.IsAny<string>())).Returns(new QualityControlDomain());

            _dialogCallerMock = new Mock<IDialogCaller>();

            _kernel.Bind<ICellTypeManager>().ToConstant(ctManagerMock.Object);
            _kernel.Bind<IDialogCaller>().ToConstant(_dialogCallerMock.Object);
            _kernel.Bind<IConfigurationManager>().To<ConfigurationManager>().InSingletonScope();
            _kernel.Bind<IScheduledExportsService>().To<ScheduledExportsService>().InSingletonScope();
            
            _instrStatusMock = new Mock<ISystemStatus>();
            _errorMock = new Mock<IErrorLog>();
            var loggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var userService = new Mock<IUserService>();
            userService.Setup(m => m.GetUserList()).Returns(new List<UserDomain>
            {
                new UserDomain("test1"),
                new UserDomain("test2")
            });
            _kernel.Bind<IUserService>().ToConstant(userService.Object);

            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            _instrumentStatusService = new InstrumentStatusService(_instrStatusMock.Object, _errorMock.Object, loggerMock.Object, applicationStateServiceMock.Object);

            _kernel.Bind<IInstrumentStatusService>().ToConstant(_instrumentStatusService);

            _dialogCallerMock.Reset();
        }

        private void UseDefaultFileSystemService()
        {
            _kernel.Bind<IFileSystemService>().To<FileSystemService>().InSingletonScope();
        }

        private void UseMockedFileSystemService()
        {
            _fileSystemMock = new Mock<IFileSystemService>();
            _kernel.Bind<IFileSystemService>().ToConstant(_fileSystemMock.Object);

            _fileSystemMock.Reset();
            _fileSystemMock
                .Setup(m => m.FolderIsValidForScheduledExport(It.IsAny<string>()))
                .Returns(true);
            _fileSystemMock
                .Setup(m => m.DirectoryExists(It.IsAny<string>()))
                .Returns(true);
            _fileSystemMock
                .Setup(m => m.FileNameIsValid(It.IsAny<string>()))
                .Returns(true);
        }

        [Test()]
        public void FieldsAreValidTest_Success()
        {
            UseMockedFileSystemService();
            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);

            vm.ScheduledExport = GetScheduledExportDomain();
            Assert.IsTrue(vm.FieldsAreValid());
        }

        [Test()]
        public void FieldsAreValidTest_Success_BlankEmail()
        {
            UseMockedFileSystemService();
            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);

            vm.ScheduledExport = GetScheduledExportDomain();
            vm.ScheduledExport.NotificationEmail = string.Empty;
            Assert.IsTrue(vm.FieldsAreValid());
        }

        [Test()]
        public void FieldsAreValidTest_Failure_BadEmail()
        {
            UseDefaultFileSystemService();
            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);

            vm.ScheduledExport = GetScheduledExportDomain();
            vm.ScheduledExport.NotificationEmail = "notAnEmailAddress@com";
            Assert.IsFalse(vm.FieldsAreValid());
        }

        [Test()]
        public void FieldsAreValidTest_Failure_DirNotExist()
        {
            UseDefaultFileSystemService();
            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);

            vm.ScheduledExport = GetScheduledExportDomain();
            vm.ScheduledExport.DestinationFolder = "C:\\IAmNotARealFolder-IamFake";
            Assert.IsFalse(vm.FieldsAreValid());
        }

        [Test()]
        public void FieldsAreValidTest_Failure_FilenameEmpty()
        {
            UseDefaultFileSystemService();
            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);

            vm.ScheduledExport = GetScheduledExportDomain();
            vm.ScheduledExport.FilenameTemplate = string.Empty;
            Assert.IsFalse(vm.FieldsAreValid());
        }

        [Test()]

        public void FieldsAreValidTest_Success_FilenameInvalid()
        {
            UseDefaultFileSystemService();
            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);

            string badname = "invalid&file*name";

            vm.ScheduledExport = GetScheduledExportDomain();
            vm.ScheduledExport.FilenameTemplate = badname;
            Assert.IsFalse(vm.ScheduledExport.FilenameTemplate.Equals(badname));
            Assert.IsTrue(vm.FieldsAreValid());
        }

        [Test()]
        public void FieldsAreValidTest_Failure_BadRecurrenceRules()
        {
            UseDefaultFileSystemService();
            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);

            vm.ScheduledExport = GetScheduledExportDomain();
            vm.ScheduledExport.RecurrenceRule.SelectedClockFormat = ClockFormat.AM;
            vm.ScheduledExport.RecurrenceRule.Hour = 23;
            Assert.IsFalse(vm.FieldsAreValid());
        }

        [Test()]
        public void ScheduledExport_DirectoryUpdatesWithFolderSelectDialog()
        {
            // Arrange
            UseDefaultFileSystemService();
            var initialPath = @"C:\Instrument\Export";
            var newPath = @"C:\Instrument\Export\admin2";
            _dialogCallerMock
                .Setup(m => m.OpenFolderSelectDialog(It.IsAny<object>(), It.IsAny<FolderSelectDialogEventArgs>()))
                .Callback<object, FolderSelectDialogEventArgs>((o, f) => f.SelectedFolderPath = newPath)
                .Returns(true);

            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);
            vm.ScheduledExport.DestinationFolder = initialPath;
            Assert.AreEqual(initialPath, vm.ScheduledExport.DestinationFolder);

            // Act (calls _dialogCallerMock.OpenFolderSelectDialog)
            vm.SelectExportDirectoryCommand.Execute(null);

            // Assert
            Assert.AreNotEqual(initialPath, vm.ScheduledExport.DestinationFolder);
            Assert.AreEqual(newPath, vm.ScheduledExport.DestinationFolder);
        }

        [Test()]
        public void ScheduledExport_DirectoryDoesNotUpdateWhenFolderSelectDialogCancelled()
        {
            // Arrange
            UseDefaultFileSystemService();
            var initialPath = @"C:\Instrument\Export";
            var newPath = @"C:\Instrument\Export\admin2";
            _dialogCallerMock
                .Setup(m => m.OpenFolderSelectDialog(It.IsAny<object>(), It.IsAny<FolderSelectDialogEventArgs>()))
                .Callback<object, FolderSelectDialogEventArgs>((o, f) => f.SelectedFolderPath = newPath)
                .Returns(false);

            var exportDomain = new AuditLogScheduledExportDomain();
            var args = new ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain>(exportDomain, false);
            var vm = _kernel.Get<IScoutViewModelFactory>()
                            .CreateScheduledExportAuditLogsAddEditViewModel(args, null);
            Assert.IsNotNull(vm);
            vm.ScheduledExport.DestinationFolder = initialPath;
            Assert.AreEqual(initialPath, vm.ScheduledExport.DestinationFolder);

            // Act (calls _dialogCallerMock.OpenFolderSelectDialog)
            vm.SelectExportDirectoryCommand.Execute(null);

            // Assert
            Assert.AreNotEqual(newPath, vm.ScheduledExport.DestinationFolder);
            Assert.AreEqual(initialPath, vm.ScheduledExport.DestinationFolder);
        }

        private AuditLogScheduledExportDomain GetScheduledExportDomain()
        {
            var now = DateTime.Now;

            var export = new AuditLogScheduledExportDomain();
            export.Uuid = new uuidDLL(Guid.NewGuid());
            export.Name = "OG name";
            export.Comments = "OG comments";
            export.FilenameTemplate = "OG_Filename";
            export.DestinationFolder = "C:\\Instrument\\Export"; // this is a real folder
            export.IsEnabled = true;
            export.IncludeAuditLog = true;
            export.IncludeErrorLog = true;
            export.NotificationEmail = "my@email.com";
            export.LastRunStatus = ScheduledExportLastRunStatus.Success;

            export.DataFilterCriteria.FromDate = now.AddDays(-10);
            export.DataFilterCriteria.ToDate = now;

            export.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Monthly;
            export.RecurrenceRule.Hour = 1;
            export.RecurrenceRule.Minutes = 23;
            export.RecurrenceRule.SelectedClockFormat = ClockFormat.PM;
            export.RecurrenceRule.DayOfTheMonth = 20;

            return export;
        }
    }
}