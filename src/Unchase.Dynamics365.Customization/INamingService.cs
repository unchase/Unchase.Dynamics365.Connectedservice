using System;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Used by the ICodeGenerationService to retrieve names for the CodeDOM objects being created.
	/// </summary>
    public interface INamingService
	{
		/// <summary>
		/// Returns a name for the OptionSet being generated.
		/// </summary>
        Task<string> GetNameForOptionSetAsync(EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the Option being generated.
		/// </summary>
        Task<string> GetNameForOptionAsync(OptionSetMetadataBase optionSetMetadata, OptionMetadata optionMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the Entity being generated.
		/// </summary>
        Task<string> GetNameForEntityAsync(EntityMetadata entityMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the Attribute being generated.
		/// </summary>
        Task<string> GetNameForAttributeAsync(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the 1:N, N:N, or N:1 relationship being generated.
		/// </summary>
        Task<string> GetNameForRelationshipAsync(EntityMetadata entityMetadata, RelationshipMetadataBase relationshipMetadata, EntityRole? reflexiveRole, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the data context being generated.
		/// </summary>
        Task<string> GetNameForServiceContextAsync(IServiceProvider services);

		/// <summary>
		/// Retrieves a name for a set of entities.
		/// </summary>
        Task<string> GetNameForEntitySetAsync(EntityMetadata entityMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the MessagePair being generated.
		/// </summary>
        Task<string> GetNameForMessagePairAsync(SdkMessagePair messagePair, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the Request Field being generated.
		/// </summary>
        Task<string> GetNameForRequestFieldAsync(SdkMessageRequest request, SdkMessageRequestField requestField, IServiceProvider services);

		/// <summary>
		/// Retrieves a name for the Response Field being generated.
		/// </summary>
        Task<string> GetNameForResponseFieldAsync(SdkMessageResponse response, SdkMessageResponseField responseField, IServiceProvider services);
	}
}
