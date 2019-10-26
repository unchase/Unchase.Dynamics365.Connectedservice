using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Interface for metadata provider query service
	/// </summary>
    public interface IMetadataProviderQueryService
	{
        /// <summary>
        /// Retrieves entities for the given service
        /// </summary>
        /// <param name="service">Service to query</param>
        /// <returns>An EntityMetadata array</returns>
        Task<EntityMetadata[]> RetrieveEntitiesAsync(IOrganizationService service);

        /// <summary>
        /// Retrieves option sets for the given service
        /// </summary>
        /// <param name="service">Service to query</param>
        /// <returns>An OptionSetMetadataBase array</returns>
        Task<OptionSetMetadataBase[]> RetrieveOptionSetsAsync(IOrganizationService service);

        /// <summary>
        /// Retrieves SDK requests for the given service
        /// </summary>
        /// <param name="service">Service to query</param>
        /// <returns>SdkMessages</returns>
        Task<SdkMessages> RetrieveSdkRequestsAsync(IOrganizationService service);
	}
}
