﻿using FloatingStatusWindowLibrary;
using ProcessCpuUsageStatusWindow.Properties;
using Squirrel;
using System;
using System.Diagnostics;
using System.Windows;

namespace ProcessCpuUsageStatusWindow
{
    public partial class App
    {
        private WindowSource _windowSource;

        public static string UpdateUrl = "https://github.com/ckaczor/ProcessCpuUsageStatusWindow";

        [STAThread]
        public static void Main(string[] args)
        {
            SquirrelAwareApp.HandleEvents(onAppUpdate: version => Common.Settings.Extensions.RestoreSettings());

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartManager.ManageAutoStart = true;
            StartManager.AutoStartEnabled = !Debugger.IsAttached && Settings.Default.AutoStart;
            StartManager.AutoStartChanged += value =>
            {
                Settings.Default.AutoStart = value;
                Settings.Default.Save();
            };

            _windowSource = new WindowSource();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _windowSource.Dispose();

            base.OnExit(e);
        }
    }
}
