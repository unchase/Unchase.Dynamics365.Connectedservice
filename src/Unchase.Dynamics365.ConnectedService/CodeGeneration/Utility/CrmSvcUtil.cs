using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.Xrm.Sdk;
using Unchase.Dynamics365.Customization;
using Task = System.Threading.Tasks.Task;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class CrmSvcUtil
	{
        internal static bool EnableDebugMode = false;

        internal CrmSvcUtil(Dynamics365CodeGenDescriptor serviceDescriptor)
        {
            EnableDebugMode = serviceDescriptor.Instance.ServiceConfig.EnableDebugMode;
            this._methodTracer = new MethodTracer();
            CrmSvcUtilLogger = new TraceLogger(serviceDescriptor.Context.Logger);
            this.Parameters = new CrmSvcUtilParameters(this._methodTracer);
        }

        private readonly MethodTracer _methodTracer;

        private CrmSvcUtilParameters Parameters { get; }

        private ServiceProvider ServiceProvider
		{
			get
			{
				if (this._serviceProvider == null)
				{
					this._serviceProvider = new ServiceProvider();
                    ThreadHelper.JoinableTaskFactory.Run(async delegate {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        await this._serviceProvider.InitializeServicesAsync(this.Parameters);
                    });
                }
				return this._serviceProvider;
			}
		}

		internal static string ApplicationName => ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private static string ApplicationDescription => ((AssemblyDescriptionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;

        internal static string ApplicationVersion => ((AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0]).Version;

        private static string ApplicationCopyright => ((AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;

        private async Task LoadArgumentsAsync(string[] args)
		{
			await this.Parameters.LoadArgumentsAsync(args);
		}

		private async Task<bool> RunAsync()
		{
            await this._methodTracer.EnterAsync();
            await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("Loading service metadata. May take several minutes...");
            if (this.ServiceProvider.MetadataProviderService is IMetadataProviderService2 providerService2)
			{
                this._organizationMetadata = await providerService2.LoadMetadataAsync(this.ServiceProvider);
			}
			else
			{
                this._organizationMetadata = await this.ServiceProvider.MetadataProviderService.LoadMetadataAsync();
			}
			if (this._organizationMetadata == null)
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("{0} returned null metadata", typeof(IMetadataProviderService).Name);
				return false;
			}
            else
            {
                await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("Loading service metadata is complete!");
            }
            await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("Generating service client code. May take several minutes...");
            await this.WriteCodeAsync(this._organizationMetadata);
            await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("Generating service client code is complete!");
            return true;
		}

		private async Task WriteCodeAsync(IOrganizationMetadata organizationMetadata)
		{
            await this._methodTracer.EnterAsync();
			await this.ServiceProvider.CodeGenerationService.WriteAsync(organizationMetadata, this.Parameters.Language, this.Parameters.OutputFile, this.Parameters.Namespace, this.ServiceProvider);
            await this._methodTracer.ExitAsync();
		}

        /// <summary>
        /// Start early-bound classes generation.
        /// </summary>
        /// <remarks>
        /// Class entry point.
        /// </remarks>
        /// <param name="args">Arguments.</param>
        internal async Task<bool> StartGenerationAsync(string[] args)
        {
            try
			{
                await this.LoadArgumentsAsync(args);
				if (!await this.Parameters.VerifyArgumentsAsync())
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Exiting program due to error while verify arguments.");
                    return false;
				}
			}
			catch (InvalidOperationException ex)
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Exiting program due to exception : {0}", ex);
                return false;
			}
			bool result;
			try
			{
                result = await this.RunAsync();
            }
			catch (FaultException<OrganizationServiceFault> faultException)
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Exiting program due to exception : {0}", faultException.Detail);
				await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("===== DETAIL ======", LoggerMessageCategory.Error);
				await CrmSvcUtil.CrmSvcUtilLogger.LogAsync(faultException);
				result = false;
			}
			catch (MessageSecurityException ex)
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Exiting program due to exception : {0}", ex.InnerException);
				await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("===== DETAIL ======", LoggerMessageCategory.Error);
				await CrmSvcUtil.CrmSvcUtilLogger.LogAsync(ex);
				result = false;
			}
			catch (Exception ex2)
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Exiting program due to exception : {0}", ex2);
				await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("===== DETAIL ======", LoggerMessageCategory.Error);
				await CrmSvcUtil.CrmSvcUtilLogger.LogAsync(ex2);
				result = false;
			}
			return result;
		}

        private ServiceProvider _serviceProvider;

		private IOrganizationMetadata _organizationMetadata;

		internal static TraceLogger CrmSvcUtilLogger;
    }
}
