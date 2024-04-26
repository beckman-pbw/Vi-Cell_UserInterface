using System;
using System.Windows;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using NUnit.Framework;
using ScoutUtilities.Delegate;
using Winium.Cruciatus;

namespace ScoutUiTest.TabOrder
{
    public class TabVerifier
    {
        /// <summary>
        /// Traverse all the TabPoint(s) described in the top level TabGroup, and all its contained
        /// TabGroup(s) and their TabPoint(s).
        /// </summary>
        /// <param name="tabGroup"></param>
        /// <returns></returns>
        public static bool IsValid(TabGroup tabGroup)
        {
            var tabManager = new TabManager(tabGroup);
            var firstElement = tabManager.CurrentFocused;
            var firstControl = tabManager.Find(firstElement);

            // Assert that the first control that has the focus is somewhere in the tabGroup.
            Assert.IsNotNull(firstControl, $"The starting element with focus {firstElement.Instance.Current.AutomationId} is not within the defined TabGroup. This is likely an incorrectly constructed TabGroup for this test.");

            var count = 0;
            var currentControl = firstControl;
            do
            {
                CruciatusFactory.Keyboard.SendTab();
                //Thread.Sleep(10000);
                currentControl = tabManager.Next(currentControl);
                Assert.IsTrue(currentControl.MatchElement.Invoke(tabManager.CurrentFocused), $"Expected named tab stop {currentControl.Name}, found automation id {tabManager.CurrentFocused.Instance.Current.AutomationId}");
                count++;
            } while (count < tabManager.NumberOfControls + 1 && ! tabManager.CurrentFocused.Equals(firstElement));
            return count <= tabManager.NumberOfControls;
        }
    }
}