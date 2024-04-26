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
    public class QualityControlToQualityControlDomainTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void QualityControlToQualityControlDomainTest_Name()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            PropertyReflectionTest(qcObject, map, "My qc name",
                nameof(QualityControl.QualityControlName), nameof(QualityControlDomain.QcName));
            PropertyReflectionTest(qcObject, map, string.Empty,
                nameof(QualityControl.QualityControlName), nameof(QualityControlDomain.QcName));
        }

        [Test]
        public void QualityControlToQualityControlDomainTest_CellTypeName()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            PropertyReflectionTest(qcObject, map, "My CellType name",
                nameof(QualityControl.CellTypeName), nameof(QualityControlDomain.CellTypeName));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_AcceptanceLimits()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            qcObject = new QualityControl { AcceptanceLimits = 2654 };
            map = Mapper.Map<QualityControlDomain>(qcObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(qcObject.AcceptanceLimits, map.AcceptanceLimit);
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_AssayParameter()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            PropertyReflectionTest(qcObject, map, AssayParameterEnum.Concentration,
                nameof(QualityControl.AssayParameter), nameof(QualityControlDomain.AssayParameter));
            PropertyReflectionTest(qcObject, map, AssayParameterEnum.PopulationPercentage,
                nameof(QualityControl.AssayParameter), nameof(QualityControlDomain.AssayParameter));
            PropertyReflectionTest(qcObject, map, AssayParameterEnum.Size,
                nameof(QualityControl.AssayParameter), nameof(QualityControlDomain.AssayParameter));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_LotNumber()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            PropertyReflectionTest(qcObject, map, "my lot number",
                nameof(QualityControl.LotNumber), nameof(QualityControlDomain.LotInformation));
            PropertyReflectionTest(qcObject, map, string.Empty,
                nameof(QualityControl.LotNumber), nameof(QualityControlDomain.LotInformation));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_Comments()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            PropertyReflectionTest(qcObject, map, "my comments",
                nameof(QualityControl.Comments), nameof(QualityControlDomain.CommentText));
            PropertyReflectionTest(qcObject, map, string.Empty,
                nameof(QualityControl.Comments), nameof(QualityControlDomain.CommentText));
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_AssayValue()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            qcObject = new QualityControl { AssayValue = 2654.321 };
            map = Mapper.Map<QualityControlDomain>(qcObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(qcObject.AssayValue, map.AssayValue);

            qcObject = new QualityControl { AssayValue = -1256.54678 };
            map = Mapper.Map<QualityControlDomain>(qcObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(qcObject.AssayValue, map.AssayValue);
        }

        [Test]
        public void QualityControlDomainToQualityControlTest_ExpirationDate()
        {
            var qcObject = new QualityControl();
            var map = new QualityControlDomain();

            qcObject.ExpirationDate = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime());
            map = Mapper.Map<QualityControlDomain>(qcObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(qcObject.ExpirationDate.ToDateTime(), map.ExpirationDate);

            qcObject.ExpirationDate = Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime());
            map = Mapper.Map<QualityControlDomain>(qcObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(qcObject.ExpirationDate.ToDateTime(), map.ExpirationDate);

            qcObject.ExpirationDate = Timestamp.FromDateTime(DateTime.MaxValue.ToUniversalTime());
            map = Mapper.Map<QualityControlDomain>(qcObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(qcObject.ExpirationDate.ToDateTime(), map.ExpirationDate);
        } 
    } 
}