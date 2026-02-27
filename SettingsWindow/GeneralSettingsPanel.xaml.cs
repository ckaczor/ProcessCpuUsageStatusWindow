using System.Windows;
using ChrisKaczor.Wpf.Application;
using ProcessCpuUsageStatusWindow.Properties;

namespace ProcessCpuUsageStatusWindow.SettingsWindow;

public partial class GeneralSettingsPanel
{
    public GeneralSettingsPanel()
    {
        InitializeComponent();
    }

    public override string CategoryName => Properties.Resources.optionCategoryGeneral;

    public override void LoadPanel(Window parentWindow)
    {
        base.LoadPanel(parentWindow);

        MarkLoaded();
    }

    private void OnSaveSettings(object sender, RoutedEventArgs e)
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        if (!HasLoaded) return;

        Settings.Default.Save();

        Application.Current.SetStartWithWindows(Settings.Default.AutoStart);
    }
}