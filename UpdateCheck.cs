using System.Threading.Tasks;
using System.Windows;
using NuGet.Versioning;
using Serilog;
using Velopack;
using Velopack.Sources;

namespace ProcessCpuUsageStatusWindow;

internal static class UpdateCheck
{
    private static UpdateManager _updateManager;

    public static UpdateManager UpdateManager => _updateManager ??= new UpdateManager(new GithubSource("https://github.com/ckaczor/ProcessCpuUsageStatusWindow", null, false));

    public static string LocalVersion => (UpdateManager.CurrentVersion ?? new SemanticVersion(0, 0, 0)).ToString();

    public static bool IsInstalled => UpdateManager.IsInstalled;

    public static async Task DisplayUpdateInformation(bool showIfCurrent)
    {
        var newVersion = IsInstalled ? await UpdateManager.CheckForUpdatesAsync() : null;

        if (newVersion != null)
        {
            var updateCheckTitle = string.Format(Properties.Resources.UpdateCheckTitle, Properties.Resources.ApplicationName);

            var updateCheckMessage = string.Format(Properties.Resources.UpdateCheckNewVersion, newVersion.TargetFullRelease.Version);

            if (MessageBox.Show(updateCheckMessage, updateCheckTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Log.Logger.Information("Downloading update");

            await UpdateManager.DownloadUpdatesAsync(newVersion);

            Log.Logger.Information("Installing update");

            UpdateManager.ApplyUpdatesAndRestart(newVersion);
        }
        else if (showIfCurrent)
        {
            var updateCheckTitle = string.Format(Properties.Resources.UpdateCheckTitle, Properties.Resources.ApplicationName);

            var updateCheckMessage = string.Format(Properties.Resources.UpdateCheckCurrent, Properties.Resources.ApplicationName);

            MessageBox.Show(updateCheckMessage, updateCheckTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}