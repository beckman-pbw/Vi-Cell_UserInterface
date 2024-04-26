using AutoMapper;
using GrpcServer;
using GrpcService;
using Ninject;
using NUnit.Framework;
using ScoutServices.Enums;
using ScoutServices.Ninject;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class LockStateAutoMapperTests
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
        public void LockStateAutoMapperTest()
        {
            var lockState = LockStateEnum.Locked;
            var map = _mapper.Map<LockResult>(lockState);

            Assert.IsNotNull(map);
            Assert.AreEqual((ushort)lockState, (ushort)map);

            lockState = LockStateEnum.Unlocked;
            map = _mapper.Map<LockResult>(lockState);

            Assert.IsNotNull(map);
            Assert.AreEqual((ushort)lockState, (ushort)map);
        }
    }
}