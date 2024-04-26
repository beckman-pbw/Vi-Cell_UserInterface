using System;
using System.Linq;
using System.Windows.Media.Imaging;
using AutoMapper;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GrpcServer;
using GrpcService;
using Ninject;
using NUnit.Framework;
using ScoutDomains.DataTransferObjects;
using ScoutServices.Ninject;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using SampleStatus = GrpcService.SampleStatusEnum;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class SimpleTypesAutoMapperTests
    {
        private IKernel _kernel;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel(new ScoutServiceModule(), new OpcUaGrpcModule());
            _mapper = _kernel.Get<IMapper>();
            Assert.IsNotNull(_mapper);
        }

        [Test]
        public void StringToGuidTests()
        {
            var str = string.Empty;
            var map = Guid.Empty;

            str = Guid.NewGuid().ToString();
            map = _mapper.Map<Guid>(str);

            Assert.IsNotNull(map);
            Assert.AreEqual(str, map.ToString());

            str = string.Empty;
            map = _mapper.Map<Guid>(str);
            Assert.IsNotNull(map);
            Assert.AreEqual(Guid.Empty, map);

            str = null;
            map = _mapper.Map<Guid>(str);
            Assert.IsNotNull(map);
            Assert.AreEqual(Guid.Empty, map);
        }

        [Test]
        public void GuidToStringTests()
        {
            var guid = Guid.Empty;
            var map = string.Empty;

            guid = Guid.Empty;
            map = _mapper.Map<string>(guid);
            Assert.IsNotNull(map);
            Assert.AreEqual(Guid.Empty.ToString().ToUpper(), map.ToUpper());

            guid = Guid.NewGuid();
            map = _mapper.Map<string>(guid);
            Assert.IsNotNull(map);
            Assert.AreEqual(guid.ToString().ToUpper(), map.ToUpper());
        }

        [Test]
        public void StringToUuidDllTests()
        {
            var str = string.Empty;
            var map = new uuidDLL();

            str = Guid.NewGuid().ToString();
            map = _mapper.Map<uuidDLL>(str);

            Assert.IsNotNull(map);
            Assert.AreEqual(str.ToUpper(), map.ToString());

            str = string.Empty;
            map = _mapper.Map<uuidDLL>(str);
            Assert.IsNotNull(map);
            Assert.AreEqual(Guid.Empty.ToString().ToUpper(), map.ToString());

            str = null;
            map = _mapper.Map<uuidDLL>(str);
            Assert.IsNotNull(map);
            Assert.AreEqual(string.Empty, map.ToString());
        }

        [Test]
        public void UuidDllToStringTests()
        {
            var guid = new uuidDLL();
            var map = string.Empty;

            guid = new uuidDLL(Guid.NewGuid());
            map = _mapper.Map<string>(guid);
            Assert.IsNotNull(map);
            Assert.AreEqual(guid.ToString(), map.ToUpper());

            guid = new uuidDLL(Guid.Empty);
            map = _mapper.Map<string>(guid);
            Assert.IsNotNull(map);
            Assert.AreEqual(guid.ToString(), map.ToUpper());

            guid = new uuidDLL();
            map = _mapper.Map<string>(guid);
            Assert.IsNotNull(map);
            Assert.AreEqual(Guid.Empty.ToString().ToUpper(), map.ToUpper());
        }

        [Test]
        public void GuidToUuidDllTests()
        {
            var guid = Guid.Empty;
            var map = new uuidDLL();

            guid = Guid.NewGuid();
            map = _mapper.Map<uuidDLL>(guid);

            Assert.IsNotNull(map);
            Assert.AreEqual(guid.ToString().ToUpper(), map.ToString());

            guid = Guid.Empty;
            map = _mapper.Map<uuidDLL>(guid);
            Assert.IsNotNull(map);
            Assert.AreEqual(guid.ToString().ToUpper(), map.ToString());
        }

        [Test]
        public void UuidDllToGuidTests()
        {
            var uuid = new uuidDLL();
            var map = Guid.Empty;

            uuid = new uuidDLL(Guid.NewGuid());
            map = _mapper.Map<Guid>(uuid);
            Assert.IsNotNull(map);
            Assert.AreEqual(uuid.ToString(), map.ToString().ToUpper());

            uuid = new uuidDLL(Guid.Empty);
            map = _mapper.Map<Guid>(uuid);
            Assert.IsNotNull(map);
            Assert.AreEqual(uuid.ToString(), map.ToString().ToUpper());

            uuid = new uuidDLL();
            map = _mapper.Map<Guid>(uuid);
            Assert.IsNotNull(map);
            Assert.AreEqual(Guid.Empty.ToString().ToUpper(), map.ToString().ToUpper());
        }

        [Test]
        public void DateTimeToTimestampTests()
        {
            var dateTime = DateTime.MinValue;
            var map = new Timestamp();

            dateTime = DateTime.MinValue.ToLocalTime();
            map = _mapper.Map<Timestamp>(dateTime);
            Assert.IsNotNull(map);
            Assert.AreEqual(dateTime, map.ToDateTime());

            dateTime = DateTime.MinValue.ToUniversalTime();
            map = _mapper.Map<Timestamp>(dateTime);
            Assert.IsNotNull(map);
            Assert.AreEqual(dateTime, map.ToDateTime());

            dateTime = DateTime.MaxValue.ToLocalTime();
            map = _mapper.Map<Timestamp>(dateTime);
            Assert.IsNotNull(map);
            Assert.AreEqual(dateTime, map.ToDateTime());

            dateTime = DateTime.MaxValue.ToUniversalTime();
            map = _mapper.Map<Timestamp>(dateTime);
            Assert.IsNotNull(map);
            Assert.AreEqual(dateTime, map.ToDateTime());

            dateTime = DateTime.Now.ToLocalTime();
            map = _mapper.Map<Timestamp>(dateTime);
            Assert.IsNotNull(map);
            Assert.AreEqual(dateTime, map.ToDateTime());

            dateTime = DateTime.Now.ToUniversalTime();
            map = _mapper.Map<Timestamp>(dateTime);
            Assert.IsNotNull(map);
            Assert.AreEqual(dateTime, map.ToDateTime());
        }

        [Test]
        public void TimestampToDateTimeTests()
        {
            var timestamp = new Timestamp();
            var map = DateTime.MinValue;

            timestamp = Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime());
            map = _mapper.Map<DateTime>(timestamp);
            Assert.IsNotNull(map);
            Assert.AreEqual(timestamp.ToDateTime(), map);

            timestamp = Timestamp.FromDateTime(DateTime.MaxValue.ToUniversalTime());
            map = _mapper.Map<DateTime>(timestamp);
            Assert.IsNotNull(map);
            Assert.AreEqual(timestamp.ToDateTime(), map);

            timestamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime());
            map = _mapper.Map<DateTime>(timestamp);
            Assert.IsNotNull(map);
            Assert.AreEqual(timestamp.ToDateTime(), map);
        }

        [Test]
        public void LockStateEnumToLockResultTests()
        {
            var lockState = LockStateEnum.Locked;
            var map = ScoutServices.Enums.LockResult.Locked;

            lockState = LockStateEnum.Locked;
            map = _mapper.Map<ScoutServices.Enums.LockResult>(lockState);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) lockState, (uint) map);

            lockState = LockStateEnum.Unlocked;
            map = _mapper.Map<ScoutServices.Enums.LockResult>(lockState);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) lockState, (uint) map);
        }

        [Test]
        public void LockResultToLockStateEnumTests()
        {
            var lockState = ScoutServices.Enums.LockResult.Locked;
            var map = LockStateEnum.Locked;

            lockState = ScoutServices.Enums.LockResult.Locked;
            map = _mapper.Map<LockStateEnum>(lockState);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) lockState, (uint) map);

            lockState = ScoutServices.Enums.LockResult.Unlocked;
            map = _mapper.Map<LockStateEnum>(lockState);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) lockState, (uint) map);
        }

        [Test]
        public void DeclusterDegreeToeCellDeclusterSettingTests()
        {
            var declusterDegree = DeclusterDegreeEnum.High;
            var map = eCellDeclusterSetting.eDCHigh;

            declusterDegree = DeclusterDegreeEnum.High;
            map = _mapper.Map<eCellDeclusterSetting>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);

            declusterDegree = DeclusterDegreeEnum.Low;
            map = _mapper.Map<eCellDeclusterSetting>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);

            declusterDegree = DeclusterDegreeEnum.Medium;
            map = _mapper.Map<eCellDeclusterSetting>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);

            declusterDegree = DeclusterDegreeEnum.None;
            map = _mapper.Map<eCellDeclusterSetting>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);
        }

        [Test]
        public void eCellDeclusterSettingToDeclusterDegreeTests()
        {
            var declusterDegree = eCellDeclusterSetting.eDCHigh;
            var map = DeclusterDegreeEnum.High;

            declusterDegree = eCellDeclusterSetting.eDCHigh;
            map = _mapper.Map<DeclusterDegreeEnum>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);

            declusterDegree = eCellDeclusterSetting.eDCLow;
            map = _mapper.Map<DeclusterDegreeEnum>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);

            declusterDegree = eCellDeclusterSetting.eDCMedium;
            map = _mapper.Map<DeclusterDegreeEnum>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);

            declusterDegree = eCellDeclusterSetting.eDCNone;
            map = _mapper.Map<DeclusterDegreeEnum>(declusterDegree);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) declusterDegree, (uint) map);
        }

        [Test]
        public void SampleStatusToSampleStatusTests()
        {
            TestStatus(GrpcService.SampleStatusEnum.NotProcessed);
            TestStatus(GrpcService.SampleStatusEnum.InProcessAspirating);
            TestStatus(GrpcService.SampleStatusEnum.InProcessMixing);
            TestStatus(GrpcService.SampleStatusEnum.InProcessImageAcquisition);
            TestStatus(GrpcService.SampleStatusEnum.InProcessCleaning);
            TestStatus(GrpcService.SampleStatusEnum.AcquisitionComplete);
            TestStatus(GrpcService.SampleStatusEnum.Completed);
            TestStatus(GrpcService.SampleStatusEnum.SkipManual);
            TestStatus(GrpcService.SampleStatusEnum.SkipError);
        }

        private void TestStatus(SampleStatusEnum status)
        {
            var map = _mapper.Map<ScoutUtilities.Enums.SampleStatus>(status);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) status, (uint) map);
        }

        [Test]
        public void SampleStatusToSampleStatusTests2()
        {
            TestStatus(ScoutUtilities.Enums.SampleStatus.NotProcessed);
            TestStatus(ScoutUtilities.Enums.SampleStatus.InProcessAspirating);
            TestStatus(ScoutUtilities.Enums.SampleStatus.InProcessMixing);
            TestStatus(ScoutUtilities.Enums.SampleStatus.InProcessImageAcquisition);
            TestStatus(ScoutUtilities.Enums.SampleStatus.InProcessCleaning);
            TestStatus(ScoutUtilities.Enums.SampleStatus.AcquisitionComplete);
            TestStatus(ScoutUtilities.Enums.SampleStatus.Completed);
            TestStatus(ScoutUtilities.Enums.SampleStatus.SkipManual);
            TestStatus(ScoutUtilities.Enums.SampleStatus.SkipError);
        }

        private void TestStatus(ScoutUtilities.Enums.SampleStatus status)
        {
            var map = _mapper.Map<SampleStatus>(status);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint) status, (uint) map);
        }

        [Test]
        public void PrecessionToPrecessionTests()
        {
            TestPrecession(GrpcService.PrecessionEnum.ColumnMajor);
            TestPrecession(GrpcService.PrecessionEnum.RowMajor);
        }

        private void TestPrecession(GrpcService.PrecessionEnum value)
        {
            var map = _mapper.Map<ScoutUtilities.Enums.Precession>(value);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint)value, (uint)map);
        }

        [Test]
        public void PrecessionToPrecessionTests2()
        {
            TestPrecession(ScoutUtilities.Enums.Precession.ColumnMajor);
            TestPrecession(ScoutUtilities.Enums.Precession.RowMajor);
        }

        private void TestPrecession(ScoutUtilities.Enums.Precession value)
        {
            var map = _mapper.Map<GrpcService.PrecessionEnum>(value);
            Assert.IsNotNull(map);
            Assert.AreEqual((uint)value, (uint)map);
        }


        [TestCase(FilterOnEnum.FilterSample, ExpectedResult = eFilterItem.eSample)]
        [TestCase(FilterOnEnum.FilterSampleSet, ExpectedResult = eFilterItem.eSampleSet)]
        public eFilterItem TestFilterEnumMapping(FilterOnEnum source)
        {
            var map = _mapper.Map<eFilterItem>(source);
            Assert.IsNotNull(map);
            return map;
        }

        [TestCase(eFilterItem.eSample, ExpectedResult = FilterOnEnum.FilterSample)]
        [TestCase(eFilterItem.eSampleSet, ExpectedResult = FilterOnEnum.FilterSampleSet)]
        public FilterOnEnum TestFilterEnumMapping(eFilterItem source)
        {
            var map = _mapper.Map<FilterOnEnum>(source);
            Assert.IsNotNull(map);
            return map;
        }

        public void ByteToByteStringTests()
        {
            var map = _mapper.Map<ByteString>(0);
            Assert.IsNotNull(map);
            Assert.AreEqual(0, map.ToByteArray().FirstOrDefault());

            map = _mapper.Map<ByteString>(2);
            Assert.IsNotNull(map);
            Assert.AreEqual(2, map.ToByteArray().FirstOrDefault());

            map = _mapper.Map<ByteString>(8);
            Assert.IsNotNull(map);
            Assert.AreEqual(8, map.ToByteArray().FirstOrDefault());
        }

        public void ByteStringToByteTests()
        {
            var byteString = ByteString.CopyFrom(0);
            var map = _mapper.Map<byte>(byteString);
            Assert.IsNotNull(map);
            Assert.AreEqual(0, map);
            Assert.AreEqual(byteString.ToByteArray().FirstOrDefault(), map);

            byteString = ByteString.CopyFrom(19);
            map = _mapper.Map<byte>(byteString);
            Assert.IsNotNull(map);
            Assert.AreEqual(19, map);
            Assert.AreEqual(byteString.ToByteArray().FirstOrDefault(), map);

            byteString = ByteString.CopyFrom(255);
            map = _mapper.Map<byte>(byteString);
            Assert.IsNotNull(map);
            Assert.AreEqual(255, map);
            Assert.AreEqual(byteString.ToByteArray().FirstOrDefault(), map);
        }
    }
}