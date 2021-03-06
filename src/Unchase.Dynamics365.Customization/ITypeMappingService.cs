﻿using System;
using System.CodeDom;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Metadata;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Used by the ICodeGenerationService to retrieve types for the CodeDOM objects being created.
	/// </summary>
    public interface ITypeMappingService
	{
		/// <summary>
		/// Retrieves a CodeTypeReference for the entity set being generated.
		/// </summary>
        Task<CodeTypeReference> GetTypeForEntityAsync(EntityMetadata entityMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a CodeTypeReference for the attribute being generated.
		/// </summary>
        Task<CodeTypeReference> GetTypeForAttributeTypeAsync(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a CodeTypeReference for the 1:N, N:N, or N:1 relationship being generated.
		/// </summary>
        Task<CodeTypeReference> GetTypeForRelationshipAsync(RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata, IServiceProvider services);

		/// <summary>
		/// Retrieves a CodeTypeReference for the Request Field being generated.
		/// </summary>
        Task<CodeTypeReference> GetTypeForRequestFieldAsync(SdkMessageRequestField requestField, IServiceProvider services);

		/// <summary>
		/// Retrieves a CodeTypeReference for the Response Field being generated.
		/// </summary>
        Task<CodeTypeReference> GetTypeForResponseFieldAsync(SdkMessageResponseField responseField, IServiceProvider services);
	}
}
