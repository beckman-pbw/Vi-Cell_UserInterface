using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using NUnit.Framework;
using ScoutUtilities.Structs;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class ListUuidDllToRepeatedFieldStringMapTests : BaseAutoMapperUnitTest
    {
        [Test]
        public void Test1()
        {
            var list = new List<uuidDLL>
            {
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.NewGuid())
            };

            var map = Mapper.Map<RepeatedField<string>>(list);

            Assert.IsNotNull(map);
            Assert.AreEqual(list.Count, map.Count);
            Assert.AreEqual(list[0].ToString(), map[0]);
            Assert.AreEqual(list[1].ToString(), map[1]);
            Assert.AreEqual(list[2].ToString(), map[2]);
            Assert.AreEqual(list[3].ToString(), map[3]);
            Assert.AreEqual(list[4].ToString(), map[4]);
        }

        [Test]
        public void Test2()
        {
            var list = new List<uuidDLL>
            {
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.NewGuid()),
                new uuidDLL(Guid.Empty)
            };

            var map = Mapper.Map<RepeatedField<string>>(list);

            Assert.IsNotNull(map);
            Assert.AreEqual(list.Count, map.Count);
            Assert.AreEqual(list[0].ToString(), map[0]);
            Assert.AreEqual(list[1].ToString(), map[1]);
            Assert.AreEqual(list[2].ToString(), map[2]);
            Assert.AreEqual(list[3].ToString(), map[3]);
            Assert.AreEqual(list[4].ToString(), map[4]);
        }
    }
}