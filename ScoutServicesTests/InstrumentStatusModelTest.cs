// ***********************************************************************
// Assembly         : ScoutModelsTests
// Author           : 40001533
// Created          : 1-03-2019
//
// Last Modified By : 40001533
// Last Modified On : 1-03-2019
// ***********************************************************************
// <copyright file="InstrumentStatusModelTest.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Windows.Forms;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.Enums;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace ScoutModelsTests.InstrumentStatus
{
    [TestFixture]
    public class InstrumentStatusServiceTest
    {
        private IKernel _kernel;

        [SetUp]
        public void Setup()
        {
            var settings = new NinjectSettings()
            {
                LoadExtensions = false
            };
            _kernel = new StandardKernel();
        }
        [Test]
        public void GetSystemErrorLog_ShouldLogAndReturnSystemErrorDomainIfApplicationLanguageIsNull()
        {
            var hardwareSettingsMock = new Mock<IHardwareSettingsModel>();
            string serialNumber = "12345";
            hardwareSettingsMock.Setup(m => m.GetSystemSerialNumber(ref serialNumber)).Returns(HawkeyeError.eSuccess);
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            InstrumentStatusService mockInstrumentStatusService = new InstrumentStatusService(
                new Mock<ISystemStatus>().Object,
                new Mock<IErrorLog>().Object, new Mock<ILogger>().Object, hardwareSettingsMock.Object, applicationStateServiceMock.Object);

            //Act
            var systemErrorDomain = mockInstrumentStatusService.SystemErrorCodeToExpandedResourceStrings(16908290);

            //Assert
            Assert.IsNotNull(systemErrorDomain);
            Assert.IsEmpty(systemErrorDomain.SeverityKey);
            Assert.IsEmpty(systemErrorDomain.SeverityDisplayValue);
            Assert.IsEmpty(systemErrorDomain.System);
            Assert.IsEmpty(systemErrorDomain.SubSystem);
            Assert.IsEmpty(systemErrorDomain.Instance);
            Assert.IsEmpty(systemErrorDomain.FailureMode);
        }

    }
}
