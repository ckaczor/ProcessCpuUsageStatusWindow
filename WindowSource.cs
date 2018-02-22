using Common.Wpf.Windows;
using FloatingStatusWindowLibrary;
using ProcessCpuUsageStatusWindow.Options;
using ProcessCpuUsageStatusWindow.Properties;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProcessCpuUsageStatusWindow
{
    public class WindowSource : IWindowSource, IDisposable
    {
        private readonly FloatingStatusWindow _floatingStatusWindow;
        private readonly ProcessCpuUsageWatcher _processCpuUsageWatcher;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        private CategoryWindow _optionsWindow;

        internal WindowSource()
        {
            _floatingStatusWindow = new FloatingStatusWindow(this);
            _floatingStatusWindow.SetText(Resources.Loading);

            _processCpuUsageWatcher = new ProcessCpuUsageWatcher();

            Task.Factory.StartNew(UpdateApp);
        }

        private async Task UpdateApp()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var updateManager = await UpdateManager.GitHubUpdateManager(App.UpdateUrl))
                {
                    var updates = await updateManager.CheckForUpdate();

                    var lastVersion = updates?.ReleasesToApply?.OrderBy(releaseEntry => releaseEntry.Version).LastOrDefault();

                    if (lastVersion == null)
                        return;

                    _dispatcher.Invoke(() => _floatingStatusWindow.SetText(Resources.Updating));
                    Thread.Sleep(500);

                    Common.Settings.Extensions.BackupSettings();

#if !DEBUG
                    await updateManager.DownloadReleases(new[] { lastVersion });
                    await updateManager.ApplyReleases(updates);
                    await updateManager.UpdateApp();
#endif
                }

#if !DEBUG
                UpdateManager.RestartApp();
#endif
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            finally
            {
                await Task.Factory.StartNew(() => _processCpuUsageWatcher.Initialize(Settings.Default.UpdateInterval, UpdateDisplay, _dispatcher));
            }
        }

        public void Dispose()
        {
            _processCpuUsageWatcher.Terminate();

            _floatingStatusWindow.Save();
            _floatingStatusWindow.Dispose();
        }

        public void ShowSettings()
        {
            var panels = new List<CategoryPanel>
            {
                new GeneralOptionsPanel(),
                new AboutOptionsPanel()
            };

            if (_optionsWindow == null)
            {
                _optionsWindow = new CategoryWindow(null, panels, Resources.ResourceManager, "OptionsWindow");
                _optionsWindow.Closed += (o, args) => { _optionsWindow = null; };
            }

            var dialogResult = _optionsWindow.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                Settings.Default.Save();

                Refresh();
            }
        }

        public void Refresh()
        {
            UpdateDisplay(_processCpuUsageWatcher.CurrentProcessList);
        }

        public string Name => Resources.ApplicationName;

        public System.Drawing.Icon Icon => Resources.ApplicationIcon;

        public bool HasSettingsMenu => true;
        public bool HasRefreshMenu => true;
        public bool HasAboutMenu => false;

        public void ShowAbout()
        {
        }

        public string WindowSettings
        {
            get => Settings.Default.WindowSettings;
            set
            {
                Settings.Default.WindowSettings = value;
                Settings.Default.Save();
            }
        }

        #region Display updating

        private static class PredefinedProcessName
        {
            public const string Total = "_Total";
            public const string Idle = "Idle";
        }

        private void UpdateDisplay(Dictionary<string, ProcessCpuUsage> currentProcessList)
        {       
            // Filter the process list to valid ones and exclude the idle and total values
            var validProcessList = (currentProcessList.Values.Where(
                process =>
                process.UsageValid && process.ProcessName != PredefinedProcessName.Total &&
                process.ProcessName != PredefinedProcessName.Idle)).ToList();

            // Calculate the total usage by adding up all the processes we know about
            var totalUsage = validProcessList.Sum(process => process.PercentUsage);

            // Sort the process list by usage and take only the top few
            var sortedProcessList = (validProcessList.OrderByDescending(process => process.PercentUsage)).Take(Settings.Default.ProcessCount);

            // Create a new string builder
            var stringBuilder = new StringBuilder();

            // Add the header line (if any)
            if (Resources.HeaderLine.Length > 0)
            {
                stringBuilder.AppendFormat(Resources.HeaderLine, totalUsage);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
            }

            // Loop over all processes in the sorted list
            foreach (ProcessCpuUsage processCpuUsage in sortedProcessList)
            {
                // Move to the next line if it isn't the first line
                if (stringBuilder.Length != 0)
                    stringBuilder.AppendLine();

                // Format the process information into a string to display
                stringBuilder.AppendFormat(Resources.ProcessLine, processCpuUsage.ProcessName, processCpuUsage.PercentUsage);
            }

            // Add the footer line (if any)
            if (Resources.FooterLine.Length > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat(Resources.FooterLine, totalUsage);
            }

            // Update the window with the text
            _floatingStatusWindow.SetText(stringBuilder.ToString());
        }

        #endregion
    }
}
