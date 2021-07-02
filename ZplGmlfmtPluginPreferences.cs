using YoYoStudio.Core.Utils.Preferences;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplGmlfmtPlugin
        {
            public class ZplGmlfmtPluginPreferences
            {
                [Prefs("machine.Plugins.ZplGmlfmtPlugin.GmlfmtPath", 0, "The path to the gml_fmt executable.", "ZplGmlfmt_Path", ePrefType.text_filename, new object[] { })]
                public string GmlfmtPath { get; set; }

                [Prefs("machine.Plugins.ZplGmlfmtPlugin.RunOnSave", 10, "Run gml_fmt on every save or not?", "ZplGmlfmt_OnSave", ePrefType.boolean, new object[] { })]
                public bool RunGmlfmtOnSave { get; set; }

                public ZplGmlfmtPluginPreferences()
                {
                    GmlfmtPath = "";
                    RunGmlfmtOnSave = false;
                }
            }
        }
    }
}
