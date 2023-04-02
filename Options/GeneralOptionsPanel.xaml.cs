using Common.Wpf.Extensions;
using System.Windows;

namespace ProcessCpuUsageStatusWindow.Options
{
    public partial class GeneralOptionsPanel
    {
        public GeneralOptionsPanel()
        {
            InitializeComponent();
        }

        public override void LoadPanel(object data)
        {
            base.LoadPanel(data);

            var settings = Properties.Settings.Default;

            StartWithWindows.IsChecked = settings.AutoStart;
            NumberOfProcesses.Text = settings.ProcessCount.ToString();
            ShowProcessId.IsChecked = settings.ShowProcessId;
        }

        public override bool ValidatePanel()
        {
            return true;
        }

        public override void SavePanel()
        {
            var settings = Properties.Settings.Default;

            if (StartWithWindows.IsChecked.HasValue && settings.AutoStart != StartWithWindows.IsChecked.Value)
                settings.AutoStart = StartWithWindows.IsChecked.Value;

            settings.ProcessCount = int.Parse(NumberOfProcesses.Text);

            if (ShowProcessId.IsChecked.HasValue && settings.ShowProcessId != ShowProcessId.IsChecked.Value)
                settings.ShowProcessId = ShowProcessId.IsChecked.Value;

            Application.Current.SetStartWithWindows(settings.AutoStart);
        }

        public override string CategoryName => Properties.Resources.OptionCategory_General;
    }
}
