using NUnit.Framework;
using ScoutViewModels.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Moq;
using Ninject;
using ScoutDomains.Analysis;
using ScoutServices.Interfaces;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutViewModels.Interfaces;
using TestSupport;

namespace ScoutViewModels.ViewModels.Dialogs.Tests
{
    [TestFixture()]
    public class AddQualityControlDialogViewModelTests : BaseTest
    {
        private IScoutViewModelFactory _vmFactory;
        private Mock<ICellTypeManager> _cellTypeManagerMock;

        [SetUp]
        public void Setup()
        {
            _cellTypeManagerMock = new Mock<ICellTypeManager>();
            _cellTypeManagerMock
                .Setup(m => m.GetAllCellTypes())
                .Returns(ObjectsForTesting.GetFactoryCellTypes());

            Kernel = new StandardKernel(new ScoutViewModelsModule());
            Kernel.Bind<ICellTypeManager>().ToConstant(_cellTypeManagerMock.Object);
            
            _vmFactory = Kernel.Get<IScoutViewModelFactory>();
            
            SetUser(Username, UserPermissionLevel.eElevated);
        }

        [TestCase(assay_parameter.ap_Concentration)]
        [TestCase(assay_parameter.ap_PopulationPercentage)]
        [TestCase(assay_parameter.ap_Size)]
        public void CheckSelectedAssayParameterUpdatesQualityControl(assay_parameter newParam) // PC3549-5128
        {
            // Arrange
            var args = new AddCellTypeEventArgs(0);
            var vm = _vmFactory.CreateAddQualityControlDialogViewModel(args, null);
            Assert.IsNotNull(vm);
            Assert.AreEqual(assay_parameter.ap_Concentration, vm.SelectedAssayParameter);
            Assert.AreEqual(assay_parameter.ap_Concentration, vm.QualityControl.AssayParameter);

            // Act
            vm.SelectedAssayParameter = newParam;

            // Assert
            Assert.AreEqual(newParam, vm.SelectedAssayParameter);
            Assert.AreEqual(newParam, vm.QualityControl.AssayParameter);
        }
    }
}