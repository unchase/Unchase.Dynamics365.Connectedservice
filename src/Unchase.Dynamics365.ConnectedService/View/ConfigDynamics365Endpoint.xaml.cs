using System;
using System.Windows;
using System.Windows.Controls;

namespace Unchase.Dynamics365.ConnectedService.View
{
    /// <summary>
    /// Interaction logic for ConfigDynamics365Endpoint.xaml
    /// </summary>
    public partial class ConfigDynamics365Endpoint : UserControl
    {
        private readonly Wizard _wizard;

        private const string ReportABugUrlFormat = "https://github.com/unchase/Unchase.Dynamics365.Connectedservice/issues/new?title={0}&labels=bug&body={1}";

        internal ConfigDynamics365Endpoint(Wizard wizard)
        {
            InitializeComponent();
            this._wizard = wizard;
            if (string.IsNullOrWhiteSpace(this.LanguageOption.Text))
                this.LanguageOption.SelectedIndex = 0;
        }

        private void ReportABugButton_Click(object sender, RoutedEventArgs e)
        {
            var title = Uri.EscapeUriString("<BUG title>");
            var body = Uri.EscapeUriString("<Please describe what bug you found when using the service.>");
            var url = string.Format(ReportABugUrlFormat, title, body);
            System.Diagnostics.Process.Start(url);
        }

        private void OpenEndpointFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "WSDL Files (.xml)|*.xml",
                Title = "Open service WSDL-file"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                Endpoint.Text = openFileDialog.FileName;
            }
        }
    }
}
