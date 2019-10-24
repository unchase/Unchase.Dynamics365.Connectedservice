using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.Shell;
using Unchase.Dynamics365.ConnectedService.Models;
using Unchase.Dynamics365.ConnectedService.View;
using Unchase.Dynamics365.ConnectedService.ViewModels;

namespace Unchase.Dynamics365.ConnectedService
{
    internal class Wizard : ConnectedServiceWizard
    {
        #region Fields
        private Instance _serviceInstance;
        #endregion

        #region Properties
        public ConfigDynamics365EndpointViewModel ConfigDynamics365EndpointViewModel { get; set; }

        public ConnectedServiceProviderContext Context { get; set; }

        public Instance ServiceInstance => this._serviceInstance ?? (this._serviceInstance = new Instance());

        public UserSettings UserSettings { get; set; }
        #endregion

        #region Constructors
        public Wizard(ConnectedServiceProviderContext context)
        {
            this.Context = context;

            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                this.UserSettings = await UserSettings.LoadAsync(context.Logger);
            });

            ConfigDynamics365EndpointViewModel = new ConfigDynamics365EndpointViewModel(this.UserSettings, this);

            if (this.Context.IsUpdating)
            {
                var serviceConfig = this.Context.GetExtendedDesignerData<ServiceConfiguration>();
                ConfigDynamics365EndpointViewModel.Endpoint = serviceConfig.Endpoint;
                ConfigDynamics365EndpointViewModel.ServiceName = serviceConfig.ServiceName;
                ConfigDynamics365EndpointViewModel.EnableDebugMode = serviceConfig.EnableDebugMode;
                ConfigDynamics365EndpointViewModel.AddClientNuGet = serviceConfig.AddClientNuGet;
                ConfigDynamics365EndpointViewModel.LanguageOption = serviceConfig.LanguageOption;
                ConfigDynamics365EndpointViewModel.UseInteractiveLogin = serviceConfig.UseInteractiveLogin;
                ConfigDynamics365EndpointViewModel.Namespace = serviceConfig.Namespace;
                ConfigDynamics365EndpointViewModel.ConnectionString = serviceConfig.ConnectionString;
                ConfigDynamics365EndpointViewModel.UseOAuth = serviceConfig.UseOAuth;
                ConfigDynamics365EndpointViewModel.UseConnectionString = serviceConfig.UseConnectionString;
                ConfigDynamics365EndpointViewModel.ServiceContextName = serviceConfig.ServiceContextName;
                ConfigDynamics365EndpointViewModel.GenerateMessages = serviceConfig.GenerateMessages;
                ConfigDynamics365EndpointViewModel.MessageNamespace = serviceConfig.MessageNamespace;
                ConfigDynamics365EndpointViewModel.GenerateCustomActions = serviceConfig.GenerateCustomActions;
                ConfigDynamics365EndpointViewModel.NetworkCredentialsDomain = serviceConfig.NetworkCredentialsDomain;
                ConfigDynamics365EndpointViewModel.NetworkCredentialsUserName = serviceConfig.NetworkCredentialsUserName;
                ConfigDynamics365EndpointViewModel.NetworkCredentialsPassword = serviceConfig.NetworkCredentialsPassword;
                ConfigDynamics365EndpointViewModel.UseNetworkCredentials = serviceConfig.UseNetworkCredentials;
                ConfigDynamics365EndpointViewModel.Description = "An Dynamics 365 service endpoint and generation options was regenerated";
                if (ConfigDynamics365EndpointViewModel.View is ConfigDynamics365Endpoint сonfigDynamics365Endpoint)
                {
                    //сonfigOpenApiEndpoint.Endpoint.IsEnabled = false;
                    //сonfigOpenApiEndpoint.ServiceName.IsEnabled = false;
                }
            }

            this.Pages.Add(ConfigDynamics365EndpointViewModel);
            this.IsFinishEnabled = true;
        }
        #endregion

        #region Methods
        public override async Task<ConnectedServiceInstance> GetFinishedServiceInstanceAsync()
        {
            await this.UserSettings.SaveAsync();
            this.ServiceInstance.Name = ConfigDynamics365EndpointViewModel.UserSettings.ServiceName;
            this.ServiceInstance.ServiceConfig = this.CreateServiceConfiguration();

            #region Network Credentials
            this.ServiceInstance.ServiceConfig.UseNetworkCredentials =
                ConfigDynamics365EndpointViewModel.UserSettings.UseNetworkCredentials;
            this.ServiceInstance.ServiceConfig.NetworkCredentialsUserName =
                ConfigDynamics365EndpointViewModel.NetworkCredentialsUserName;
            this.ServiceInstance.ServiceConfig.NetworkCredentialsPassword =
                ConfigDynamics365EndpointViewModel.NetworkCredentialsPassword;
            this.ServiceInstance.ServiceConfig.NetworkCredentialsDomain =
                ConfigDynamics365EndpointViewModel.NetworkCredentialsDomain;
            #endregion

            return this.ServiceInstance;
        }

        /// <summary>
        /// Create the service configuration.
        /// </summary>
        private ServiceConfiguration CreateServiceConfiguration()
        {
            return new ServiceConfiguration
            {
                ServiceName = string.IsNullOrWhiteSpace(ConfigDynamics365EndpointViewModel.UserSettings.ServiceName) ? Constants.DefaultServiceName : ConfigDynamics365EndpointViewModel.UserSettings.ServiceName,
                Endpoint = ConfigDynamics365EndpointViewModel.UserSettings.Endpoint,
                EnableDebugMode = ConfigDynamics365EndpointViewModel.UserSettings.EnableDebugMode,
                AddClientNuGet = ConfigDynamics365EndpointViewModel.UserSettings.AddClientNuGet,
                LanguageOption = ConfigDynamics365EndpointViewModel.UserSettings.LanguageOption,
                GeneratedFileNamePrefix = ConfigDynamics365EndpointViewModel.UserSettings.GeneratedFileName,
                UseInteractiveLogin = ConfigDynamics365EndpointViewModel.UserSettings.UseInteractiveLogin,
                Namespace = ConfigDynamics365EndpointViewModel.UserSettings.Namespace,
                ConnectionString = ConfigDynamics365EndpointViewModel.UserSettings.ConnectionString,
                UseOAuth = ConfigDynamics365EndpointViewModel.UserSettings.UseOAuth,
                UseConnectionString = ConfigDynamics365EndpointViewModel.UserSettings.UseConnectionString,
                ServiceContextName = ConfigDynamics365EndpointViewModel.UserSettings.ServiceContextName,
                GenerateMessages = ConfigDynamics365EndpointViewModel.UserSettings.GenerateMessages,
                MessageNamespace = ConfigDynamics365EndpointViewModel.UserSettings.MessageNamespace,
                GenerateCustomActions = ConfigDynamics365EndpointViewModel.UserSettings.GenerateCustomActions,
                UseNetworkCredentials = ConfigDynamics365EndpointViewModel.UserSettings.UseNetworkCredentials,
                OpenGeneratedFilesOnComplete = ConfigDynamics365EndpointViewModel.UserSettings.OpenGeneratedFilesOnComplete
            };
        }

        /// <summary>
        /// Cleanup object references.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this.ConfigDynamics365EndpointViewModel != null)
                    {
                        this.ConfigDynamics365EndpointViewModel.Dispose();
                        this.ConfigDynamics365EndpointViewModel = null;
                    }

                    if (this._serviceInstance != null)
                    {
                        this._serviceInstance.Dispose();
                        this._serviceInstance = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
