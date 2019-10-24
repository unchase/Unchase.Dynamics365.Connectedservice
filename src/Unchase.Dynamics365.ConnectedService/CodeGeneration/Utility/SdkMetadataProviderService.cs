using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Unchase.Dynamics365.ConnectedService.View;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Tooling.Connector;
using System.Data.Common;
using System.Threading.Tasks;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class SdkMetadataProviderService : IMetadataProviderService2, IMetadataProviderService
	{
        internal SdkMetadataProviderService(CrmSvcUtilParameters parameters)
		{
			this.Parameters = parameters;
		}

		private CrmSvcUtilParameters Parameters { get; }

        public async Task<IOrganizationMetadata> LoadMetadataAsync()
		{
			if (this._organizationMetadata == null)
			{
				var organizationService = await this.CreateOrganizationServiceEndpointAsync();
				if (organizationService == null)
				{
					throw new Exception("Connection to CRM is not established. Aborting process.");
				}
				this.SetConnectionTimeoutValues();
				var entityMetadata = this.RetrieveEntities(organizationService);
				var optionSetMetadata = this.RetrieveOptionSets(organizationService);
				var messages = this.RetrieveSdkRequests(organizationService);
				this._organizationMetadata = this.CreateOrganizationMetadata(entityMetadata, optionSetMetadata, messages);
			}
			return this._organizationMetadata;
		}

		public async Task<IOrganizationMetadata> LoadMetadataAsync(IServiceProvider service)
		{
			if (this._organizationMetadata == null)
			{
				if (service == null)
				{
					return null;
				}
				var serviceProvider = (ServiceProvider)service;
				var organizationService = await this.CreateOrganizationServiceEndpointAsync();
				if (organizationService == null)
				{
					throw new Exception("Connection to CRM is not established. Aborting process.");
				}
				this.SetConnectionTimeoutValues();
				var entityMetadata = serviceProvider.MetadataProviderQueryService.RetrieveEntities(organizationService);
				var optionSetMetadata = serviceProvider.MetadataProviderQueryService.RetrieveOptionSets(organizationService);
				var messages = serviceProvider.MetadataProviderQueryService.RetrieveSdkRequests(organizationService);
				this._organizationMetadata = this.CreateOrganizationMetadata(entityMetadata, optionSetMetadata, messages);
			}
			return this._organizationMetadata;
		}

		/// <summary>
		/// Updates the timeout value to extend the amount of item that a request will wait.
		/// </summary>
        private void SetConnectionTimeoutValues()
		{
			if (this._crmSvcCli != null)
			{
				if (this._crmSvcCli.ActiveAuthenticationType == Microsoft.Xrm.Tooling.Connector.AuthenticationType.OAuth)
				{
					if (this._crmSvcCli.OrganizationWebProxyClient != null)
					{
						this._crmSvcCli.OrganizationWebProxyClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(20.0);
                        var endpointBinding = this._crmSvcCli.OrganizationWebProxyClient.Endpoint.Binding;
                        if (endpointBinding != null)
                        {
                            endpointBinding.SendTimeout = TimeSpan.FromMinutes(20.0);
                            endpointBinding.ReceiveTimeout = TimeSpan.FromMinutes(20.0);
                        }

                        return;
					}
				}
				else if (this._crmSvcCli.OrganizationServiceProxy != null)
				{
					this._crmSvcCli.OrganizationServiceProxy.Timeout = TimeSpan.FromMinutes(20.0);
				}
			}
		}

		private EntityMetadata[] RetrieveEntities(IOrganizationService service)
		{
            var organizationRequest = new OrganizationRequest("RetrieveAllEntities")
            {
                Parameters =
                {
                    ["EntityFilters"] = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                    ["RetrieveAsIfPublished"] = false
                }
            };
            return (EntityMetadata[])service.Execute(organizationRequest).Results["EntityMetadata"];
		}

		private OptionSetMetadataBase[] RetrieveOptionSets(IOrganizationService service)
		{
            var organizationRequest = new OrganizationRequest("RetrieveAllOptionSets")
            {
                Parameters = {["RetrieveAsIfPublished"] = true}
            };
            return (OptionSetMetadataBase[])service.Execute(organizationRequest).Results["OptionSetMetadata"];
		}

		private SdkMessages RetrieveSdkRequests(IOrganizationService service)
		{
			const string text = "<fetch distinct='true' version='1.0'>\r\n\t<entity name='sdkmessage'>\r\n\t\t<attribute name='name'/>\r\n\t\t<attribute name='isprivate'/>\r\n\t\t<attribute name='sdkmessageid'/>\r\n\t\t<attribute name='customizationlevel'/>\r\n\t\t<link-entity name='sdkmessagepair' alias='sdkmessagepair' to='sdkmessageid' from='sdkmessageid' link-type='inner'>\r\n\t\t\t<filter>\r\n\t\t\t\t<condition alias='sdkmessagepair' attribute='endpoint' operator='eq' value='2011/Organization.svc' />\r\n\t\t\t</filter>\r\n\t\t\t<attribute name='sdkmessagepairid'/>\r\n\t\t\t<attribute name='namespace'/>\r\n\t\t\t<link-entity name='sdkmessagerequest' alias='sdkmessagerequest' to='sdkmessagepairid' from='sdkmessagepairid' link-type='outer'>\r\n\t\t\t\t<attribute name='sdkmessagerequestid'/>\r\n\t\t\t\t<attribute name='name'/>\r\n\t\t\t\t<link-entity name='sdkmessagerequestfield' alias='sdkmessagerequestfield' to='sdkmessagerequestid' from='sdkmessagerequestid' link-type='outer'>\r\n\t\t\t\t\t<attribute name='name'/>\r\n\t\t\t\t\t<attribute name='optional'/>\r\n\t\t\t\t\t<attribute name='position'/>\r\n\t\t\t\t\t<attribute name='publicname'/>\r\n\t\t\t\t\t<attribute name='clrparser'/>\r\n\t\t\t\t\t<order attribute='sdkmessagerequestfieldid' descending='false' />\r\n\t\t\t\t</link-entity>\r\n\t\t\t\t<link-entity name='sdkmessageresponse' alias='sdkmessageresponse' to='sdkmessagerequestid' from='sdkmessagerequestid' link-type='outer'>\r\n\t\t\t\t\t<attribute name='sdkmessageresponseid'/>\r\n\t\t\t\t\t<link-entity name='sdkmessageresponsefield' alias='sdkmessageresponsefield' to='sdkmessageresponseid' from='sdkmessageresponseid' link-type='outer'>\r\n\t\t\t\t\t\t<attribute name='publicname'/>\r\n\t\t\t\t\t\t<attribute name='value'/>\r\n\t\t\t\t\t\t<attribute name='clrformatter'/>\r\n\t\t\t\t\t\t<attribute name='name'/>\r\n\t\t\t\t\t\t<attribute name='position' />\r\n\t\t\t\t\t</link-entity>\r\n\t\t\t\t</link-entity>\r\n\t\t\t</link-entity>\r\n\t\t</link-entity>\r\n\t\t<link-entity name='sdkmessagefilter' alias='sdmessagefilter' to='sdkmessageid' from='sdkmessageid' link-type='inner'>\r\n\t\t\t<filter>\r\n\t\t\t\t<condition alias='sdmessagefilter' attribute='isvisible' operator='eq' value='1' />\r\n\t\t\t</filter>\r\n\t\t\t<attribute name='sdkmessagefilterid'/>\r\n\t\t\t<attribute name='primaryobjecttypecode'/>\r\n\t\t\t<attribute name='secondaryobjecttypecode'/>\r\n\t\t</link-entity>\r\n\t\t<order attribute='sdkmessageid' descending='false' />\r\n\t </entity>\r\n</fetch>";
			MessagePagingInfo messagePagingInfo = null;
			var num = 1;
			var sdkMessages = new SdkMessages(null);
			var organizationRequest = new OrganizationRequest("ExecuteFetch");
			while (messagePagingInfo == null || messagePagingInfo.HasMoreRecords)
			{
				var value = text;
				if (messagePagingInfo != null)
				{
					value = this.SetPagingCookie(text, messagePagingInfo.PagingCookie, num);
				}
				organizationRequest.Parameters["FetchXml"] = value;
				var organizationResponse = service.Execute(organizationRequest);
				messagePagingInfo = SdkMessages.FromFetchResult(sdkMessages, (string)organizationResponse.Results["FetchXmlResult"]);
				num++;
			}
			return sdkMessages;
		}

		private string SetPagingCookie(string fetchQuery, string pagingCookie, int pageNumber)
		{
			var xDocument = XDocument.Parse(fetchQuery);
			if (pagingCookie != null)
			{
				xDocument.Root?.Add(new XAttribute(XName.Get("paging-cookie"), pagingCookie));
			}
			xDocument.Root?.Add(new XAttribute(XName.Get("page"), pageNumber.ToString(CultureInfo.InvariantCulture)));
			return xDocument.ToString();
		}

		private IOrganizationMetadata CreateOrganizationMetadata(EntityMetadata[] entityMetadata, OptionSetMetadataBase[] optionSetMetadata, SdkMessages messages)
		{
			return new OrganizationMetadata(entityMetadata, optionSetMetadata, messages);
		}

		private async Task<IOrganizationService> CreateOrganizationServiceEndpointAsync()
		{
			var hostProfileName = string.IsNullOrEmpty(this.Parameters.ConnectionProfileName) ? "default" : this.Parameters.ConnectionProfileName;
			if (!string.IsNullOrEmpty(this.Parameters.ConnectionAppName))
			{
                var crmInteractiveLogin = new CRMInteractiveLogin
                {
                    HostProfileName = hostProfileName, HostApplicationNameOverride = this.Parameters.ConnectionAppName
                };
                crmInteractiveLogin.ShowDialog();
				if (crmInteractiveLogin.CrmConnectionMgr?.CrmSvc == null || !crmInteractiveLogin.CrmConnectionMgr.CrmSvc.IsReady)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync(this._crmSvcCli.LastCrmError);
					return null;
				}
				this._crmSvcCli = crmInteractiveLogin.CrmConnectionMgr.CrmSvc;
				IOrganizationService organizationService;
				if (this._crmSvcCli.OrganizationServiceProxy == null)
				{
					organizationService = this._crmSvcCli.OrganizationWebProxyClient;
					return organizationService;
				}
				organizationService = this._crmSvcCli.OrganizationServiceProxy;
				return organizationService;
			}
			else if (!this.Parameters.UseInteractiveLogin)
			{
				this._crmSvcCli = new CrmServiceClient(this.GetConnectionString());
				if (this._crmSvcCli == null || !this._crmSvcCli.IsReady)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync(this._crmSvcCli.LastCrmError);
					return null;
				}
				IOrganizationService organizationService;
				if (this._crmSvcCli.OrganizationServiceProxy == null)
				{
					organizationService = this._crmSvcCli.OrganizationWebProxyClient;
					return organizationService;
				}
				organizationService = this._crmSvcCli.OrganizationServiceProxy;
				return organizationService;
			}
			else
			{
                var crmInteractiveLogin2 = new CRMInteractiveLogin
                {
                    ForceDirectLogin = true, HostProfileName = hostProfileName
                };
                if (!string.IsNullOrEmpty(this.Parameters.ConnectionAppName))
				{
					crmInteractiveLogin2.HostApplicationNameOverride = this.Parameters.ConnectionAppName;
				}
				if (!crmInteractiveLogin2.ShowDialog().Value)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("User aborted Login");
					return null;
				}
				IOrganizationService organizationService;
				try
				{
					if (crmInteractiveLogin2.CrmConnectionMgr?.CrmSvc != null && crmInteractiveLogin2.CrmConnectionMgr.CrmSvc.IsReady)
					{
						this._crmSvcCli = crmInteractiveLogin2.CrmConnectionMgr.CrmSvc;
                        IOrganizationService organizationService2;
						if (this._crmSvcCli.OrganizationServiceProxy == null)
						{
							organizationService = this._crmSvcCli.OrganizationWebProxyClient;
							organizationService2 = organizationService;
						}
						else
						{
							organizationService = this._crmSvcCli.OrganizationServiceProxy;
							organizationService2 = organizationService;
						}
						organizationService = organizationService2;
					}
					else
					{
                        await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync(this._crmSvcCli.LastCrmError);
						organizationService = null;
					}
				}
				catch (Exception ex)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Failed to Login : {0}", ex);
					organizationService = null;
				}
				return organizationService;
			}
		}

		private static string GetValueOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

		/// <summary>
		/// Builds a connection string from the passed in parameters.
		/// </summary>
		/// <returns></returns>
        private string GetConnectionString()
		{
			if (!string.IsNullOrEmpty(this.Parameters.ConnectionString))
			{
				return this.Parameters.ConnectionString;
			}

            if (!Uri.TryCreate(this.Parameters.Url, UriKind.RelativeOrAbsolute, out var uri))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot connect to organization service at {0}", this.Parameters.Url));
			}
			var stringBuilder = new StringBuilder();
			DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "Server", uri.ToString());
			DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "UserName", this.Parameters.UserName);
			DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "Password", this.Parameters.Password);
			if (!string.IsNullOrEmpty(this.Parameters.Domain))
			{
				DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "Domain", this.Parameters.Domain);
				DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "AuthType", "AD");
			}
			else if (this.Parameters.UseOAuth)
			{
				DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "AuthType", "OAuth");
				DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "ClientId", "2ad88395-b77d-4561-9441-d0e40824f9bc");
				DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "RedirectUri", "app://5d3e90d6-aa8e-48a8-8f2c-58b45cc67315");
				DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "LoginPrompt", "Never");
			}
			else
			{
				DbConnectionStringBuilder.AppendKeyValuePair(stringBuilder, "AuthType", "Office365");
			}
			return stringBuilder.ToString();
		}

        private IOrganizationMetadata _organizationMetadata;

		private CrmServiceClient _crmSvcCli;
	}
}
