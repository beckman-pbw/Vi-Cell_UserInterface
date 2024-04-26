using NUnit.Framework;
using ScoutUtilities.Enums;

namespace ScoutUtilitiesTest
{
    [TestFixture]
    public class SampleProcessingValidationResultTests
    {
        [Test]
        public void TestValid()
        {
            var e = SampleProcessingValidationResult.Valid;
            
            Assert.IsFalse(e.HasFlag(SampleProcessingValidationResult.Invalid96WellPlateSample));
            Assert.IsFalse(e.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));

            Assert.IsTrue(e.HasFlag(SampleProcessingValidationResult.Valid)); // always TRUE
            Assert.AreEqual(SampleProcessingValidationResult.Valid, e);
            Assert.IsTrue(e.IsValid());
            Assert.IsTrue(e.HasValidFlag());
        }

        [Test]
        public void CannotBeValidAndSomethingElse2()
        {
            var e = SampleProcessingValidationResult.Valid;
            e |= SampleProcessingValidationResult.CarrierNotDefined;

            Assert.AreNotEqual(SampleProcessingValidationResult.Valid, e);
            Assert.AreEqual(SampleProcessingValidationResult.CarrierNotDefined, e);
            Assert.IsTrue(e.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));
            Assert.IsFalse(e.HasFlag(SampleProcessingValidationResult.Invalid96WellPlateSample));

            Assert.AreNotEqual(SampleProcessingValidationResult.Valid, e);
            Assert.IsFalse(e.IsValid());
            Assert.IsFalse(e.HasValidFlag());
        }

        [Test]
        public void CanBe2Things()
        {
            var e = 
                SampleProcessingValidationResult.InvalidDilution | 
                SampleProcessingValidationResult.CarrierNotDefined;
            Assert.AreNotEqual(SampleProcessingValidationResult.InvalidDilution, e);
            Assert.AreNotEqual(SampleProcessingValidationResult.CarrierNotDefined, e);
            Assert.IsTrue(e.HasFlag(SampleProcessingValidationResult.InvalidDilution));
            Assert.IsTrue(e.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));
            Assert.IsFalse(e.HasFlag(SampleProcessingValidationResult.Invalid96WellPlateSample));
            
            Assert.AreNotEqual(SampleProcessingValidationResult.Valid, e);
            Assert.IsFalse(e.IsValid());
            Assert.IsFalse(e.HasValidFlag());
        }

        [Test]
        public void CanBe3Things()
        {
            var e = 
                SampleProcessingValidationResult.InvalidDilution | 
                SampleProcessingValidationResult.CarrierNotDefined |
                SampleProcessingValidationResult.InvalidSamplePosition;
            Assert.AreNotEqual(SampleProcessingValidationResult.InvalidDilution, e);
            Assert.AreNotEqual(SampleProcessingValidationResult.CarrierNotDefined, e);
            Assert.AreNotEqual(SampleProcessingValidationResult.InvalidSamplePosition, e);
            Assert.IsTrue(e.HasFlag(SampleProcessingValidationResult.InvalidDilution));
            Assert.IsTrue(e.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));
            Assert.IsTrue(e.HasFlag(SampleProcessingValidationResult.InvalidSamplePosition));
            Assert.IsFalse(e.HasFlag(SampleProcessingValidationResult.Invalid96WellPlateSample));
            
            Assert.AreNotEqual(SampleProcessingValidationResult.Valid, e);
            Assert.IsFalse(e.IsValid());
            Assert.IsFalse(e.HasValidFlag());
        }

        [Test]
        public void TestToString_Valid()
        {
            var e = SampleProcessingValidationResult.Valid;
            
            Assert.AreEqual($"{nameof(SampleProcessingValidationResult.Valid)}",
                e.ToString());
        }

        [Test]
        public void TestToString_1Enum()
        {
            var e = SampleProcessingValidationResult.InvalidSamplePosition;
            
            Assert.AreEqual($"{nameof(SampleProcessingValidationResult.InvalidSamplePosition)}",
                e.ToString());
        }

        [Test]
        public void TestToString_2Enum()
        {
            var e = SampleProcessingValidationResult.InvalidSamplePosition;
            e |= SampleProcessingValidationResult.InvalidDilution;

            var first = $"{nameof(SampleProcessingValidationResult.InvalidSamplePosition)}";
            var second = $"{nameof(SampleProcessingValidationResult.InvalidDilution)}";
            Assert.AreEqual($"{first}, {second}", e.ToString());
        }

        [Test]
        public void TestToString_3Enum()
        {
            var e = SampleProcessingValidationResult.InvalidSamplePosition;
            e |= SampleProcessingValidationResult.InvalidDilution;
            e |= SampleProcessingValidationResult.InvalidSaveNthImage;

            var first = $"{nameof(SampleProcessingValidationResult.InvalidSamplePosition)}";
            var second = $"{nameof(SampleProcessingValidationResult.InvalidDilution)}";
            var third = $"{nameof(SampleProcessingValidationResult.InvalidSaveNthImage)}";
            Assert.AreEqual($"{first}, {second}, {third}", e.ToString());
        }
    }
}