using YoYoStudio.Core.Utils.Preferences;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplGmlfmtPlugin
        {
            public class ZplGmlfmtPluginInit : IPlugin
            {
                public PluginConfig Initialise()
                {
                    PluginConfig cfg = new PluginConfig("gml_fmt for Zeus", "gml_fmt 'integration' inside the IDE.", false);
                    cfg.AddCommand("zplgmlfmtplugin_command", "ide_loaded", "adding a command that will do the UI stuff.", "create", typeof(ZplGmlfmtPluginCommand));
                    cfg.AddCommand("zplgmlfmtplugin_run", "key(CTRL+SHIFT+Q)", "runs gml_fmt on keycombination", "function");
                    PreferencesManager.Register(typeof(ZplGmlfmtPluginPreferences));
                    return cfg;
                }
            }
        }
    }
}
