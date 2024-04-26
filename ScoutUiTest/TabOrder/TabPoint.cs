using Winium.Cruciatus.Elements;

namespace ScoutUiTest.TabOrder
{
    /// <summary>
    /// A TabPoint represents a control that is a valid tab stop. It contains a delegate that takes a
    /// CruciatusElement as a parameter, and returns true if that element is the same. The delegate can
    /// utilize the Cruciatus methods to access the parent or children properties and determine if it
    /// is the correct element.
    /// </summary>
    public class TabPoint
    {
        public string Name { get; }

        /// <summary>
        /// Base class name to uniquely identify the nodes that make up an ordered tree of tabbed controls.
        /// </summary>
        /// <param name="name">A unique name within the application for this control.
        /// passed as a parameter identifies this unique element.</param>
        public TabPoint(string name)
        {
            Name = name;
        }

        protected bool Equals(TabPoint other)
        {
            return Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return Equals((TabPoint) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}