using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class MetadataProviderQueryService : IMetadataProviderQueryService
	{
        internal MetadataProviderQueryService(CrmSvcUtilParameters parameters)
		{
			this._parameters = parameters;
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (CrmSvcUtil.EnableDebugMode)
                    await CrmSvcUtil.CrmSvcUtilLogger.LogAsync("Creating Default Metadata Provider Query Service");
            });
        }

        public async Task<EntityMetadata[]> RetrieveEntitiesAsync(IOrganizationService service)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            var organizationRequest = new OrganizationRequest("RetrieveAllEntities")
            {
                Parameters =
                {
                    ["EntityFilters"] =EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                    ["RetrieveAsIfPublished"] = false
                }
            };
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return (EntityMetadata[])service.Execute(organizationRequest).Results["EntityMetadata"];
		}

        public async Task<OptionSetMetadataBase[]> RetrieveOptionSetsAsync(IOrganizationService service)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            var organizationRequest = new OrganizationRequest("RetrieveAllOptionSets")
            {
                Parameters = {["RetrieveAsIfPublished"] = true}
            };
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return (OptionSetMetadataBase[])service.Execute(organizationRequest).Results["OptionSetMetadata"];
		}

        async Task<SdkMessages> IMetadataProviderQueryService.RetrieveSdkRequestsAsync(IOrganizationService service)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
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
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
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

        private readonly CrmSvcUtilParameters _parameters;
	}
}
