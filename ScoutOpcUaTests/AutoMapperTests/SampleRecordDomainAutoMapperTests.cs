using System;
using AutoMapper;
using GrpcServer;
using GrpcService;
using Ninject;
using NUnit.Framework;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutServices.Ninject;
using ScoutUtilities.Structs;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class SampleRecordDomainAutoMapperTests
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
        public void SampleRecordDomainAutoMapperTest_IsSingleExportStatus()
        {
            var sampleObject = new SampleRecordDomain();
            var map = new ScoutDomains.SampleRecordDomain();

            sampleObject.IsSingleExportStatus = true;
            map = _mapper.Map<ScoutDomains.SampleRecordDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.IsSingleExportStatus, map.IsSingleExportStatus);
        }

        [Test]
        public void SampleRecordDomainAutoMapperTest_NumOfResultRecord()
        {
            var sampleObject = new SampleRecordDomain();
            var map = new ScoutDomains.SampleRecordDomain();

            sampleObject.NumOfResultRecord = 123;
            map = _mapper.Map<ScoutDomains.SampleRecordDomain>(sampleObject);

            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.NumOfResultRecord, map.NumOfResultRecord);
        }

        [Test]
        public void SampleRecordDomainAutoMapperTest_Uuid()
        {
            var sampleObject = new SampleRecordDomain();
            var map = new ScoutDomains.SampleRecordDomain();

            sampleObject.UuidDll = Guid.NewGuid().ToString();
            map = _mapper.Map<ScoutDomains.SampleRecordDomain>(sampleObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.UuidDll.ToUpper(), map.UUID.ToString());

            sampleObject.UuidDll = Guid.Empty.ToString();
            map = _mapper.Map<ScoutDomains.SampleRecordDomain>(sampleObject);
            Assert.IsNotNull(map);
            Assert.AreEqual(sampleObject.UuidDll.ToUpper(), map.UUID.ToString());
        }
    }
}