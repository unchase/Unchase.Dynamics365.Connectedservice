using System;
using System.Threading.Tasks;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
	/// <summary>
	/// Interface that provides metadata for a given organization.
	/// </summary>
    public interface IMetadataProviderService2 : IMetadataProviderService
	{
		/// <summary>
		/// Loads metadata for the given service
		/// </summary>
		/// <param name="service">Service to query</param>
		/// <returns>IOrganizationMetadata</returns>
        Task<IOrganizationMetadata> LoadMetadataAsync(IServiceProvider service);
	}
}
