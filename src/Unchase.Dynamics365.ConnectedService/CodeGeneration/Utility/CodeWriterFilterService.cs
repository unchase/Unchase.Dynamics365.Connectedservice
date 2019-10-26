using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Metadata;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class CodeWriterFilterService : ICodeWriterFilterService, ICodeWriterMessageFilterService
	{
        static CodeWriterFilterService()
		{
			CodeWriterFilterService.ExcludedNamespaces.Add("http://schemas.microsoft.com/xrm/2011/contracts");
		}

		internal CodeWriterFilterService(CrmSvcUtilParameters parameters)
		{
			this._messageNamespace = parameters.MessageNamespace;
			this._generateMessages = parameters.GenerateMessages;
			this._generateCustomActions = parameters.GenerateCustomActions;
			this._generateServiceContext = !string.IsNullOrWhiteSpace(parameters.ServiceContextName);
		}

		async Task<bool> ICodeWriterFilterService.GenerateOptionSetAsync(OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return optionSetMetadata.OptionSetType != null && optionSetMetadata.OptionSetType.Value == OptionSetType.State;
		}

		async Task<bool> ICodeWriterFilterService.GenerateOptionAsync(OptionMetadata option, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return true;
		}

		async Task<bool> ICodeWriterFilterService.GenerateEntityAsync(EntityMetadata entityMetadata, IServiceProvider services)
		{
			if (entityMetadata == null)
			{
				return false;
			}
			if (entityMetadata.IsIntersect.GetValueOrDefault())
			{
				return true;
			}
			if (string.Equals(entityMetadata.LogicalName, "activityparty", StringComparison.Ordinal))
			{
				return true;
			}
			if (string.Equals(entityMetadata.LogicalName, "calendarrule", StringComparison.Ordinal))
			{
				return true;
			}
			var metadataProviderService = (IMetadataProviderService)services?.GetService(typeof(IMetadataProviderService));
			IOrganizationMetadata organizationMetadata;
			if (metadataProviderService is IMetadataProviderService2 providerService2)
			{
				organizationMetadata = await providerService2.LoadMetadataAsync(services);
			}
			else
			{
				organizationMetadata = await metadataProviderService.LoadMetadataAsync();
			}
			foreach (var sdkMessage in organizationMetadata.Messages.MessageCollection.Values)
			{
				if (!sdkMessage.IsPrivate)
				{
					foreach (var sdkMessageFilter in sdkMessage.SdkMessageFilters.Values)
					{
						if (entityMetadata.ObjectTypeCode != null && sdkMessageFilter.PrimaryObjectTypeCode == entityMetadata.ObjectTypeCode.Value)
						{
							return true;
						}
						if (entityMetadata.ObjectTypeCode != null && sdkMessageFilter.SecondaryObjectTypeCode == entityMetadata.ObjectTypeCode.Value)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		async Task<bool> ICodeWriterFilterService.GenerateAttributeAsync(AttributeMetadata attributeMetadata, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return !this.IsNotExposedChildAttribute(attributeMetadata) && (attributeMetadata.IsValidForCreate.GetValueOrDefault() || attributeMetadata.IsValidForRead.GetValueOrDefault() || attributeMetadata.IsValidForUpdate.GetValueOrDefault());
		}

		/// <summary>
		/// If true a child attribute cannot be published or externally consumed.
		/// </summary>
        private bool IsNotExposedChildAttribute(AttributeMetadata attributeMetadata)
		{
			return !string.IsNullOrEmpty(attributeMetadata.AttributeOf) && !(attributeMetadata is ImageAttributeMetadata) && !attributeMetadata.LogicalName.EndsWith("_url", StringComparison.OrdinalIgnoreCase) && !attributeMetadata.LogicalName.EndsWith("_timestamp", StringComparison.OrdinalIgnoreCase);
		}

		async Task<bool> ICodeWriterFilterService.GenerateRelationshipAsync(RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata, IServiceProvider services)
		{
			var codeWriterFilterService = (ICodeWriterFilterService)services?.GetService(typeof(ICodeWriterFilterService));
			return otherEntityMetadata != null && !string.Equals(otherEntityMetadata.LogicalName, "calendarrule", StringComparison.Ordinal) && await codeWriterFilterService.GenerateEntityAsync(otherEntityMetadata, services);
		}

        async Task<bool> ICodeWriterFilterService.GenerateServiceContextAsync(IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return this._generateServiceContext;
		}

        async Task<bool> ICodeWriterMessageFilterService.GenerateSdkMessageAsync(SdkMessage message,
            IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return (this._generateMessages || this._generateCustomActions) && !message.IsPrivate && message.SdkMessageFilters.Count != 0;
		}

        async Task<bool> ICodeWriterMessageFilterService.GenerateSdkMessagePairAsync(SdkMessagePair messagePair,
            IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return (this._generateMessages || this._generateCustomActions) && !CodeWriterFilterService.ExcludedNamespaces.Contains(messagePair.MessageNamespace.ToUpperInvariant()) && (!this._generateCustomActions || messagePair.Message.IsCustomAction) && (string.IsNullOrEmpty(this._messageNamespace) || string.Equals(this._messageNamespace, messagePair.MessageNamespace, StringComparison.OrdinalIgnoreCase));
		}

		private static readonly List<string> ExcludedNamespaces = new List<string>();

		private readonly string _messageNamespace;

		private readonly bool _generateMessages;

		private readonly bool _generateCustomActions;

		private readonly bool _generateServiceContext;
	}
}
