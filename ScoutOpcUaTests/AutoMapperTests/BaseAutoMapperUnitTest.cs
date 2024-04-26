using AutoMapper;
using AutoMapper.Internal;
using GrpcServer;
using Ninject;
using NUnit.Framework;
using ScoutServices.Ninject;
using System;

namespace ScoutOpcUaTests
{
    public class BaseAutoMapperUnitTest
    {
        protected IKernel Kernel;
        protected IMapper Mapper;

        [SetUp]
        public void Setup()
        {
            Kernel = new StandardKernel(new ScoutServiceModule(), new OpcUaGrpcModule());
            Mapper = Kernel.Get<IMapper>();
            Assert.IsNotNull(Mapper);
        }

        public void PropertyReflectionTest(object baseObject, object mappedObject, object valueToTest,
            string baseObjectPropertyName, string mappedObjectPropertyName = null)
        {
            // set the value of baseObject.baseObjectPropertyName to valueToTest
            var baseObjectPropInfo = baseObject.GetType().GetProperty(baseObjectPropertyName);
            if (baseObjectPropInfo == null)
            {
                Assert.Fail($"baseObject ({baseObject.GetType().Name}) has no property '{baseObjectPropertyName}'");
            }

            if (baseObjectPropInfo.PropertyType.IsNullableType())
            {
                Assert.Fail($"{nameof(PropertyReflectionTest)} does not work with NULLABLE properties. Sorry ( ._.)");
            }
            else
            {
                baseObjectPropInfo.SetValue(baseObject, valueToTest);
            }

            // create the mapped object and check that it is not null
            Mapper.Map(baseObject, mappedObject);
            Assert.IsNotNull(mappedObject);

            // set the mappedObjectPropertyName to check against (it may be different than the baseObjectPropertyName)
            mappedObjectPropertyName = mappedObjectPropertyName ?? baseObjectPropertyName;
            var mappedObjectPropInfo = mappedObject.GetType().GetProperty(mappedObjectPropertyName);
            if (mappedObjectPropInfo == null)
            {
                Assert.Fail($"mappedObject ({mappedObject.GetType().Name}) has no property '{mappedObjectPropertyName}'");
            }

            if (mappedObjectPropInfo.PropertyType.IsNullableType())
            {
                Assert.Fail($"{nameof(PropertyReflectionTest)} does not work with NULLABLE properties. Sorry ( ._.)");
            }

            // get the values of the objects (which should be the same)
            var baseObjectValue = baseObjectPropInfo.GetValue(baseObject);
            var mappedObjectValue = mappedObjectPropInfo.GetValue(mappedObject);

            // check that the value in the parameters matches the baseObject's expected value
            Assert.AreEqual(valueToTest, baseObjectValue);

            // perform the assert for which we are testing
            if (baseObjectValue.GetType().IsEnum)
            {
                // Enum values can be compared using their base numeric value instead of enum title
                Assert.AreEqual(Convert.ToInt32(baseObjectValue), Convert.ToInt32(mappedObjectValue));
            }
            else
            {
                Assert.AreEqual(baseObjectValue, mappedObjectValue);
            }
        }

        private static bool IsNullable<T>(T obj)
        {
            if (obj == null) return true; // obvious
            Type type = typeof(T);
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }
    }
}