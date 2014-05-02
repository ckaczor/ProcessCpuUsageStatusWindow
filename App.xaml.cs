using System.Windows;

namespace ProcessCpuUsageStatusWindow
{
    public partial class App
    {
        private WindowSource _windowSource;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _windowSource = new WindowSource();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _windowSource.Dispose();

            base.OnExit(e);
        }
    }
}
