using System;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutUtilities.Enums;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class QualityControlDomainToQualityControlTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void QualityControlDomainToQualityControlTest_QcName()
        {
            var qc = GetGoodQCDomain();
            var map = new QualityControl();

            PropertyReflectionTest(qc, map, "My qc name",
                nameof(QualityControlDomain.QcName), nameof(QualityControl.QualityControlName));
            PropertyReflectionTest(qc, map, string.Empty,
                nameof(QualityControlDomain.QcName), nameof(QualityControl.QualityControlName));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_AcceptanceLimits()
        {
            var qc = GetGoodQCDomain();
            var map = new QualityControl();

            qc.AcceptanceLimit = 2654;
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(qc.AcceptanceLimit, map.AcceptanceLimits);

            qc.AcceptanceLimit = 56;
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(qc.AcceptanceLimit, map.AcceptanceLimits);

            qc.AcceptanceLimit = null;
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(int), map.AcceptanceLimits);
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_AssayParameter()
        {
            var qc = GetGoodQCDomain();
            var map = new QualityControl();

            PropertyReflectionTest(qc, map, assay_parameter.ap_Concentration,
                nameof(QualityControlDomain.AssayParameter), nameof(QualityControl.AssayParameter));
            PropertyReflectionTest(qc, map, assay_parameter.ap_PopulationPercentage,
                nameof(QualityControlDomain.AssayParameter), nameof(QualityControl.AssayParameter));
            PropertyReflectionTest(qc, map, assay_parameter.ap_Size,
                nameof(QualityControlDomain.AssayParameter), nameof(QualityControl.AssayParameter));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_LotNumber()
        {
            var qc = GetGoodQCDomain();
            var map = new QualityControl();

            PropertyReflectionTest(qc, map, "my lot number",
                nameof(QualityControlDomain.LotInformation), nameof(QualityControl.LotNumber));
            PropertyReflectionTest(qc, map, string.Empty,
                nameof(QualityControlDomain.LotInformation), nameof(QualityControl.LotNumber));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_Comments()
        {
            var qc = GetGoodQCDomain();
            var map = new QualityControl();

            PropertyReflectionTest(qc, map, "my comments",
                nameof(QualityControlDomain.CommentText), nameof(QualityControl.Comments));
            PropertyReflectionTest(qc, map, string.Empty,
                nameof(QualityControlDomain.CommentText), nameof(QualityControl.Comments));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_AssayValue()
        {
            var qc = GetGoodQCDomain();
            var map = new QualityControl();

            qc.AssayValue = 2654.321;
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(qc.AssayValue, map.AssayValue);

            qc.AssayValue = -1256.54678;
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(qc.AssayValue, map.AssayValue);

            qc.AssayValue = null;
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(default(double), map.AssayValue);
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_ExpirationDate()
        {
            var qc = GetGoodQCDomain();
            var map = new QualityControl();

            qc.ExpirationDate = DateTime.UtcNow;
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(qc.ExpirationDate, map.ExpirationDate.ToDateTime());

            qc.ExpirationDate = DateTime.MinValue.ToUniversalTime();
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(qc.ExpirationDate, map.ExpirationDate.ToDateTime());

            qc.ExpirationDate = DateTime.MaxValue.ToUniversalTime();
            map = Mapper.Map<QualityControl>(qc);
            Assert.IsNotNull(map);
            Assert.AreEqual(qc.ExpirationDate, map.ExpirationDate.ToDateTime());
        }

        private QualityControlDomain GetGoodQCDomain()
        {
            return new QualityControlDomain
            {
                AcceptanceLimit = 10,
                AssayParameter = assay_parameter.ap_Concentration,
                AssayValue = 5.0,
                CellTypeIndex = 0,
                CellTypeName = "BCI Default",
                CommentText = "This is a test",
                ExpirationDate = DateTime.Now.AddDays(20),
                LotInformation = "121",
                QcName = "TestQC"
            };
        }
    }
}