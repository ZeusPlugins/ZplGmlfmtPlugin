using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YoYoStudio.Core.Utils.Preferences;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplGmlfmtPlugin
        {
            public class ZplGmlfmtPluginPreferences : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                private string _GmlfmtPath;
                private bool _RunGmlfmtOnSave;

                [Prefs("machine.Plugins.ZplGmlfmtPlugin.GmlfmtPath", 0, "The path to the gml_fmt executable.", "ZplGmlfmt_Path", ePrefType.text_filename, new object[] { "tooltip:ZplGmlfmt_Path_Tooltip" })]
                public string GmlfmtPath { get { return _GmlfmtPath; } set { SetPropertyIfChanged(ref _GmlfmtPath, value); } }

                [Prefs("machine.Plugins.ZplGmlfmtPlugin.RunOnSave", 10, "Run gml_fmt on every save or not?", "ZplGmlfmt_OnSave", ePrefType.boolean, new object[] { "tooltip:ZplGmlfmt_OnSave_Tooltip" })]
                public bool RunGmlfmtOnSave { get { return _RunGmlfmtOnSave; } set { SetPropertyIfChanged(ref _RunGmlfmtOnSave, value); } }

                public ZplGmlfmtPluginPreferences()
                {
                    GmlfmtPath = "";
                    RunGmlfmtOnSave = false;
                }

                private void SetPropertyIfChanged<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
                {
                    var isEqual = property != null && ((IEquatable<T>)property).Equals(value);

                    if (!isEqual)
                    {
                        property = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }
                }
            }
        }
    }
}
