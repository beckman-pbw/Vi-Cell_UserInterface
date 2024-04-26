using Winium.Cruciatus.Elements;

namespace ScoutUiTest.TabOrder
{
    public delegate bool MatchElement(CruciatusElement element);

    public class TabControl : TabPoint
    {
        internal MatchElement MatchElement { get; }

        /// <summary>
        /// Construct a TabControl with a unique name and lambda that can uniquely identify it.
        /// </summary>
        /// <param name="name">A unique name within the application for this control.</param>
        /// <param name="matchElement">A lambda that will evaluate true only when the CruciatusElement
        /// passed as a parameter identifies this unique element.</param>
        public TabControl(string name, MatchElement matchElement) : base(name)
        {
            MatchElement = matchElement;
        }
    }
}