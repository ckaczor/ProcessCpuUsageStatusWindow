using System;
using Microsoft.Extensions.Logging;
using Serilog;
using Velopack;

namespace ProcessCpuUsageStatusWindow;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();

        Log.Logger.Information("Start");

        var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

        VelopackApp.Build().Run(loggerFactory.CreateLogger("Install"));

        var app = new App();
        app.InitializeComponent();
        app.Run();

        Log.Logger.Information("End");
    }
}