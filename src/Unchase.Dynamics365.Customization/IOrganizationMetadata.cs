using Microsoft.Xrm.Sdk.Metadata;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Interface for IOrganization metadata
	/// </summary>
    public interface IOrganizationMetadata
	{
		/// <summary>
		/// Array of complete EntityMetadata for the Organization.
		/// </summary>
        EntityMetadata[] Entities { get; }

		/// <summary>
		/// Array of complete OptionSetMetadata for the Organization.
		/// </summary>
        OptionSetMetadataBase[] OptionSets { get; }

		/// <summary>
		/// All SdkMessages for the Organization.
		/// </summary>
        SdkMessages Messages { get; }
	}
}
