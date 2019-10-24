using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Unchase.Dynamics365.Customization;

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

        private static ObservableCollection<string> GetClassesImplementsInterface<T>(Assembly assembly)
        {
            var result = new ObservableCollection<string>();
            try
            {
                foreach (var assemblyType in assembly.GetExportedTypes())
                {
                    if (assemblyType.GetInterfaces().Any(i => i.FullName == typeof(T).FullName))
                    {
                        result.Add($"{assemblyType.FullName}, {assemblyType.Assembly.GetName().Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Load assembly error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private string OpenFile(string defaultExt, string filter, string title)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = defaultExt,
                Filter = filter,
                Title = title
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        private void OpenEndpointFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".xml", "WSDL Files (.xml)|*.xml", "Open service WSDL-file");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                Endpoint.Text = filePath;
            }
        }

        private void OpenFileWithCustomizeCodeDomServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with code customization service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    CustomizeCodeDomServices.ItemsSource = GetClassesImplementsInterface<ICustomizeCodeDomService>(assembly);
                    CustomizeCodeDomServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileWithCodeWriterFilterServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with code writer filter service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    CodeWriterFilterServices.ItemsSource = GetClassesImplementsInterface<ICodeWriterFilterService>(assembly);
                    CodeWriterFilterServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileWithCodeWriterMessageFilterServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with code writer message filter service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    CodeWriterMessageFilterServices.ItemsSource = GetClassesImplementsInterface<ICodeWriterMessageFilterService>(assembly);
                    CodeWriterMessageFilterServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileWithMetadataProviderServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with metadata provider service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    MetadataProviderServices.ItemsSource = GetClassesImplementsInterface<IMetadataProviderService>(assembly);
                    MetadataProviderServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileWithMetadataProviderQueryServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with metadata provider query service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    MetadataProviderQueryServices.ItemsSource = GetClassesImplementsInterface<IMetadataProviderQueryService>(assembly);
                    MetadataProviderQueryServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileWithCodeGenerationServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with code generation service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    CodeGenerationServices.ItemsSource = GetClassesImplementsInterface<ICodeGenerationService>(assembly);
                    CodeGenerationServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileWithNamingServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with naming service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    NamingServices.ItemsSource = GetClassesImplementsInterface<INamingService>(assembly);
                    NamingServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileWithAllServicesButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = OpenFile(".dll", "DLL Files (.dll)|*.dll", "Open DLL-file with naming service");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    CustomizeCodeDomServices.ItemsSource = GetClassesImplementsInterface<ICustomizeCodeDomService>(assembly);
                    CustomizeCodeDomServices.SelectedIndex = 0;
                    CodeWriterFilterServices.ItemsSource = GetClassesImplementsInterface<ICodeWriterFilterService>(assembly);
                    CodeWriterFilterServices.SelectedIndex = 0;
                    CodeWriterMessageFilterServices.ItemsSource = GetClassesImplementsInterface<ICodeWriterMessageFilterService>(assembly);
                    CodeWriterMessageFilterServices.SelectedIndex = 0;
                    MetadataProviderServices.ItemsSource = GetClassesImplementsInterface<IMetadataProviderService>(assembly);
                    MetadataProviderServices.SelectedIndex = 0;
                    MetadataProviderQueryServices.ItemsSource = GetClassesImplementsInterface<IMetadataProviderQueryService>(assembly);
                    MetadataProviderQueryServices.SelectedIndex = 0;
                    CodeGenerationServices.ItemsSource = GetClassesImplementsInterface<ICodeGenerationService>(assembly);
                    CodeGenerationServices.SelectedIndex = 0;
                    NamingServices.ItemsSource = GetClassesImplementsInterface<INamingService>(assembly);
                    NamingServices.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
