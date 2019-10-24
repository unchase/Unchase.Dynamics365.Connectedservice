using System;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Metadata;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Interface that provides the ability to generate code based on organization metadata.
	/// </summary>
    public interface ICodeGenerationService
	{
        /// <summary>
        /// Writes code based on the organization metadata.
        /// </summary>
        /// <param name="organizationMetadata">Organization metadata to generate the code for.</param>
        /// <param name="language">Language to generate</param>
        /// <param name="outputFile">Output file to write the generated code to.</param>
        /// <param name="targetNamespace">Target namespace for the generated code.</param>
        /// <param name="services">ServiceProvider to query for additional services that can be used during code generation.</param>
        Task WriteAsync(IOrganizationMetadata organizationMetadata, string language, string outputFile, string targetNamespace, IServiceProvider services);

		/// <summary>
		/// Returns the type that gets generated for the OptionSetMetadata
		/// </summary>
        CodeGenerationType GetTypeForOptionSet(EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata, IServiceProvider services);

		/// <summary>
		/// Returns the type that gets generated for the Option
		/// </summary>
        CodeGenerationType GetTypeForOption(OptionSetMetadataBase optionSetMetadata, OptionMetadata optionMetadata, IServiceProvider services);

		/// <summary>
		/// Returns the type that gets generated for the EntityMetadata
		/// </summary>
        CodeGenerationType GetTypeForEntity(EntityMetadata entityMetadata, IServiceProvider services);

		/// <summary>
		/// Returns the type that gets generated for the AttributeMetadata
		/// </summary>
        CodeGenerationType GetTypeForAttribute(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services);

		/// <summary>
		/// Returns the type that gets generated for the SdkMessagePair
		/// </summary>
        CodeGenerationType GetTypeForMessagePair(SdkMessagePair messagePair, IServiceProvider services);

		/// <summary>
		/// Returns the type that gets generated for the SdkMessageRequestField
		/// </summary>
        CodeGenerationType GetTypeForRequestField(SdkMessageRequest request, SdkMessageRequestField requestField, IServiceProvider services);

		/// <summary>
		/// Returns the type that gets generated for the SdkMessageResponseField
		/// </summary>
        CodeGenerationType GetTypeForResponseField(SdkMessageResponse response, SdkMessageResponseField responseField, IServiceProvider services);
	}
}
