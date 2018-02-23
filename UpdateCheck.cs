using Squirrel;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace ProcessCpuUsageStatusWindow
{
    public static class UpdateCheck
    {
        public enum UpdateStatus
        {
            Checking,
            None,
            Downloading,
            Installing,
            Restarting
        }

        public delegate void UpdateStatusDelegate(UpdateStatus updateStatus, string message);

        public static Version LocalVersion => Assembly.GetEntryAssembly().GetName().Version;
        
        public static async Task<bool> CheckUpdate(UpdateStatusDelegate onUpdateStatus)
        {
            try
            {
                onUpdateStatus.Invoke(UpdateStatus.Checking, Properties.Resources.CheckingForUpdate);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var updateManager = await UpdateManager.GitHubUpdateManager(App.UpdateUrl))
                {
                    var updates = await updateManager.CheckForUpdate();

                    var lastVersion = updates?.ReleasesToApply?.OrderBy(releaseEntry => releaseEntry.Version).LastOrDefault();

                    if (lastVersion == null)
                    {
                        onUpdateStatus.Invoke(UpdateStatus.None, Properties.Resources.NoUpdate);
                        return false;
                    }

                    onUpdateStatus.Invoke(UpdateStatus.Downloading, Properties.Resources.DownloadingUpdate);

                    Common.Settings.Extensions.BackupSettings();

                    await updateManager.DownloadReleases(new[] { lastVersion });

                    onUpdateStatus.Invoke(UpdateStatus.Installing, Properties.Resources.InstallingUpdate);

                    await updateManager.ApplyReleases(updates);
                    await updateManager.UpdateApp();
                }

                onUpdateStatus.Invoke(UpdateStatus.Restarting, Properties.Resources.RestartingAfterUpdate);

                UpdateManager.RestartApp();

                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);

                return false;
            }
        }
    }
}
