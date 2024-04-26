using ScoutUiTest.TabOrder;

namespace ScoutUiTest.TabModel
{
    /// <summary>
    /// Constructs the TabGroup and its TabControls for the HomeView. These controls
    /// are on the default window and are used to set up the samples to be run, as well
	/// as the controls to start, stop and pause the samples run.
    /// </summary>
    public class HomeViewBuilder : ITabGroupBuilder
    {
        public TabGroup Build()
        {
            // TabIndex=0
            var tabGroup = new TabGroup("HomeView");
            tabGroup.AddTabPoint(new TabControl("AddSampleButton",
                (element => element.Instance.Current.AutomationId.Equals("btnAddSample"))
                ));

            // TabIndex=1
            tabGroup.AddTabPoint(new TabControl("SampleNameTextEdit",
                (element => element.Instance.Current.AutomationId.Equals("textSampleName"))
            ));

            // TabIndex=2
            tabGroup.AddTabPoint(new TabControl("SampleTemplateSettingsButton",
                (element => element.Instance.Current.AutomationId.Equals("btnSampleTemplateSettings"))
            ));

            // TabIndex=3
            tabGroup.AddTabPoint(new TabControl("CellTypeInTemplate",
                (element => element.Instance.Current.AutomationId.Equals("textTemplateCellType"))
            ));

            tabGroup.AddTabPoint(new TabControl("DilutionInSampleTemplate",
                (element => element.Instance.Current.AutomationId.Equals("editTemplateSampleDilution"))
            ));

            tabGroup.AddTabPoint(new TabControl("WashTypeInSampleTemplate",
                (element => element.Instance.Current.AutomationId.Equals("comboWashType"))
            ));

            tabGroup.AddTabPoint(new TabControl("TemplateSampleTag",
                (element => element.Instance.Current.AutomationId.Equals("editTemplateSampleTag"))
            ));

            tabGroup.AddTabPoint(new TabControl("SampleTemplateAdvancedSettingsButton",
                (element => element.Instance.Current.AutomationId.Equals("btnSampleTemplateAdvancedSettings"))
            ));

            tabGroup.AddTabPoint(new TabControl("SampleTemplateEswInfoButton",
                (element => element.Instance.Current.AutomationId.Equals("btnEswInfo"))
            ));

            tabGroup.AddTabPoint(new TabControl("SampleEjectButton",
                (element => element.Instance.Current.AutomationId.Equals("btnSampleRunEject"))
            ));

            tabGroup.AddTabPoint(new TabControl("SamplePlayButton",
                (element => element.Instance.Current.AutomationId.Equals("btnSampleRunPlay"))
            ));

            tabGroup.AddTabPoint(new TabControl("SamplesFilterButton",
                (element => element.Instance.Current.AutomationId.Equals("btnFilter"))
            ));

            return tabGroup;
        }
    }
}