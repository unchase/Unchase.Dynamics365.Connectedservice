using System.Threading.Tasks;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Interface that provides metadata for a given organization.
	/// </summary>
    public interface IMetadataProviderService
	{
		/// <summary>
		/// Returns the metadata for a given organization.  Subsequent calls to the method should
		/// return the same set of information on the IOrganizationMetadata object.
		/// </summary>
        Task<IOrganizationMetadata> LoadMetadataAsync();
	}
}
