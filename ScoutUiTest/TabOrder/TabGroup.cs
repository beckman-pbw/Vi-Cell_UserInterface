using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace ScoutUiTest.TabOrder
{
    /// <summary>
    /// A TabGroup is also a TabPoint, but it contains a list of TabPoint(s). The TabPoint(s)
    /// can also be TabGroup(s), providing for the nesting of form components.
    /// </summary>
    public class TabGroup : TabPoint
    {
        protected readonly LinkedList<TabPoint> _order;

        /// <summary>
        /// Construct a TabGroup to represent the ordered list of controls that should be traversed
        /// when tabbing.
        /// </summary>
        /// <param name="name">A unique name within the application for this control.</param>
        /// <param name="matchElement">A lambda that will evaluate true only when the CruciatusElement
        /// passed as a parameter identifies this unique element.</param>
        public TabGroup(string name) : base(name)
        {
            _order = new LinkedList<TabPoint>();
        }

        /// <summary>
        /// Copy constructor. Creates a shallow copy of the contained TabPoint(s).
        /// </summary>
        /// <param name="tabGroup">The TabGroup to copy from.</param>
        public TabGroup(TabGroup tabGroup) : base(tabGroup.Name)
        {
            // There are more efficient ways of doing this, but this is test code.
            var temp = new TabPoint[tabGroup._order.Count];
            tabGroup._order.CopyTo(temp, 0);
            _order = new LinkedList<TabPoint>(temp);
        }

        public LinkedList<TabPoint> OrderedPoints => _order;

        public void AddTabPoint(TabPoint tabPoint)
        {
            _order.AddLast(tabPoint);
        }

        /// <summary>
        /// Insert a tabPoint before the TabPoint with the specified name.
        /// </summary>
        /// <param name="tabPoint">The new TabPoint to be inserted into this TabGroup.</param>
        /// <param name="name">If the name is null, the new tabPoint is inserted at the beginning of the TabGroup.</param>
        public void InsertBefore(TabPoint tabPoint, string name)
        {
            if (null == name)
            {
                _order.AddFirst(tabPoint);
                return;
            }
            var tempTabPoint = new TabPoint(name);
            var matchingPoint = _order.Find(tempTabPoint);
            if (null == matchingPoint)
            {
                throw new Exception("Test design error. Attempted to insert before non-existent TabPoint");
            }

            _order.AddBefore(matchingPoint, tabPoint);
        }

        /// <summary>
        /// The tab testing paradigm is to enable the creation of top level tab sequences in TabGroup(s)
        /// and then insert sub-TabGroup(s) for forms within the larger application. This method will
        /// create a shallow copy of a TabGroup that other TabGroup(s) could be inserted using the
        /// InsertBefore method.
        /// </summary>
        /// <returns></returns>
        public TabGroup Clone()
        {
            return new TabGroup(this);
        }

        /// <summary>
        /// Returns the next TabPoint in the ordered list of TabPoints.
        /// </summary>
        /// <param name="tabPoint">The starting TabPoint.</param>
        /// <returns>The next TabPoint, or null if at the end of the list.</returns>
        public TabPoint Next(TabPoint tabPoint)
        {
            var node = _order.Find(tabPoint);
            return node?.Next?.Value;
        }
    }
}