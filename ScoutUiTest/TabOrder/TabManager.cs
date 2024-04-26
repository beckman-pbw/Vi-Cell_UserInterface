using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using NUnit.Framework;
using Winium.Cruciatus;
using Winium.Cruciatus.Elements;

namespace ScoutUiTest.TabOrder
{
    internal class TabContext
    {
        public TabContext(TabPoint tabPoint, TabGroup containedBy)
        {
            TabPoint = tabPoint;
            ContainedBy = containedBy;
        }

        internal TabPoint TabPoint { get; }
        internal TabGroup ContainedBy { get; }
    }

    /// <summary>
    /// The TabManager, given a top level TabGroup, can determine the next or previous
    /// TabPoint from any other TabPoint, or the current focus, determined by
    /// CruciatusFactory.FocusedElement property.
    /// </summary>
    public class TabManager
    {
        private bool _initialized = false;
        private readonly TabGroup _topTabGroup;
        private readonly Dictionary<TabPoint, TabContext> _pointLookup = new Dictionary<TabPoint, TabContext>();
        private readonly List<TabContext> _controlLookup = new List<TabContext>();
        private int _numControls;

        public TabManager(TabGroup topTabGroup)
        {
            _topTabGroup = topTabGroup;
        }

        /// <summary>
        /// Construct PointLookup from topTabGroup by traversing tree of elements.
        /// </summary>
        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            AddTabPointsFromTabGroup(_topTabGroup);
            _numControls = _controlLookup.Count;
            _initialized = true;
        }

        public int NumberOfControls => _numControls;

        /// <summary>
        /// Recursively traverse TabGroup(s) accumulating TabPoint(s) in the PointLookup dictionary.
        /// </summary>
        /// <param name="parentTabGroup">Current TabGroup being traversed.</param>
        private void AddTabPointsFromTabGroup(TabGroup parentTabGroup)
        {
            foreach (var tabPoint in parentTabGroup.OrderedPoints)
            {
                if (tabPoint is TabGroup tabGroup)
                {
                    AddTabPointsFromTabGroup(tabGroup);
                }
                else
                {
                    if (_pointLookup.ContainsKey(tabPoint))
                    {
                        throw new Exception($"Attempting to add the same point, named {tabPoint.Name}, a second time for this top level TabGroup. This is likely incorrect construction of the TabGroup(s).");
                    }
                    var tabContext = new TabContext(tabPoint, parentTabGroup);
                    _pointLookup[tabPoint] = tabContext;
                    if (tabPoint is TabControl)
                    {
                        _controlLookup.Add(tabContext);
                    }
                }
            }
        }

        public CruciatusElement CurrentFocused
        {
            get
            {
                var control = CruciatusFactory.FocusedElement;

                // ToDo: Remove once test framework working
                // System.Diagnostics.Debug.WriteLine($"TabManager.CurrentFocused: Element Name: {control.Instance.Current.Name}, Automation Id: {control.Instance.Current.AutomationId}");

                // ToDo: Should we check for null if no control has focus, and perhaps set it to the Hamburger menu.
                return control;
            }
        }

        public TabControl Next(TabControl current = null)
        {
            Initialize();
            TabContext tabContext;
            if (null == current)
            {
                var currentFocused = CurrentFocused;
                tabContext = _controlLookup.FirstOrDefault(p => ((TabControl)p.TabPoint).MatchElement.Invoke(currentFocused));
                Assert.IsNotNull(tabContext);
                current = tabContext.TabPoint as TabControl;
            }
            else
            {
                if (!_pointLookup.TryGetValue(current, out tabContext))
                {
                    throw new Exception($"The {current.Name} control is not within the current TabGroup.");
                }
            }

            return NextControlInTabGroup(current, tabContext.ContainedBy);
        }

        private TabControl NextControlInTabGroup(TabPoint current, TabGroup containingGroup)
        {
            while (true)
            {
                var nextPoint = containingGroup.Next(current);
                if (null != nextPoint)
                {
                    return nextPoint is TabGroup tabGroup ? FindFirstControlInGroup(tabGroup) : (TabControl) nextPoint;
                }
                else
                {
                    if (containingGroup.Equals(_topTabGroup))
                    {
                        // Reached the end of the end of the _topTabGroup, restart at beginning.
                        return FindFirstControlInGroup(_topTabGroup);
                    }
                    else
                    {
                        // Reached the end of this TabGroup, continue traveling up to the next containing group.
                        current = containingGroup;
                        containingGroup = _pointLookup[containingGroup].ContainedBy;
                    }
                }
            }
        }

        private TabControl FindFirstControlInGroup(TabGroup tabGroup)
        {
            while (true)
            {
                // Reached the end of the current TabGroup, Look for the containing tab group in its parent.
                var firstPoint = tabGroup.OrderedPoints.First.Value;
                if (firstPoint is TabGroup nextGroup)
                {
                    tabGroup = nextGroup;
                }
                else
                {
                    return (TabControl) firstPoint;
                }
            }
        }

        /// <summary>
        /// Find the TabControl that matches the automation element using the MatchElement delegate.
        /// </summary>
        /// <param name="control">The Winium element</param>
        /// <returns>The matching TabControl for the automation. Returns null if not found.</returns>
        public TabControl Find(CruciatusElement control)
        {
            Initialize();
            return _controlLookup.FirstOrDefault(p => ((TabControl)p.TabPoint).MatchElement.Invoke(control))?.TabPoint as TabControl;
        }

        public bool Contains(CruciatusElement control)
        {
            return null != Find(control);
        }
    }
}