using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Threading;

namespace ProcessCpuUsageStatusWindow
{
    public class ProcessCpuUsageWatcher
    {
        #region Update delegate

        public delegate void ProcessListUpdatedDelegate(Dictionary<string, ProcessCpuUsage> currentProcessList);

        #endregion

        #region Member variables

        private Dispatcher _dispatcher;
        private Timer _processUpdateTimer;
        private ProcessListUpdatedDelegate _processListUpdatedCallback;
        private PerformanceCounterCategory _processCategory;

        #endregion

        #region Properties

        public Dictionary<string, ProcessCpuUsage> CurrentProcessList;

        #endregion

        #region Initialize and terminate

        public void Initialize(TimeSpan updateInterval, ProcessListUpdatedDelegate callback)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            // Create a new dictionary for the process list
            CurrentProcessList = new Dictionary<string, ProcessCpuUsage>();

            // Get the category for process performance info
            _processCategory = PerformanceCounterCategory.GetCategories().FirstOrDefault(category => category.CategoryName == "Process");

            if (_processCategory == null)
                return;

            // Read the entire category
            InstanceDataCollectionCollection processCategoryData = _processCategory.ReadCategory();

            // Get the processor time data
            InstanceDataCollection processorTimeData = processCategoryData["% processor time"];

            if (processorTimeData == null || processorTimeData.Values == null)
                return;

            // Loop over each instance and add it to the list
            foreach (InstanceData instanceData in processorTimeData.Values)
            {
                // Create a new process usage object
                var processCpuUsage = new ProcessCpuUsage(instanceData);

                // Add to the list
                CurrentProcessList.Add(processCpuUsage.ProcessName, processCpuUsage);
            }

            // Save the update callback
            _processListUpdatedCallback = callback;

            // Create a timer to update the process list
            _processUpdateTimer = new Timer(updateInterval.TotalMilliseconds) { AutoReset = false };
            _processUpdateTimer.Elapsed += HandleProcessUpdateTimerElapsed;
            _processUpdateTimer.Start();
        }

        public void Terminate()
        {
            // Get rid of the timer
            _processUpdateTimer.Stop();
            _processUpdateTimer.Dispose();

            // Clear the callback
            _processListUpdatedCallback = null;

            // Clear the process list
            CurrentProcessList = null;
        }

        #endregion

        #region Timer handling

        private void HandleProcessUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Update the current process list
            UpdateCurrentProcessList();

            // Restart the timer
            _processUpdateTimer.Start();
        }

        #endregion

        #region Process list management

        private void UpdateCurrentProcessList()
        {
            // Get a timestamp for the current time that we can use to see if a process was found this check
            DateTime checkStart = DateTime.Now;

            // Read the entire category
            InstanceDataCollectionCollection processCategoryData = _processCategory.ReadCategory();

            // Get the processor time data
            InstanceDataCollection processorTimeData = processCategoryData["% processor time"];

            if (processorTimeData == null || processorTimeData.Values == null)
                return;

            // Loop over each instance and add it to the list
            foreach (InstanceData instanceData in processorTimeData.Values)
            {
                // See if we already know about this process
                if (CurrentProcessList.ContainsKey(instanceData.InstanceName))
                {
                    // Get the previous process usage object
                    ProcessCpuUsage processCpuUsage = CurrentProcessList[instanceData.InstanceName];

                    // Update the CPU usage with new data
                    processCpuUsage.UpdateCpuUsage(instanceData, checkStart);
                }
                else
                {
                    // Create a new CPU usage object
                    var processCpuUsage = new ProcessCpuUsage(instanceData, checkStart);

                    // Add it to the list
                    CurrentProcessList.Add(processCpuUsage.ProcessName, processCpuUsage);
                }
            }

            // Build a list of cached processes we haven't found this check
            var oldProcessList = (from processCpuUsage in CurrentProcessList
                                  where processCpuUsage.Value.LastFound != checkStart
                                  select processCpuUsage.Key).ToList();

            // Loop over the list and remove the old process
            foreach (var key in oldProcessList)
                CurrentProcessList.Remove(key);

            // Invoke the callback with the new current process list
            _dispatcher.InvokeAsync(() => _processListUpdatedCallback.Invoke(CurrentProcessList));
        }

        #endregion
    }
}
