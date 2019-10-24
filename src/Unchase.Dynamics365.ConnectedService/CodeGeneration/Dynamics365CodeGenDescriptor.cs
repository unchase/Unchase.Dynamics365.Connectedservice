using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility;
using Unchase.Dynamics365.ConnectedService.Common;
using Unchase.Dynamics365.ConnectedService.Models;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration
{

    internal class Dynamics365CodeGenDescriptor : BaseCodeGenDescriptor
    {
        #region Constructors
        public Dynamics365CodeGenDescriptor(ConnectedServiceHandlerContext context, Instance serviceInstance) : base(context, serviceInstance) { }
        #endregion

        #region Methods

        #region NuGet
        public override async Task AddNugetPackagesAsync()
        {
            await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "Adding Nuget Packages for Dynamics365 service Client...");
            var packageSource = Constants.NuGetOnlineRepository;
            await CheckAndInstallNuGetPackageAsync(packageSource, Constants.MicrosoftCrmSdkXrmToolingCoreAssemblyNuGetPackage);
            await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "Nuget Packages for Dynamics365 service Client were installed.");
        }

        internal async Task CheckAndInstallNuGetPackageAsync(string packageSource, string nugetPackage)
        {
            try
            {
                if (!PackageInstallerServices.IsPackageInstalled(this.Project, nugetPackage))
                {
                    PackageInstaller.InstallPackage(packageSource, this.Project, nugetPackage, (Version)null, false);
                    await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Nuget Package \"{nugetPackage}\" for Dynamics 365 service was added.");
                }
                else
                    await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Nuget Package \"{nugetPackage}\" for Dynamics 365 service already installed.");
            }
            catch (Exception ex)
            {
                await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Warning, $"Nuget Package \"{nugetPackage}\" for Dynamics 365 service not installed. Error: {ex.Message}.");
            }
        }
        #endregion

        #region Generation
        public override async Task AddGeneratedCodeAsync()
        {
            await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "Generating Client Proxy for Dynamics365 service Client...");
            try
            {
                if (await GenerateCodeAsync(Context, Instance))
                    await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "Client Proxy for Dynamics365 service Client was generated.");
                else
                    await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Warning, $"Client Proxy for Dynamics365 service Client was not generated.");
            }
            catch (Exception e)
            {
                await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Warning, $"Client Proxy for Dynamics365 service Client was not generated. Error: {e.Message}.");
            }
        }

        internal async Task<bool> GenerateCodeAsync(ConnectedServiceHandlerContext context, Instance instance)
        {
            var result = false;
            var serviceFolder = instance.Name;
            var clientTempFileName = Path.GetTempFileName();
            try
            {
                var rootFolder = context.HandlerHelper.GetServiceArtifactsRootFolder();
                var folderPath = context.ProjectHierarchy.GetProject().GetServiceFolderPath(rootFolder, serviceFolder);
                var generationUtil = new CrmSvcUtil(this);
                result = await generationUtil.StartGenerationAsync(GetArguments(instance.ServiceConfig, clientTempFileName));
                if (!result)
                {
                    await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Error, $"Internal error occured. See upper.");
                }
                else
                {
                    await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Copying generated file to project...");
                    await context.HandlerHelper.AddFileAsync(clientTempFileName,
                        Path.Combine(folderPath,
                            $"{instance.ServiceConfig.GeneratedFileNamePrefix}Generated.{instance.ServiceConfig.LanguageOption.ToLower()}"),
                        new AddFileOptions { OpenOnComplete = instance.ServiceConfig.OpenGeneratedFilesOnComplete });
                    await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Generated file was copied.");
                }
            }
            catch (Exception ex)
            {
                await this.Context.Logger.WriteMessageAsync(LoggerMessageCategory.Warning, $"Error: {ex.Message}.");
            }
            finally
            {
                if (File.Exists(clientTempFileName))
                    File.Delete(clientTempFileName);
            }
            return result;
        }

        private string[] GetArguments(ServiceConfiguration serviceConfiguration, string clientTempFileName)
        {
            var result = new List<string>();

            #region Required
            result.Add($"/o:{clientTempFileName}");
            result.Add($"/l:{serviceConfiguration.LanguageOption}");
            if (serviceConfiguration.UseNetworkCredentials && string.IsNullOrWhiteSpace(serviceConfiguration.Endpoint))
                throw new ArgumentNullException(nameof(serviceConfiguration.Endpoint), "Dynamics 365 service endpoint is null or empty.");
            else
                result.Add($"/url:{serviceConfiguration.Endpoint}");
            #endregion

            #region Optional
            if (serviceConfiguration.UseNetworkCredentials)
            {
                if (!string.IsNullOrWhiteSpace(serviceConfiguration.NetworkCredentialsUserName))
                    result.Add($"/username:{serviceConfiguration.NetworkCredentialsUserName}");
                if (!string.IsNullOrWhiteSpace(serviceConfiguration.NetworkCredentialsPassword))
                    result.Add($"/password:{serviceConfiguration.NetworkCredentialsPassword}");
                if (!string.IsNullOrWhiteSpace(serviceConfiguration.NetworkCredentialsDomain))
                    result.Add($"/domain:{serviceConfiguration.NetworkCredentialsDomain}");
                if (serviceConfiguration.UseOAuth)
                    result.Add($"/useoauth:true");
            }
            else
            {
                if (serviceConfiguration.UseConnectionString && !string.IsNullOrWhiteSpace(serviceConfiguration.ConnectionString))
                    result.Add($"/connstr:{serviceConfiguration.ConnectionString}");
                if (serviceConfiguration.UseInteractiveLogin)
                    result.Add($"/il");
            }
            
            if (!string.IsNullOrWhiteSpace(serviceConfiguration.Namespace))
                result.Add($"/namespace:{serviceConfiguration.Namespace}");
            if (serviceConfiguration.GenerateMessages)
                result.Add($"/includeMessages");
            if (serviceConfiguration.GenerateCustomActions)
                result.Add($"/generateActions");
            if (!string.IsNullOrWhiteSpace(serviceConfiguration.MessageNamespace))
                result.Add($"/m:{serviceConfiguration.MessageNamespace}");
            if (!string.IsNullOrWhiteSpace(serviceConfiguration.ServiceContextName))
                result.Add($"/serviceContextName:{serviceConfiguration.ServiceContextName}");

            //ToDo: добавить остальные параметры (от интерфейсов)
            #endregion

            return result.ToArray();
        }
        #endregion

        #endregion
    }
}
