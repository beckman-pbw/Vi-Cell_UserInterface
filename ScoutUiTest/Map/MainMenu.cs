namespace ScoutUiTest.Map
{
    #region using

    using Winium.Cruciatus.Core;
    using Winium.Cruciatus.Elements;
    using Winium.Cruciatus.Extensions;

    #endregion

    public class MainMenu : Winium.Cruciatus.Elements.Menu
    {
        #region Constructors and Destructors

        public MainMenu(CruciatusElement parent, By getStrategy)
            : base(parent, getStrategy)
        {
        }

        #endregion

        #region Public Properties

//        public CruciatusElement Option1 => this.FindElementByUid("RibbonButton");

        //        public ComboBox RibbonCheckComboBox => this.FindElementByUid("RibbonCheckComboBox").ToComboBox();

        #endregion
    }
}