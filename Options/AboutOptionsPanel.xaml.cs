using System.Reflection;
using System.Windows;

namespace ProcessCpuUsageStatusWindow.Options
{
    public partial class AboutOptionsPanel
    {
        public AboutOptionsPanel()
        {
            InitializeComponent();
        }

        public override void LoadPanel(object data)
        {
            base.LoadPanel(data);

            ApplicationNameLabel.Text = Properties.Resources.ApplicationName;

            var version = UpdateCheck.LocalVersion.ToString();
            VersionLabel.Text = string.Format(Properties.Resources.About_Version, version);

            CompanyLabel.Text = ((AssemblyCompanyAttribute)Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0]).Company;
        }

        public override bool ValidatePanel()
        {
            return true;
        }

        public override void SavePanel()
        {
        }

        public override string CategoryName => Properties.Resources.OptionCategory_About;

        private async void HandleCheckForUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            await UpdateCheck.CheckUpdate((status, message) => UpdateMessage.Content = message);
        }
    }
}
