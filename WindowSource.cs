using Common.Wpf.Windows;
using FloatingStatusWindowLibrary;
using ProcessCpuUsageStatusWindow.Options;
using ProcessCpuUsageStatusWindow.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            Task.Factory.StartNew(UpdateApp).ContinueWith(task => StartUpdate(task.Result.Result));
        }

        private void StartUpdate(bool updateRequired)
        {
            if (updateRequired)
                return;

            Task.Factory.StartNew(() => _processCpuUsageWatcher.Initialize(Settings.Default.UpdateInterval, UpdateDisplay, _dispatcher));
        }

        private async Task<bool> UpdateApp()
        {
            return await UpdateCheck.CheckUpdate(HandleUpdateStatus);
        }

        private void HandleUpdateStatus(UpdateCheck.UpdateStatus status, string message)
        {
            if (status == UpdateCheck.UpdateStatus.None)
                message = Resources.Loading;

            _dispatcher.Invoke(() => _floatingStatusWindow.SetText(message));
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
            public const string Idle = "Idle:0";
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
            var sortedProcessList = validProcessList.OrderByDescending(process => process.PercentUsage).Take(Settings.Default.ProcessCount);

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
            foreach (var processCpuUsage in sortedProcessList)
            {
                // Move to the next line if it isn't the first line
                if (stringBuilder.Length != 0)
                    stringBuilder.AppendLine();

                var colonPosition = processCpuUsage.ProcessName.LastIndexOf(':');

                var processName = colonPosition == -1 ? processCpuUsage.ProcessName : processCpuUsage.ProcessName.Substring(0, colonPosition);
                var processId = colonPosition == -1 ? string.Empty : processCpuUsage.ProcessName.Substring(colonPosition + 1);

                // Format the process information into a string to display
                stringBuilder.AppendFormat(Resources.ProcessLine, processName, processCpuUsage.PercentUsage, processId);
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