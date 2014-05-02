﻿using FloatingStatusWindowLibrary;
using ProcessCpuUsageStatusWindow.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessCpuUsageStatusWindow
{
    public class WindowSource : IWindowSource, IDisposable
    {
        private readonly FloatingStatusWindow _floatingStatusWindow;
        private readonly ProcessCpuUsageWatcher _processCpuUsageWatcher;

        internal WindowSource()
        {
            _floatingStatusWindow = new FloatingStatusWindow(this);
            _floatingStatusWindow.SetText(Resources.Loading);

            _processCpuUsageWatcher = new ProcessCpuUsageWatcher();
            _processCpuUsageWatcher.Initialize(Settings.Default.UpdateInterval, UpdateDisplay);
        }

        public void Dispose()
        {
            _processCpuUsageWatcher.Terminate();

            _floatingStatusWindow.Save();
            _floatingStatusWindow.Dispose();
        }

        public string Name
        {
            get { return "Process CPU Usage"; }
        }

        public System.Drawing.Icon Icon
        {
            get { return Resources.ApplicationIcon; }
        }

        public string WindowSettings
        {
            get
            {
                return Settings.Default.WindowSettings;
            }
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