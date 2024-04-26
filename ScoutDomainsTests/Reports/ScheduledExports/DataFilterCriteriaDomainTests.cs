using NUnit.Framework;
using System;

namespace ScoutDomains.Reports.ScheduledExports.Tests
{
    [TestFixture]
    public class DataFilterCriteriaDomainTests
    {
        [Test]
        public void DataFilterIsValidTest_FromDateToDate_Success()
        {
            var data = new DataFilterCriteriaDomain();
            data.IsAllCellTypeSelected = true;
            data.SelectedUsername = "Test";

            var now = DateTime.Now;
            data.FromDate = now;
            data.ToDate = now.AddMilliseconds(10);
            Assert.IsTrue(data.DataFilterIsValid());
        }

        [TestCase("Test", ExpectedResult = true)]
        [TestCase("", ExpectedResult = false)]
        public bool DataFilterIsValidTest_SelectedUsername(string username)
        {
            var data = new DataFilterCriteriaDomain();
            data.IsAllCellTypeSelected = true;
            data.FromDate = DateTime.Now;
            data.ToDate = DateTime.Now.AddMilliseconds(10);

            data.SelectedUsername = username;
            return data.DataFilterIsValid();
        }

        [Test]
        public void DataFilterIsValidTest_SelectedCellType_Success()
        {
            var data = new DataFilterCriteriaDomain();
            data.IsAllCellTypeSelected = false;
            data.FromDate = DateTime.Now;
            data.ToDate = DateTime.Now.AddMilliseconds(10);
            data.SelectedUsername = "Test";

            data.SelectedCellTypeOrQualityControlGroup = new CellTypeQualityControlGroupDomain();

            Assert.IsTrue(data.DataFilterIsValid());
        }

        [Test]
        public void DataFilterIsValidTest_SelectedCellType_Failure()
        {
            var data = new DataFilterCriteriaDomain();
            data.IsAllCellTypeSelected = false;
            data.FromDate = DateTime.Now;
            data.ToDate = DateTime.Now.AddMilliseconds(10);
            data.SelectedUsername = "Test";

            data.SelectedCellTypeOrQualityControlGroup = null;

            Assert.IsFalse(data.DataFilterIsValid());
        }
    }
}
