using System.ComponentModel.Design;
using NUnit.Framework;
using ScoutUtilities.Enums;

namespace ScoutUtilitiesTest
{
    [TestFixture]
    public class ValvePositionTest
    {
        [Test]
        public void TestValveNameToValvePosition()
        {
            Assert.AreEqual(ValvePosition.ValveA, ValvePositionMap.ValveNameToValvePosition("ValveA"));
            Assert.AreEqual(ValvePosition.ValveB, ValvePositionMap.ValveNameToValvePosition("ValveB"));
            Assert.AreEqual(ValvePosition.ValveC, ValvePositionMap.ValveNameToValvePosition("ValveC"));
            Assert.AreEqual(ValvePosition.ValveD, ValvePositionMap.ValveNameToValvePosition("ValveD"));
            Assert.AreEqual(ValvePosition.ValveE, ValvePositionMap.ValveNameToValvePosition("ValveE"));
            Assert.AreEqual(ValvePosition.ValveF, ValvePositionMap.ValveNameToValvePosition("ValveF"));
            Assert.AreEqual(ValvePosition.ValveG, ValvePositionMap.ValveNameToValvePosition("ValveG"));
            Assert.AreEqual(ValvePosition.ValveH, ValvePositionMap.ValveNameToValvePosition("ValveH"));

            // Check bad values evaluate as ValveA
            Assert.AreEqual(ValvePosition.ValveA, ValvePositionMap.ValveNameToValvePosition(null));
            Assert.AreEqual(ValvePosition.ValveA, ValvePositionMap.ValveNameToValvePosition(""));
            Assert.AreEqual(ValvePosition.ValveA, ValvePositionMap.ValveNameToValvePosition("   "));
            Assert.AreEqual(ValvePosition.ValveA, ValvePositionMap.ValveNameToValvePosition("ValveZ"));
            Assert.AreEqual(ValvePosition.ValveA, ValvePositionMap.ValveNameToValvePosition("Bogus String"));
            Assert.AreEqual(ValvePosition.ValveA, ValvePositionMap.ValveNameToValvePosition("Short"));
        }
    }
}