using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using ChrisKaczor.Wpf.Windows.FloatingStatusWindow;
using ProcessCpuUsageStatusWindow.Properties;

namespace ProcessCpuUsageStatusWindow;

public partial class App
{
    private List<IDisposable> _windowSourceList;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        StartManager.ManageAutoStart = true;
        StartManager.AutoStartEnabled = !Debugger.IsAttached && Settings.Default.AutoStart;
        StartManager.AutoStartChanged += (value =>
        {
            Settings.Default.AutoStart = value;
            Settings.Default.Save();
        });

        _windowSourceList =
        [
            new WindowSource()
        ];
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _windowSourceList.ForEach(ws => ws.Dispose());

        base.OnExit(e);
    }
}