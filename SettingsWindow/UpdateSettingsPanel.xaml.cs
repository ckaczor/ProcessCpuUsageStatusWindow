using System.Windows;
using System.Windows.Input;
using ProcessCpuUsageStatusWindow.Properties;

namespace ProcessCpuUsageStatusWindow.SettingsWindow;

public partial class UpdateSettingsPanel
{
    public UpdateSettingsPanel()
    {
        InitializeComponent();
    }

    public override string CategoryName => Properties.Resources.optionCategoryUpdate;

    private async void HandleCheckVersionNowButtonClick(object sender, RoutedEventArgs e)
    {
        var cursor = Cursor;

        Cursor = Cursors.Wait;

        await UpdateCheck.DisplayUpdateInformation(true);

        Cursor = cursor;
    }

    private void OnSaveSettings(object sender, RoutedEventArgs e)
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        if (!HasLoaded) return;

        Settings.Default.Save();
    }
}