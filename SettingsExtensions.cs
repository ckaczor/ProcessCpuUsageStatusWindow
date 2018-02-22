using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ProcessCpuUsageStatusWindow
{
    public static class SettingsExtensions
    {
        public static void BackupSettings()
        {
            Debugger.Launch();

            var settingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            var destination = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\last.config";
            File.Copy(settingsFile, destination, true);
        }

        public static void RestoreSettings()
        {
            Debugger.Launch();

            var destFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            var sourceFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\last.config";

            if (!File.Exists(sourceFile))
                return;

            var destDirectory = Path.GetDirectoryName(destFile);

            if (destDirectory == null)
                return;

            try
            {
                Directory.CreateDirectory(destDirectory);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            try
            {
                File.Copy(sourceFile, destFile, true);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            try
            {
                File.Delete(sourceFile);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
