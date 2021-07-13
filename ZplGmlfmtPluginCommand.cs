using System;
using System.Collections.Generic;
using System.Linq;
using YoYoStudio.Core.Utils;
using YoYoStudio.Core.Utils.Preferences;
using YoYoStudio.FileAPI;
using Core.CoreOS.FileAPI;
using YoYoStudio.GUI.Gadgets;
using YoYoStudio.GUI.Layout;
using YoYoStudio.Plugins.Attributes;
using YoYoStudio.GUI;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplGmlfmtPlugin
        {
            [ModuleName("ZplGmlfmtPlugin Command", "Responsible for the plugin's UI and process stuff.")]
            public class ZplGmlfmtPluginCommand : IModule, IDisposable
            {
                public ModulePackage IdeInterface { get; set; }
                public ModulePackage Package => IdeInterface;
                public ZplGmlfmtPluginPreferences Preferences { get; set; }
                public bool LayoutFailed { get; set; }
                public bool Running { get; set; }
                public string MenuId { get; set; }
                public MenuEntry RunButton { get; set; }

                public void SetButtonActive()
                {
                    Running = false;

                    if (!LayoutFailed && RunButton != null)
                    {
                        RunButton.Deactivated = Running;
                    }
                }

                public void SetButtonDeactive()
                {
                    Running = true;

                    if (!LayoutFailed)
                    {
                        RunButton.Deactivated = Running;
                    }
                }

                public void ReloadProject()
                {
                    IDE.OpenProject(MacroExpansion.Expand("${project_full_filename}"), false);
                }

                public void CloseReloadProject(TimeSpan _delta)
                {
                    WindowManager.OnPreProcess.RemoveThis();
                    IDE.CloseProject(ReloadProject, false);
                }

                public void OnCmdCompletion(int _exitcode)
                {
                    SetButtonActive();

                    // fuck filewatcher
                    // WindowManager.OnPreProcess += CloseReloadProject;
                }

                public bool YYFileExists(string _path)
                {
                    return FileSystem.FileExists(_path, null, null).wait() == FileError.OK;
                }

                public string GetProjectDirectorySafe()
                {
                    return FileSystem.PlatformSafePath(MacroExpansion.Expand("${project_dir}"));
                }

                public void RunGmlfmt()
                {
                    if (!LayoutFailed && Preferences != null && !Running)
                    {
                        string _process = Preferences.GmlfmtPath;
                        if (string.IsNullOrWhiteSpace(_process) || !YYFileExists(_process))
                        {
                            MessageDialog.ShowWarning("ZGFP_Title", "ZGFP_Path");
                            return;
                        }
                        
                        string _wdir = GetProjectDirectorySafe();
                        if (string.IsNullOrWhiteSpace(_wdir))
                        {
                            MessageDialog.ShowWarning("ZGFP_Title", "ZGFP_Project");
                            return;
                        }

                        var gmlfmtproc = new CmdProcess(_process, "", eOutputStream.Output | eOutputStream.AssetCompiler, true);
                        gmlfmtproc.OnCompletion += OnCmdCompletion;
                        // deactivate the UI button first, and then run the tool:
                        SetButtonDeactive();
                        gmlfmtproc.RunAsync(true, _wdir);
                    }
                }

                public void OnButtonClick()
                {
                    RunGmlfmt();
                }

                public MenuEntry AttachToButton(MenuBar _mb, string _id, ButtonClick _delegate, bool deactiv)
                {
                    MenuEntry me = _mb.RetrieveMenuEntry(_id);
                    if (me != null)
                    {
                        me.OnButtonClick += _delegate;
                        me.Deactivated = deactiv;
                    }
                    else
                    {
                        Log.WriteLine(eLog.Default, "[ZplGmlfmt]: Failed to attach to a UI button {0} :(", _id);
                    }

                    return me;
                }

                public void PerformUIAttachement(MenuBar _mb)
                {
                    if (!LayoutFailed && _mb != null)
                    {
                        RunButton = AttachToButton(_mb, "menu_run_gmlfmt", OnButtonClick, Running || !IDE.IsProjectLoadingComplete);
                    }
                }

                public void AttachToUI()
                {
                    if (!LayoutFailed)
                    {
                        if (MenuId != "")
                        {
                            IdeInterface.WindowManager.RemoveStaticMenu(MenuId);
                            //Log.WriteLine(eLog.Default, "[ZplGmlfmt]: Detached from menu.");
                        }

                        MenuBar mb = (MenuBar)IdeInterface.WindowManager.CreateLayout("ZplGmlfmtPluginLayout")[0];
                        MenuBarEntry mbe = (MenuBarEntry)mb.StackedGadgets[0];
                        PerformUIAttachement(mb);
                        MenuId = mbe.MenuBarEntryID.Name;
                        IdeInterface.WindowManager.RegisterStaticMenu(mbe);
                    }
                }

                public string GetPluginDirectory()
                {
                    string path = YoYoPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom Plugins");
                    Log.WriteLine(eLog.Default, "[ZplGmlfmt]: GetPluginDirectory() = '{0}'", path);
                    return path;
                }

                public void LoadAssets()
                {
                    try
                    {
                        LayoutFailed = false;

                        string myDir = GetPluginDirectory();
                        string langret = Language.Load(YoYoPath.Combine(myDir, "ZplGmlfmtPluginStrings.csv"));
                        LayoutManager.LoadLayout(YoYoPath.Combine(myDir, "ZplGmlfmtPluginLayout.xml"));
                        Log.WriteLine(eLog.Default, "[ZplGmlfmt]: Assets loaded. {0}", langret);
                    }
                    catch (Exception exc)
                    {
                        LayoutFailed = true;
                        MessageDialog.ShowUnlocalisedWarning("gml_fmt for Zeus", "Failed to load the required assets:\n" + exc.ToString());
                    }
                }

                public void OnChange(List<string> _changes)
                {
                    if (_changes.Any(_pref => _pref.Contains("ZplGmlfmtPlugin")))
                    {
                        Preferences = PreferencesManager.Get<ZplGmlfmtPluginPreferences>();
                        Log.WriteLine(eLog.Default, "[ZplGmlfmt]: OnChange() Preferences updated.");
                    }
                }

                public void OnProjectSaved(bool _success)
                {
                    if (_success)
                    {
                        if (Preferences != null && Preferences.RunGmlfmtOnSave)
                        {
                            RunGmlfmt();
                        }
                    }
                }

                [Function("OnGmlfmtKeycombo", "zplgmlfmtplugin_run", "Runs gml_fmt if keycombo is pressed.", "function")]
                public void OnGmlfmtKeycombo()
                {
                    // only run if the project is loaded.
                    if (IDE.IsProjectLoadingComplete) RunGmlfmt();
                }

                public void OnProjectLoaded()
                {
                    SetButtonActive();
                }

                public void OnInitialised()
                {
                    // initialize properties to default values (not running, no menu attached)
                    Running = false;
                    MenuId = "";

                    // load prefs
                    PreferencesManager.OnChange += OnChange;
                    Preferences = PreferencesManager.Get<ZplGmlfmtPluginPreferences>();

                    // load ui
                    LoadAssets();
                    AttachToUI();
                    IDE.OnProjectLoaded += OnProjectLoaded; // will activate the button on project load.

                    // 'Run On Every Save' handler.
                    IDE.OnProjectSaved += OnProjectSaved;

                    // we're done here
                    Log.WriteLine(eLog.Default, "[ZplGmlfmt]: Initialised.");
                }

                public void Initialise(ModulePackage _ide)
                {
                    IdeInterface = _ide;
                    OnInitialised();
                }

                #region IDisposable Support
                private bool disposed = false; // To detect redundant calls

                protected virtual void Dispose(bool disposing)
                {
                    if (!disposed)
                    {
                        if (disposing)
                        {
                            // TODO: dispose managed state (managed objects).
                        }

                        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                        // TODO: set large fields to null.

                        disposed = true;
                    }
                }

                // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
                ~ZplGmlfmtPluginCommand()
                {
                    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                    Dispose(false);
                }

                // This code added to correctly implement the disposable pattern.
                public void Dispose()
                {
                    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                    Dispose(true);
                    // TODO: uncomment the following line if the finalizer is overridden above.
                    GC.SuppressFinalize(this);
                }
                #endregion
            }
        }
    }
}
