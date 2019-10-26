using System;
using System.Collections.Generic;
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
                        result.Add($"{assemblyType.FullName}, {assemblyType.Assembly.GetName().FullName}");
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
                    var classes = GetClassesImplementsInterface<ICustomizeCodeDomService>(assembly);
                    CustomizeCodeDomServices.ItemsSource = classes;
                    CustomizeCodeDomServices.SelectedIndex = 0;
                    MessageBox.Show($"Loaded assembly with {classes.Count} classes implements {nameof(ICustomizeCodeDomService)}", "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var classes = GetClassesImplementsInterface<ICodeWriterFilterService>(assembly);
                    CodeWriterFilterServices.ItemsSource = classes;
                    CodeWriterFilterServices.SelectedIndex = 0;
                    MessageBox.Show($"Loaded assembly with {classes.Count} classes implements {nameof(ICodeWriterFilterService)}", "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var classes = GetClassesImplementsInterface<ICodeWriterMessageFilterService>(assembly);
                    CodeWriterMessageFilterServices.ItemsSource = classes;
                    CodeWriterMessageFilterServices.SelectedIndex = 0;
                    MessageBox.Show($"Loaded assembly with {classes.Count} classes implements {nameof(ICodeWriterMessageFilterService)}", "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var classes = GetClassesImplementsInterface<IMetadataProviderService>(assembly);
                    MetadataProviderServices.ItemsSource = classes;
                    MetadataProviderServices.SelectedIndex = 0;
                    MessageBox.Show($"Loaded assembly with {classes.Count} classes implements {nameof(IMetadataProviderService)}", "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var classes = GetClassesImplementsInterface<IMetadataProviderQueryService>(assembly);
                    MetadataProviderQueryServices.ItemsSource = classes;
                    MetadataProviderQueryServices.SelectedIndex = 0;
                    MessageBox.Show($"Loaded assembly with {classes.Count} classes implements {nameof(IMetadataProviderQueryService)}", "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var classes = GetClassesImplementsInterface<ICodeGenerationService>(assembly);
                    CodeGenerationServices.ItemsSource = classes;
                    CodeGenerationServices.SelectedIndex = 0;
                    MessageBox.Show($"Loaded assembly with {classes.Count} classes implements {nameof(ICodeGenerationService)}", "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var classes = GetClassesImplementsInterface<INamingService>(assembly);
                    NamingServices.ItemsSource = classes;
                    NamingServices.SelectedIndex = 0;
                    MessageBox.Show($"Loaded assembly with {classes.Count} classes implements {nameof(INamingService)}", "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var customizeCodeDomServicesClasses = GetClassesImplementsInterface<ICustomizeCodeDomService>(assembly);
                    CustomizeCodeDomServices.ItemsSource = customizeCodeDomServicesClasses;
                    CustomizeCodeDomServices.SelectedIndex = 0;
                    var codeWriterFilterServicesClasses = GetClassesImplementsInterface<ICodeWriterFilterService>(assembly);
                    CodeWriterFilterServices.ItemsSource = codeWriterFilterServicesClasses;
                    CodeWriterFilterServices.SelectedIndex = 0;
                    var codeWriterMessageFilterServicesClasses = GetClassesImplementsInterface<ICodeWriterMessageFilterService>(assembly);
                    CodeWriterMessageFilterServices.ItemsSource = codeWriterMessageFilterServicesClasses;
                    CodeWriterMessageFilterServices.SelectedIndex = 0;
                    var metadataProviderServicesClasses = GetClassesImplementsInterface<IMetadataProviderService>(assembly);
                    MetadataProviderServices.ItemsSource = metadataProviderServicesClasses;
                    MetadataProviderServices.SelectedIndex = 0;
                    var metadataProviderQueryServicesClasses = GetClassesImplementsInterface<IMetadataProviderQueryService>(assembly);
                    MetadataProviderQueryServices.ItemsSource = metadataProviderQueryServicesClasses;
                    MetadataProviderQueryServices.SelectedIndex = 0;
                    var codeGenerationServices = GetClassesImplementsInterface<ICodeGenerationService>(assembly);
                    CodeGenerationServices.ItemsSource = codeGenerationServices;
                    CodeGenerationServices.SelectedIndex = 0;
                    var mamingServicesClasses = GetClassesImplementsInterface<INamingService>(assembly);
                    NamingServices.ItemsSource = mamingServicesClasses;
                    NamingServices.SelectedIndex = 0;
                    MessageBox.Show(
                        $"Loaded assembly with :\n" +
                        $"\t{customizeCodeDomServicesClasses.Count} classes implements {nameof(ICustomizeCodeDomService)}\n" +
                        $"\t{codeWriterFilterServicesClasses.Count} classes implements {nameof(ICodeWriterFilterService)}\n" +
                        $"\t{codeWriterMessageFilterServicesClasses.Count} classes implements {nameof(ICodeWriterMessageFilterService)}\n" +
                        $"\t{metadataProviderServicesClasses.Count} classes implements {nameof(ICodeWriterMessageFilterService)}\n" +
                        $"\t{metadataProviderQueryServicesClasses.Count} classes implements {nameof(IMetadataProviderService)}\n" +
                        $"\t{codeGenerationServices.Count} classes implements {nameof(ICodeGenerationService)}\n" +
                        $"\t{mamingServicesClasses.Count} classes implements {nameof(INamingService)}"
                        , "Load assembly", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Load assembly data error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearCustomizeCodeDomServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomizeCodeDomServices.ItemsSource = new List<string>();
            CustomizeCodeDomServices.Text = string.Empty;
        }

        private void ClearCodeWriterFilterServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            CodeWriterFilterServices.ItemsSource = new List<string>();
            CodeWriterFilterServices.Text = string.Empty;
        }

        private void ClearCodeWriterMessageFilterServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            CodeWriterMessageFilterServices.ItemsSource = new List<string>();
            CodeWriterMessageFilterServices.Text = string.Empty;
        }

        private void ClearMetadataProviderServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            MetadataProviderServices.ItemsSource = new List<string>();
            MetadataProviderServices.Text = string.Empty;
        }

        private void ClearMetadataProviderQueryServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            MetadataProviderQueryServices.ItemsSource = new List<string>();
            MetadataProviderQueryServices.Text = string.Empty;
        }

        private void ClearCodeGenerationServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            CodeGenerationServices.ItemsSource = new List<string>();
            CodeGenerationServices.Text = string.Empty;
        }

        private void ClearNamingServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            NamingServices.ItemsSource = new List<string>();
            NamingServices.Text = string.Empty;
        }
    }
}
