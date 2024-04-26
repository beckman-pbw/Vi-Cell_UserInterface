using ScoutUiTest.TabOrder;

namespace ScoutUiTest.TabModel
{
    public interface ITabGroupBuilder
    {
        TabGroup Build();
    }

    /// <summary>
    /// Constructs the TabGroup and its TabControls starting with the Hamburger menu. These controls
    /// exist on every screen, no matter what mode the ScoutX application is in, or what form such as
    /// Settings->Signatures are active.
    /// </summary>
    public class TopLevelBuilder : ITabGroupBuilder
    {
        public TabGroup Build()
        {
            // TabIndex=0
            var tabGroup = new TabGroup("TopLevel");
            tabGroup.AddTabPoint(new TabControl("HamburgerMenu",
                (element => element.Instance.Current.AutomationId.Equals("ScoutXMainMenu"))
                ));

            // TabIndex=1
            tabGroup.AddTabPoint(new TabControl("MessageHubButton",
                (element => element.Instance.Current.AutomationId.Equals("btnMessageHub"))
            ));

            // TabIndex=2
            tabGroup.AddTabPoint(new TabControl("InstrumentStatusButton",
                (element => element.Instance.Current.AutomationId.Equals("btnInstrumentStatus"))
            ));

            // TabIndex=3
            tabGroup.AddTabPoint(new TabControl("ReagentStatusButton",
                (element => element.Instance.Current.AutomationId.Equals("btnReagentStatus"))
            ));

            tabGroup.AddTabPoint(new TabControl("UserProfileButton",
                (element => element.Instance.Current.AutomationId.Equals("btnUserProfile"))
            ));

            tabGroup.AddTabPoint(new TabControl("MinimizeButton",
                (element => element.Instance.Current.AutomationId.Equals("btnMinimize"))
            ));

            return tabGroup;
        }
    }
}