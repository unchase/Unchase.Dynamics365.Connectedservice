using System.Reflection;
using Microsoft.VisualStudio.Shell;
using Microsoft.Xrm.Sdk.Metadata;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class OrganizationMetadata : IOrganizationMetadata
	{
        internal OrganizationMetadata(EntityMetadata[] entities, OptionSetMetadataBase[] optionSets, SdkMessages messages)
		{
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            });
            this._entities = entities;
			this._optionSets = optionSets;
			this._sdkMessages = messages;
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            });
        }

		EntityMetadata[] IOrganizationMetadata.Entities => this._entities;

        OptionSetMetadataBase[] IOrganizationMetadata.OptionSets => this._optionSets;

        SdkMessages IOrganizationMetadata.Messages => this._sdkMessages;

        private readonly EntityMetadata[] _entities;

		private readonly OptionSetMetadataBase[] _optionSets;

		private readonly SdkMessages _sdkMessages;
	}
}
