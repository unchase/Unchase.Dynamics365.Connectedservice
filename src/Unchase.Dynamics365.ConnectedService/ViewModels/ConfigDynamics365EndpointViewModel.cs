using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using Unchase.Dynamics365.ConnectedService.Models;
using Unchase.Dynamics365.ConnectedService.View;

namespace Unchase.Dynamics365.ConnectedService.ViewModels
{
    internal class ConfigDynamics365EndpointViewModel : ConnectedServiceWizardPage
    {
        #region Properties
        private string _endPoint;
        public string Endpoint
        {
            get => _endPoint;
            set
            {
                _endPoint = value;
                UserSettings.Endpoint = value;
                OnPropertyChanged(nameof(Endpoint));
            }
        }

        private string _serviceName;
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                _serviceName = value;
                UserSettings.ServiceName = value;
                OnPropertyChanged(nameof(ServiceName));
            }
        }

        private string _generatedFileName;
        public string GeneratedFileName
        {
            get => _generatedFileName;
            set
            {
                _generatedFileName = value;
                UserSettings.GeneratedFileName = value;
                OnPropertyChanged(nameof(GeneratedFileName));
            }
        }

        private bool _enableDebugMode;
        public bool EnableDebugMode
        {
            get => _enableDebugMode;
            set
            {
                _enableDebugMode = value;
                UserSettings.EnableDebugMode = value;
                OnPropertyChanged(nameof(EnableDebugMode));
            }
        }

        private bool _addClientNuGet;
        public bool AddClientNuGet
        {
            get => _addClientNuGet;
            set
            {
                _addClientNuGet = value;
                UserSettings.AddClientNuGet = value;
                OnPropertyChanged(nameof(AddClientNuGet));
            }
        }

        private string _languageOption;
        public string LanguageOption
        {
            get => _languageOption;
            set
            {
                _languageOption = value;
                UserSettings.LanguageOption = value;
                OnPropertyChanged(nameof(LanguageOption));
            }
        }

        private bool _useInteractiveLogin;
        public bool UseInteractiveLogin
        {
            get => _useInteractiveLogin;
            set
            {
                _useInteractiveLogin = value;
                UserSettings.UseInteractiveLogin = value;
                OnPropertyChanged(nameof(UseInteractiveLogin));
            }
        }

        private string _namespace;
        public string Namespace
        {
            get => _namespace;
            set
            {
                _namespace = value;
                UserSettings.Namespace = value;
                OnPropertyChanged(nameof(Namespace));
            }
        }

        private string _connectionString;
        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = value;
                UserSettings.ConnectionString = value;
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        private bool _useOAuth;
        public bool UseOAuth
        {
            get => _useOAuth;
            set
            {
                _useOAuth = value;
                UserSettings.UseOAuth = value;
                OnPropertyChanged(nameof(UseOAuth));
            }
        }

        private bool _useConnectionString;
        public bool UseConnectionString
        {
            get => _useConnectionString;
            set
            {
                _useConnectionString = value;
                UserSettings.UseConnectionString = value;
                OnPropertyChanged(nameof(UseConnectionString));
            }
        }

        private string _serviceContextName;
        public string ServiceContextName
        {
            get => _serviceContextName;
            set
            {
                _serviceContextName = value;
                UserSettings.ServiceContextName = value;
                OnPropertyChanged(nameof(ServiceContextName));
            }
        }

        private bool _generateMessages;
        public bool GenerateMessages
        {
            get => _generateMessages;
            set
            {
                _generateMessages = value;
                UserSettings.GenerateMessages = value;
                OnPropertyChanged(nameof(GenerateMessages));
            }
        }

        private string _messageNamespace;
        public string MessageNamespace
        {
            get => _messageNamespace;
            set
            {
                _messageNamespace = value;
                UserSettings.MessageNamespace = value;
                OnPropertyChanged(nameof(MessageNamespace));
            }
        }

        private bool _generateCustomActions;
        public bool GenerateCustomActions
        {
            get => _generateCustomActions;
            set
            {
                _generateCustomActions = value;
                UserSettings.GenerateCustomActions = value;
                OnPropertyChanged(nameof(GenerateCustomActions));
            }
        }

        public UserSettings UserSettings { get; }

        public Wizard InternalWizard;

        public string[] LanguageOptions
        {
            get
            {
                return new[]
                {
                    "CS",
                    "VB",
                    "CPP"
                };
            }
        }
        #endregion

        #region Network Credentials
        private bool _useNetworkCredentials;
        public bool UseNetworkCredentials
        {
            get => _useNetworkCredentials;
            set
            {
                _useNetworkCredentials = value;
                UserSettings.UseNetworkCredentials = value;
                OnPropertyChanged(nameof(UseNetworkCredentials));
            }
        }

        public string NetworkCredentialsUserName { get; set; }

        public string NetworkCredentialsPassword { get; set; }

        public string NetworkCredentialsDomain { get; set; }
        #endregion

        #region Constructors
        public ConfigDynamics365EndpointViewModel(UserSettings userSettings, Wizard wizard) : base()
        {
            this.Title = "Configure service endpoint";
            this.Description = "Enter or choose an Dynamics 365 service endpoint and check generation options to begin";
            this.Legend = "Service Endpoint";
            this.InternalWizard = wizard;
            this.View = new ConfigDynamics365Endpoint(this.InternalWizard) { DataContext = this };
            this.UserSettings = userSettings;
            this.ServiceName = string.IsNullOrWhiteSpace(userSettings.ServiceName) ? Constants.DefaultServiceName : userSettings.ServiceName;
            this.Endpoint = userSettings.Endpoint;
            this.UseNetworkCredentials = userSettings.UseNetworkCredentials;
            this.EnableDebugMode = userSettings.EnableDebugMode;
            this.AddClientNuGet = userSettings.AddClientNuGet;
            this.GeneratedFileName = string.IsNullOrWhiteSpace(userSettings.GeneratedFileName) ? Constants.DefaultServiceName : userSettings.GeneratedFileName;
            this.LanguageOption = string.IsNullOrWhiteSpace(userSettings.LanguageOption) ? "cs" : userSettings.LanguageOption;
            this.UseInteractiveLogin = userSettings.UseInteractiveLogin;
            this.Namespace = userSettings.Namespace;
            this.ConnectionString = userSettings.ConnectionString;
            this.UseOAuth = userSettings.UseOAuth;
            this.UseConnectionString = userSettings.UseConnectionString;
            this.ServiceContextName = userSettings.ServiceContextName;
            this.GenerateMessages = userSettings.GenerateMessages;
            this.MessageNamespace = userSettings.MessageNamespace;
            this.GenerateCustomActions = userSettings.GenerateCustomActions;
        }
        #endregion

        #region Methods
        public override async Task<PageNavigationResult> OnPageLeavingAsync(WizardLeavingArgs args)
        {
            UserSettings.AddToTopOfMruList(((Wizard)this.Wizard).UserSettings.MruEndpoints, this.Endpoint);
            try
            {
                if (this.UserSettings.UseNetworkCredentials && string.IsNullOrWhiteSpace(this.UserSettings.Endpoint))
                    throw new ArgumentNullException(nameof(Endpoint), "Please input the Dynamics 365 service endpoint.");

                return await base.OnPageLeavingAsync(args);
            }
            catch (Exception e)
            {
                return await Task.FromResult(
                    new PageNavigationResult
                    {
                        ErrorMessage = e.Message,
                        IsSuccess = false,
                        ShowMessageBoxOnFailure = true
                    });
            }
        }
        #endregion
    }
}
