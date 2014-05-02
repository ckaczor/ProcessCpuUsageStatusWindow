using System;
using System.Diagnostics;

namespace ProcessCpuUsageStatusWindow
{
    public class ProcessCpuUsage
    {
        #region Properties

        public string ProcessName { get; private set; }
        public float PercentUsage { get; internal set; }
        public DateTime LastFound { get; set; }
        public bool UsageValid { get; private set; }

        internal CounterSample LastSample { get; set; }

        #endregion

        #region Constructor

        internal ProcessCpuUsage(InstanceData instanceData)
            : this(instanceData, DateTime.MinValue)
        { }

        internal ProcessCpuUsage(InstanceData instanceData, DateTime timestamp)
        {
            // Store the process details
            ProcessName = instanceData.InstanceName;

            // Store the initial data
            LastFound = timestamp;
            LastSample = instanceData.Sample;

            // We start out as not valid
            UsageValid = false;
        }

        #endregion

        #region Usage update

        internal void UpdateCpuUsage(InstanceData instanceData, DateTime timestamp)
        {
            // Get the new sample
            var newSample = instanceData.Sample;

            // Calculate percent usage
            PercentUsage = CounterSample.Calculate(LastSample, newSample) / Environment.ProcessorCount;

            // Update the last sample and timestmap
            LastSample = newSample;
            LastFound = timestamp;

            // Usage is now valid
            UsageValid = true;
        }

        #endregion
    }
}
