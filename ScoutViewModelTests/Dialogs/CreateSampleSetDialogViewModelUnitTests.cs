using HawkeyeCoreAPI.Facade;
using HawkeyeCoreAPI.Interfaces;
using Moq;
using NUnit.Framework;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutModels;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Dialogs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace ScoutViewModelTests.Dialogs
{
    [TestFixture]
    public class CreateSampleSetDialogViewModelUnitTests
    {
        [Test]
        public void TestAutoCupVmWellIsClicked()
        {
            var allowCarrierTypeToChange = true;
            var userIsServiceUser = true;

            var vm = CreateSampleSetDialogViewModel("user", "sample", "set", allowCarrierTypeToChange,
                userIsServiceUser);
            Assert.IsNotNull(vm);
            vm.SelectedPlateType = eSubstrateType.eAutomationCup;
            var well = vm.AutoCupVm.SampleWellButtons.FirstOrDefault();

            // check base conditions
            Assert.AreEqual(3, vm.PlateTypes.Count);
            Assert.IsTrue(vm.PlateTypes.Contains(eSubstrateType.eAutomationCup));
            Assert.AreEqual(eSubstrateType.eAutomationCup, vm.SelectedPlateType);
            Assert.IsNotNull(vm.AutoCupVm);
            Assert.IsNotNull(vm.AutoCupVm.SampleWellButtons);
            Assert.AreEqual(1, vm.AutoCupVm.SampleWellButtons.Count);
            Assert.AreEqual(RowPositionHelper.GetRowChar(RowPosition.Y), vm.AutoCupVm.SampleWellButtons.FirstOrDefault()?.SamplePosition.Row);
            Assert.AreEqual((byte) 1, vm.AutoCupVm.SampleWellButtons.FirstOrDefault()?.SamplePosition.Column);
            Assert.AreEqual(0, vm.AutoCupVm.SampleSet.Samples.Count);

            Assert.IsNotNull(vm.SampleTemplate);
            Assert.IsNotNull(vm.SampleTemplate.AdvancedSampleSettings);
            Assert.IsNotNull(vm.AutoCupVm.RunOptionSettings);
            Assert.IsNotNull(vm.AutoCupVm.RunOptionSettings.RunOptionsSettings);

            Assert.IsNotNull(well);
            Assert.IsNull(well.Sample);
            Assert.AreEqual(SampleWellState.Available, well.WellState);
            Assert.IsNull(vm.AutoCupVm.LastSampleWellAdded);

            // perform select action to test:
            vm.AutoCupVm.OnClicked.Execute(well);

            // check results
            Assert.IsNotNull(well.Sample);
            Assert.AreEqual(SampleWellState.UsedInCurrentSet, well.WellState);
            Assert.IsNotNull(vm.AutoCupVm.LastSampleWellAdded);
            Assert.IsTrue(vm.CanAccept()); // ensure the user can click the "Add" button

            // perform deselect action to test:
            vm.AutoCupVm.OnClicked.Execute(well); // click again to deselect the sample from the well

            Assert.IsNull(well.Sample);
            Assert.AreEqual(SampleWellState.Available, well.WellState);
            Assert.IsFalse(vm.CanAccept()); // ensure the user can NOT click the "Add" button
        }

        [Test]
        public void TestAutoCupVmWellIsClickedAndAddedToSampleSet()
        {
            var allowCarrierTypeToChange = true;
            var userIsServiceUser = true;

            var vm = CreateSampleSetDialogViewModel("user", "sample", "set", allowCarrierTypeToChange,
                userIsServiceUser);
            Assert.IsNotNull(vm);
            vm.SelectedPlateType = eSubstrateType.eAutomationCup;
            var well = vm.AutoCupVm.SampleWellButtons.FirstOrDefault();

            Assert.AreEqual(0, vm.AutoCupVm.SampleSet.Samples.Count);

            vm.AutoCupVm.OnClicked.Execute(well);
            vm.AcceptCommand.Execute(null);

            // for another test:
            Assert.AreEqual(1, vm.AutoCupVm.SampleSet.Samples.Count);
        }

        [Test]
        public void TestCreateSampleSetVm_DoesNotShowAutomationCupInDropdown()
        {
            var allowCarrierTypeToChange = true;
            var userIsServiceUser = false;

            var vm = CreateSampleSetDialogViewModel("user", "sample", "set", allowCarrierTypeToChange,
                userIsServiceUser);

            Assert.IsNotNull(vm);
            Assert.AreEqual(2, vm.PlateTypes.Count);
            Assert.IsFalse(vm.PlateTypes.Contains(eSubstrateType.eAutomationCup));
        }

        [Test]
        public void TestCreateSampleSetVm_DoesNotAllowAutomationCupToBeSelectedWithNonServiceUser()
        {
            var allowCarrierTypeToChange = true;
            var userIsServiceUser = false;
            
            var vm = CreateSampleSetDialogViewModel("user", "sample", "set", allowCarrierTypeToChange, 
                userIsServiceUser);

            Assert.AreEqual(eSubstrateType.eCarousel, vm.SelectedPlateType);
            vm.SelectedPlateType = eSubstrateType.eAutomationCup;
            Assert.AreEqual(eSubstrateType.eCarousel, vm.SelectedPlateType);
        }

        private static CreateSampleSetDialogViewModel CreateSampleSetDialogViewModel(string user, string sampleName, 
            string setName, bool allowCarrierTypeToChange, bool userIsServiceUser)
        {
            var colorServiceMock = new Mock<ISolidColorBrushService>();
            colorServiceMock.Setup(m => m.GetBrushForColor(It.IsAny<string>())).Returns(new SolidColorBrush(Colors.Blue));

            uint numberOfCellTypes = 0;
            var cellTypeDomainList = new List<CellTypeDomain>();
            var cellTypeAccessMock = new Mock<ICellType>();
            cellTypeAccessMock.Setup(m => m.GetAllCellTypesAPI(ref numberOfCellTypes, ref cellTypeDomainList))
                              .Returns(HawkeyeError.eSuccess);

            uint numberOfQuality = 0;
            var qualDomainList = new List<QualityControlDomain>();
            var qualTypeAccessMock = new Mock<IQualityControl>();
            qualTypeAccessMock.Setup(m => m.GetQualityControlListAPI(ref qualDomainList, out numberOfQuality))
                              .Returns(HawkeyeError.eSuccess);
            qualTypeAccessMock.Setup(m => m.GetActiveQualityControlListAPI(ref qualDomainList, out numberOfQuality))
                              .Returns(HawkeyeError.eSuccess);

            CellTypeFacade.Instance.SetCellTypeAccess(cellTypeAccessMock.Object, qualTypeAccessMock.Object);

            var dataAccessMock = new Mock<IDataAccess>();

            dataAccessMock.Setup(m => m.ReadConfigurationData<List<GenericParametersDomain>>(
                              It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                          .Returns(new List<GenericParametersDomain>());

            var optionsModel = new RunOptionSettingsModel(dataAccessMock.Object, user);
            dataAccessMock.Setup(m => m.ReadConfigurationData<RunOptionSettingsModel>(
                              It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                          .Returns(optionsModel);

            var runOptions = new RunOptionSettingsModel(dataAccessMock.Object, user);
            runOptions.RunOptionsSettings = new RunOptionSettingsDomain();
            runOptions.SavedSettings = new RunOptionSettingsDomain();

            var instrStatusMock = new Mock<ISystemStatus>();
            var sysStatus = new SystemStatusData();
            instrStatusMock.Setup(m => m.GetSystemStatusAPI(ref sysStatus)).Returns(IntPtr.Zero);

            var errorMock = new Mock<IErrorLog>();
            var str = string.Empty;
            errorMock.Setup(m => m.SystemErrorCodeToExpandedResourceStringsAPI(It.IsAny<uint>(),
                ref str, ref str, ref str, ref str,
                ref str));

            InstrumentStatusModel.Instance.SetSystemStatusService(instrStatusMock.Object,
                errorMock.Object);
            InstrumentStatusModel.Instance.UpdateSystemStatus();

            InstrumentStatusModel.Instance.SystemStatusDom.SamplePosition = SamplePosition.Parse("A1");

            var ct = new CellTypeQualityControlGroupDomain();
            var list = new ObservableCollection<CellTypeQualityControlGroupDomain> { ct };
            var eList = new ObservableCollection<eSamplePostWash> { eSamplePostWash.eNormalWash };
            var seq = new SequentialNamingSetViewModel(sampleName);
            var sampleDomain = new SampleDomain();
            var settings = new AdvancedSampleSettingsViewModel(sampleDomain, runOptions);
            var sampleTemplate = new SampleTemplateViewModel(list, list.FirstOrDefault(), 1, eList,
                eList.FirstOrDefault(), string.Empty, false, seq, settings, runOptions);
            var set = new SampleSetViewModel(null, sampleTemplate, user, user, setName);
            var args = new CreateSampleSetEventArgs<SampleSetViewModel>(set, user, user,
                setName, eSampleSetStatus.Pending, allowCarrierTypeToChange, eSubstrateType.eCarousel, userIsServiceUser,
                WorkListStatus.Idle);

            var vm = new CreateSampleSetDialogViewModel(args, null, colorServiceMock.Object, runOptions);
            return vm;
        }

    }
}
