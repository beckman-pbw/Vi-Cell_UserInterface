using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using NUnit.Framework;
using ScoutUtilities.Structs;

namespace ScoutOpcUaTests
{
    [TestFixture]
    public class RepeatedFieldStringToListUuidDllMaps : BaseAutoMapperUnitTest
    {
        [Test]
        public void Test1()
        {
            var repeated = new RepeatedField<string>
            {
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper()
            };

            var map = Mapper.Map<List<uuidDLL>>(repeated);

            Assert.IsNotNull(map);
            Assert.AreEqual(repeated.Count, map.Count);
            Assert.AreEqual(repeated[0], map[0].ToString());
            Assert.AreEqual(repeated[1], map[1].ToString());
            Assert.AreEqual(repeated[2], map[2].ToString());
            Assert.AreEqual(repeated[3], map[3].ToString());
            Assert.AreEqual(repeated[4], map[4].ToString());
        }

        [Test]
        public void TestFailure()
        {
            var repeated = new RepeatedField<string>
            {
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                "not a guid string"
            };

            try
            {
                var map = Mapper.Map<List<uuidDLL>>(repeated);
                Assert.Fail("This should have thrown an exception");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is FormatException);
            }
        }

        [Test]
        public void TestFailure2()
        {
            var repeated = new RepeatedField<string>
            {
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                Guid.NewGuid().ToString().ToUpper(),
                string.Empty,
                Guid.NewGuid().ToString().ToUpper(),
            };

            var map = Mapper.Map<List<uuidDLL>>(repeated);

            Assert.IsNotNull(map);
            Assert.AreEqual(repeated.Count, map.Count);
            Assert.AreEqual(repeated[0], map[0].ToString());
            Assert.AreEqual(repeated[1], map[1].ToString());
            Assert.AreEqual(repeated[2], map[2].ToString());
            Assert.AreEqual(Guid.Empty.ToString().ToUpper(), map[3].ToString());
            Assert.AreEqual(repeated[4], map[4].ToString());
        }
    }
}