using System.ComponentModel;
using System.Windows.Automation;

namespace ScoutUiTest.Map
{
    #region using

    using Winium.Cruciatus.Core;
    using Winium.Cruciatus.Elements;
    using Winium.Cruciatus.Extensions;

    #endregion

    public class MainWindow : CruciatusElement
    {
        private WindowWaiter _mainMenuWaiter;
        #region Constructors and Destructors

        public MainWindow(CruciatusElement parent, By getStrategy)
            : base(parent, getStrategy)
        {
        }

        #endregion

        #region Public Properties

        //public OpenFileDialog OpenFileDialog => new OpenFileDialog(this, By.Name("Открытие").OrName("Open"));

        public MainMenu MainMenu => new MainMenu(this, By.Uid("ScoutXMainMenu"));

        public void OpenMenu()
        {
            _mainMenuWaiter = WindowWaiter.CreateComponentWaiter("Main menu",
                5000, (AutomationElement element) => element.Current.AutomationId.Equals("ScoutXHamburgerMenu"));
            MainMenu.Click();
            _mainMenuWaiter.Wait();
        }

        public void OpenSettings()
        {
            OpenMenu();
            
            this.FindElementByName("Settings").Click();
        }

        /*
        public Menu RibbonMenu => new Menu(this, By.Uid("RibbonMenu"));
        public FirstRibbonTab RibbonTabItem1 => new FirstRibbonTab(this, By.Uid("RibbonTabItem1"));
        public SecondRibbonTab RibbonTabItem2 => new SecondRibbonTab(this, By.Uid("RibbonTabItem2"));
        public SaveFileDialog SaveFileDialog => new SaveFileDialog(this, By.Name("Сохранение").OrName("Save As"));
        public Menu SetTextButtonContextMenu => new Menu(this, By.Uid("SetTextButtonContextMenu"));
        public Menu SimpleMenu => this.FindElementByUid("SimpleMenu").ToMenu();
        public FirstTab TabItem1 => new FirstTab(this, By.Uid("TabItem1"));
        public SecondTab TabItem2 => new SecondTab(this, By.Uid("TabItem2"));
        public ThirdTab TabItem3 => new ThirdTab(this, By.Uid("TabItem3"));
        */

        #endregion
    }
}