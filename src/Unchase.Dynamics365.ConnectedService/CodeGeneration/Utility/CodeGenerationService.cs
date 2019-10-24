using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class CodeGenerationService : ICodeGenerationService
	{
        async Task ICodeGenerationService.WriteAsync(IOrganizationMetadata organizationMetadata, string language,
            string outputFile, string outputNamespace, IServiceProvider services)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var serviceProvider = services as ServiceProvider;
			var codeNameSpace = await CodeGenerationService.BuildCodeDomAsync(organizationMetadata, outputNamespace, serviceProvider);
			await CodeGenerationService.WriteFileAsync(outputFile, language, codeNameSpace, serviceProvider);
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
		}

		CodeGenerationType ICodeGenerationService.GetTypeForOptionSet(EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
		{
			return CodeGenerationType.Enum;
		}

		CodeGenerationType ICodeGenerationService.GetTypeForOption(OptionSetMetadataBase optionSetMetadata, OptionMetadata optionMetadata, IServiceProvider services)
		{
			return CodeGenerationType.Field;
		}

		CodeGenerationType ICodeGenerationService.GetTypeForEntity(EntityMetadata entityMetadata, IServiceProvider services)
		{
			return CodeGenerationType.Class;
		}

		CodeGenerationType ICodeGenerationService.GetTypeForAttribute(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
		{
			return CodeGenerationType.Property;
		}

		CodeGenerationType ICodeGenerationService.GetTypeForMessagePair(SdkMessagePair messagePair, IServiceProvider services)
		{
			return CodeGenerationType.Class;
		}

		CodeGenerationType ICodeGenerationService.GetTypeForRequestField(SdkMessageRequest request, SdkMessageRequestField requestField, IServiceProvider services)
		{
			return CodeGenerationType.Property;
		}

		CodeGenerationType ICodeGenerationService.GetTypeForResponseField(SdkMessageResponse response, SdkMessageResponseField responseField, IServiceProvider services)
		{
			return CodeGenerationType.Property;
		}

		private static async Task<CodeNamespace> BuildCodeDomAsync(IOrganizationMetadata organizationMetadata, string outputNamespace, ServiceProvider serviceProvider)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeNamespace = CodeGenerationService.Namespace(outputNamespace);
			codeNamespace.Types.AddRange(await CodeGenerationService.BuildOptionSetsAsync(organizationMetadata.OptionSets, serviceProvider));
			codeNamespace.Types.AddRange(await CodeGenerationService.BuildEntitiesAsync(organizationMetadata.Entities, serviceProvider));
			codeNamespace.Types.AddRange(await CodeGenerationService.BuildServiceContextAsync(organizationMetadata.Entities, serviceProvider));
			codeNamespace.Types.AddRange(await CodeGenerationService.BuildMessagesAsync(organizationMetadata.Messages, serviceProvider));
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
			return codeNamespace;
		}

		private static async Task WriteFileAsync(string outputFile, string language, CodeNamespace codeNameSpace, ServiceProvider serviceProvider)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeCompileUnit = new CodeCompileUnit();
			codeCompileUnit.Namespaces.Add(codeNameSpace);
			codeCompileUnit.AssemblyCustomAttributes.Add(CodeGenerationService.Attribute(typeof(ProxyTypesAssemblyAttribute)));
			serviceProvider.CodeCustomizationService.CustomizeCodeDom(codeCompileUnit, serviceProvider);
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                BlankLinesBetweenMembers = true, BracingStyle = "C", IndentString = "\t", VerbatimOrder = true
            };
            using (var streamWriter = new StreamWriter(outputFile))
			{
				using (var codeDomProvider = CodeDomProvider.CreateProvider(language))
				{
					codeDomProvider.GenerateCodeFromCompileUnit(codeCompileUnit, streamWriter, codeGeneratorOptions);
				}
			}
			await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Exit {0}: Code file written to {1}", MethodBase.GetCurrentMethod().Name, outputFile);
        }

		private static async Task<CodeTypeDeclarationCollection> BuildOptionSetsAsync(OptionSetMetadataBase[] optionSetMetadata, ServiceProvider serviceProvider)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclarationCollection = new CodeTypeDeclarationCollection();
			foreach (var optionSetMetadataBase in optionSetMetadata)
			{
				if (serviceProvider.CodeFilterService.GenerateOptionSet(optionSetMetadataBase, serviceProvider) && optionSetMetadataBase.IsGlobal != null && optionSetMetadataBase.IsGlobal.Value)
				{
					var codeTypeDeclaration = await CodeGenerationService.BuildOptionSetAsync(null, optionSetMetadataBase, serviceProvider);
					if (codeTypeDeclaration != null)
					{
						codeTypeDeclarationCollection.Add(codeTypeDeclaration);
					}
					else
					{
                        await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping OptionSet {0} of type {1} from being generated.", optionSetMetadataBase.Name, optionSetMetadataBase.GetType());
					}
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping OptionSet {0} from being generated.", optionSetMetadataBase.Name);
				}
			}
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
			return codeTypeDeclarationCollection;
		}

		private static async Task<CodeTypeDeclaration> BuildOptionSetAsync(EntityMetadata entity, OptionSetMetadataBase optionSet, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclaration = CodeGenerationService.Enum(serviceProvider.NamingService.GetNameForOptionSet(entity, optionSet, serviceProvider), CodeGenerationService.Attribute(typeof(DataContractAttribute)));
            if (!(optionSet is OptionSetMetadata optionSetMetadata))
			{
				return null;
			}
			foreach (var optionMetadata in optionSetMetadata.Options)
			{
				if (serviceProvider.CodeFilterService.GenerateOption(optionMetadata, serviceProvider))
				{
					codeTypeDeclaration.Members.Add(await CodeGenerationService.BuildOptionAsync(optionSet, optionMetadata, serviceProvider));
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.Option {1} from being generated.", optionSet.Name, optionMetadata.Value.Value);
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: OptionSet Enumeration {1} defined", MethodBase.GetCurrentMethod().Name, codeTypeDeclaration.Name);
			return codeTypeDeclaration;
		}

		private static async Task<CodeTypeMember> BuildOptionAsync(OptionSetMetadataBase optionSet, OptionMetadata option, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeMemberField = CodeGenerationService.Field(serviceProvider.NamingService.GetNameForOption(optionSet, option, serviceProvider), typeof(int), option.Value.Value, CodeGenerationService.Attribute(typeof(EnumMemberAttribute)));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.Option {2} defined", MethodBase.GetCurrentMethod().Name, optionSet.Name, codeMemberField.Name);
			return codeMemberField;
		}

		private static async Task<CodeTypeDeclarationCollection> BuildEntitiesAsync(EntityMetadata[] entityMetadata, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclarationCollection = new CodeTypeDeclarationCollection();
			foreach (var entityMetadata2 in from metadata in entityMetadata
			orderby metadata.LogicalName
			select metadata)
			{
				if (await serviceProvider.CodeFilterService.GenerateEntityAsync(entityMetadata2, serviceProvider))
				{
					codeTypeDeclarationCollection.AddRange(await CodeGenerationService.BuildEntityAsync(entityMetadata2, serviceProvider));
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping Entity {0} from being generated.", entityMetadata2.LogicalName);
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
			return codeTypeDeclarationCollection;
		}

		private static async Task<CodeTypeDeclarationCollection> BuildEntityAsync(EntityMetadata entity, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclarationCollection = new CodeTypeDeclarationCollection();
			var codeTypeDeclaration = CodeGenerationService.Class(serviceProvider.NamingService.GetNameForEntity(entity, serviceProvider), CodeGenerationService.TypeRef(CodeGenerationService.EntityClassBaseType), CodeGenerationService.Attribute(typeof(DataContractAttribute)), CodeGenerationService.Attribute(CodeGenerationService.EntityLogicalNameAttribute, CodeGenerationService.AttributeArg(entity.LogicalName)));
			CodeGenerationService.InitializeEntityClass(codeTypeDeclaration, entity);
			CodeTypeMember codeTypeMember = null;
			foreach (var attributeMetadata in from metadata in entity.Attributes
			orderby metadata.LogicalName
			select metadata)
			{
				if (serviceProvider.CodeFilterService.GenerateAttribute(attributeMetadata, serviceProvider))
				{
					codeTypeMember = await CodeGenerationService.BuildAttributeAsync(entity, attributeMetadata, serviceProvider);
					codeTypeDeclaration.Members.Add(codeTypeMember);
					if (entity.PrimaryIdAttribute == attributeMetadata.LogicalName && attributeMetadata.IsPrimaryId.GetValueOrDefault())
					{
						codeTypeDeclaration.Members.Add(await CodeGenerationService.BuildIdPropertyAsync(entity, attributeMetadata, serviceProvider));
					}
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.Attribute {1} from being generated.", entity.LogicalName, attributeMetadata.LogicalName);
				}
				var codeTypeDeclaration2 = await CodeGenerationService.BuildAttributeOptionSetAsync(entity, attributeMetadata, codeTypeMember, serviceProvider);
				if (codeTypeDeclaration2 != null)
				{
					codeTypeDeclarationCollection.Add(codeTypeDeclaration2);
				}
			}
			codeTypeDeclaration.Members.AddRange(await CodeGenerationService.BuildOneToManyRelationshipsAsync(entity, serviceProvider));
			codeTypeDeclaration.Members.AddRange(await CodeGenerationService.BuildManyToManyRelationshipsAsync(entity, serviceProvider));
			codeTypeDeclaration.Members.AddRange(await CodeGenerationService.BuildManyToOneRelationshipsAsync(entity, serviceProvider));
			codeTypeDeclarationCollection.Add(codeTypeDeclaration);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: Entity Class {1} defined", MethodBase.GetCurrentMethod().Name, codeTypeDeclaration.Name);
			return codeTypeDeclarationCollection;
		}

		private static void InitializeEntityClass(CodeTypeDeclaration entityClass, EntityMetadata entity)
		{
			entityClass.BaseTypes.Add(CodeGenerationService.TypeRef(typeof(INotifyPropertyChanging)));
			entityClass.BaseTypes.Add(CodeGenerationService.TypeRef(typeof(INotifyPropertyChanged)));
			entityClass.Members.Add(CodeGenerationService.EntityConstructor());
			entityClass.Members.Add(CodeGenerationService.EntityLogicalNameConstant(entity));
			entityClass.Members.Add(CodeGenerationService.EntityTypeCodeConstant(entity));
			entityClass.Members.Add(CodeGenerationService.Event("PropertyChanged", typeof(PropertyChangedEventHandler), typeof(INotifyPropertyChanged)));
			entityClass.Members.Add(CodeGenerationService.Event("PropertyChanging", typeof(PropertyChangingEventHandler), typeof(INotifyPropertyChanging)));
			entityClass.Members.Add(CodeGenerationService.RaiseEvent("OnPropertyChanged", "PropertyChanged", typeof(PropertyChangedEventArgs)));
			entityClass.Members.Add(CodeGenerationService.RaiseEvent("OnPropertyChanging", "PropertyChanging", typeof(PropertyChangingEventArgs)));
			entityClass.Comments.AddRange(CodeGenerationService.CommentSummary(entity.Description));
		}

		private static CodeTypeMember EntityLogicalNameConstant(EntityMetadata entity)
		{
			var codeMemberField = CodeGenerationService.Field("EntityLogicalName", typeof(string), entity.LogicalName, Array.Empty<CodeAttributeDeclaration>());
			codeMemberField.Attributes = (MemberAttributes)24581;
			return codeMemberField;
		}

		private static CodeTypeMember EntityTypeCodeConstant(EntityMetadata entity)
		{
			var codeMemberField = CodeGenerationService.Field("EntityTypeCode", typeof(int), entity.ObjectTypeCode.GetValueOrDefault(), Array.Empty<CodeAttributeDeclaration>());
			codeMemberField.Attributes = (MemberAttributes)24581;
			return codeMemberField;
		}

		private static CodeTypeMember EntityConstructor()
		{
			var codeConstructor = CodeGenerationService.Constructor(Array.Empty<CodeExpression>());
			codeConstructor.BaseConstructorArgs.Add(CodeGenerationService.VarRef("EntityLogicalName"));
			codeConstructor.Comments.AddRange(CodeGenerationService.CommentSummary("Default Constructor."));
			return codeConstructor;
		}

		private static async Task<CodeTypeMember> BuildAttributeAsync(EntityMetadata entity, AttributeMetadata attribute, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var typeForAttributeType = await serviceProvider.TypeMappingService.GetTypeForAttributeTypeAsync(entity, attribute, serviceProvider);
			var codeMemberProperty = CodeGenerationService.PropertyGet(typeForAttributeType, serviceProvider.NamingService.GetNameForAttribute(entity, attribute, serviceProvider), Array.Empty<CodeStatement>());
			codeMemberProperty.HasSet = (attribute.IsValidForCreate.GetValueOrDefault() || attribute.IsValidForUpdate.GetValueOrDefault());
			codeMemberProperty.HasGet = (attribute.IsValidForRead.GetValueOrDefault() || codeMemberProperty.HasSet);
			if (codeMemberProperty.HasGet)
			{
				codeMemberProperty.GetStatements.AddRange(CodeGenerationService.BuildAttributeGet(attribute, typeForAttributeType));
			}
			if (codeMemberProperty.HasSet)
			{
				codeMemberProperty.SetStatements.AddRange(CodeGenerationService.BuildAttributeSet(entity, attribute, codeMemberProperty.Name));
			}
			codeMemberProperty.CustomAttributes.Add(CodeGenerationService.Attribute(CodeGenerationService.AttributeLogicalNameAttribute, CodeGenerationService.AttributeArg(attribute.LogicalName)));
			if (attribute.DeprecatedVersion != null)
			{
				codeMemberProperty.CustomAttributes.Add(CodeGenerationService.Attribute(CodeGenerationService.ObsoleteFieldAttribute));
			}
			codeMemberProperty.Comments.AddRange(CodeGenerationService.CommentSummary(attribute.Description));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.Attribute {2} defined", MethodBase.GetCurrentMethod().Name, entity.LogicalName, codeMemberProperty.Name);
			return codeMemberProperty;
		}

		private static CodeStatementCollection BuildAttributeGet(AttributeMetadata attribute, CodeTypeReference targetType)
		{
			var codeStatementCollection = new CodeStatementCollection();
			if (attribute.AttributeType.GetValueOrDefault() == AttributeTypeCode.PartyList && targetType.TypeArguments.Count > 0)
			{
				codeStatementCollection.AddRange(CodeGenerationService.BuildEntityCollectionAttributeGet(attribute.LogicalName, targetType));
			}
			else
			{
				codeStatementCollection.Add(CodeGenerationService.Return(CodeGenerationService.ThisMethodInvoke("GetAttributeValue", targetType, CodeGenerationService.StringLiteral(attribute.LogicalName))));
			}
			return codeStatementCollection;
		}

		private static CodeStatementCollection BuildAttributeSet(EntityMetadata entity, AttributeMetadata attribute, string propertyName)
		{
            var codeStatementCollection = new CodeStatementCollection
            {
                CodeGenerationService.ThisMethodInvoke("OnPropertyChanging",
                    CodeGenerationService.StringLiteral(propertyName))
            };
            if (attribute.AttributeType.GetValueOrDefault() == AttributeTypeCode.PartyList)
			{
				codeStatementCollection.Add(CodeGenerationService.BuildEntityCollectionAttributeSet(attribute.LogicalName));
			}
			else
			{
				codeStatementCollection.Add(CodeGenerationService.ThisMethodInvoke("SetAttributeValue", CodeGenerationService.StringLiteral(attribute.LogicalName), CodeGenerationService.VarRef("value")));
			}
			if (entity.PrimaryIdAttribute == attribute.LogicalName && attribute.IsPrimaryId.GetValueOrDefault())
			{
				codeStatementCollection.Add(CodeGenerationService.If(CodeGenerationService.PropRef(CodeGenerationService.VarRef("value"), "HasValue"), CodeGenerationService.AssignValue(CodeGenerationService.BaseProp("Id"), CodeGenerationService.PropRef(CodeGenerationService.VarRef("value"), "Value")), CodeGenerationService.AssignValue(CodeGenerationService.BaseProp("Id"), CodeGenerationService.GuidEmpty())));
			}
			codeStatementCollection.Add(CodeGenerationService.ThisMethodInvoke("OnPropertyChanged", CodeGenerationService.StringLiteral(propertyName)));
			return codeStatementCollection;
		}

		private static CodeStatementCollection BuildEntityCollectionAttributeGet(string attributeLogicalName, CodeTypeReference propertyType)
		{
			return new CodeStatementCollection
			{
				CodeGenerationService.Var(typeof(EntityCollection), "collection", CodeGenerationService.ThisMethodInvoke("GetAttributeValue", CodeGenerationService.TypeRef(typeof(EntityCollection)), CodeGenerationService.StringLiteral(attributeLogicalName))),
				CodeGenerationService.If(CodeGenerationService.And(CodeGenerationService.NotNull(CodeGenerationService.VarRef("collection")), CodeGenerationService.NotNull(CodeGenerationService.PropRef(CodeGenerationService.VarRef("collection"), "Entities"))), CodeGenerationService.Return(CodeGenerationService.StaticMethodInvoke(typeof(Enumerable), "Cast", propertyType.TypeArguments[0], CodeGenerationService.PropRef(CodeGenerationService.VarRef("collection"), "Entities"))), CodeGenerationService.Return(CodeGenerationService.Null()))
			};
		}

		private static CodeStatement BuildEntityCollectionAttributeSet(string attributeLogicalName)
		{
			return CodeGenerationService.If(CodeGenerationService.ValueNull(), CodeGenerationService.ThisMethodInvoke("SetAttributeValue", CodeGenerationService.StringLiteral(attributeLogicalName), CodeGenerationService.VarRef("value")), CodeGenerationService.ThisMethodInvoke("SetAttributeValue", CodeGenerationService.StringLiteral(attributeLogicalName), CodeGenerationService.New(CodeGenerationService.TypeRef(typeof(EntityCollection)), CodeGenerationService.New(CodeGenerationService.TypeRef(typeof(List<Entity>)), CodeGenerationService.VarRef("value")))));
		}

		private static async Task<CodeTypeMember> BuildIdPropertyAsync(EntityMetadata entity, AttributeMetadata attribute, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeMemberProperty = CodeGenerationService.PropertyGet(CodeGenerationService.TypeRef(typeof(Guid)), "Id", Array.Empty<CodeStatement>());
			codeMemberProperty.CustomAttributes.Add(CodeGenerationService.Attribute(CodeGenerationService.AttributeLogicalNameAttribute, CodeGenerationService.AttributeArg(attribute.LogicalName)));
			codeMemberProperty.Attributes = (MemberAttributes)24580;
			codeMemberProperty.HasSet = (attribute.IsValidForCreate.GetValueOrDefault() || attribute.IsValidForUpdate.GetValueOrDefault());
			codeMemberProperty.HasGet = (attribute.IsValidForRead.GetValueOrDefault() || codeMemberProperty.HasSet);
			codeMemberProperty.GetStatements.Add(CodeGenerationService.Return(CodeGenerationService.BaseProp("Id")));
			if (codeMemberProperty.HasSet)
			{
				codeMemberProperty.SetStatements.Add(CodeGenerationService.AssignValue(CodeGenerationService.ThisProp(serviceProvider.NamingService.GetNameForAttribute(entity, attribute, serviceProvider)), CodeGenerationService.VarRef("value")));
			}
			else
			{
				codeMemberProperty.SetStatements.Add(CodeGenerationService.AssignValue(CodeGenerationService.BaseProp("Id"), CodeGenerationService.VarRef("value")));
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.Attribute Id defined", MethodBase.GetCurrentMethod().Name, entity.LogicalName);
			return codeMemberProperty;
		}

		private static async Task<CodeTypeDeclaration> BuildAttributeOptionSetAsync(EntityMetadata entity, AttributeMetadata attribute, CodeTypeMember attributeMember, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var attributeOptionSet = TypeMappingService.GetAttributeOptionSet(attribute);
			if (attributeOptionSet == null || !serviceProvider.CodeFilterService.GenerateOptionSet(attributeOptionSet, serviceProvider))
			{
				if (attributeOptionSet != null)
				{
					await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: No type created for {1}", MethodBase.GetCurrentMethod().Name, attributeOptionSet.Name);
				}
				return null;
			}
			var codeTypeDeclaration = await CodeGenerationService.BuildOptionSetAsync(entity, attributeOptionSet, serviceProvider);
			if (codeTypeDeclaration == null)
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: No type created for {1} of type {2}", MethodBase.GetCurrentMethod().Name, attributeOptionSet.Name, attributeOptionSet.GetType());
				return null;
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: Type {1} created for {2}", MethodBase.GetCurrentMethod().Name, codeTypeDeclaration.Name, attributeOptionSet.Name);
			CodeGenerationService.UpdateAttributeMemberStatements(attribute, attributeMember);
			return codeTypeDeclaration;
		}

		private static void UpdateAttributeMemberStatements(AttributeMetadata attribute, CodeTypeMember attributeMember)
		{
			var codeMemberProperty = attributeMember as CodeMemberProperty;
			if (codeMemberProperty != null && codeMemberProperty.HasGet)
			{
				codeMemberProperty.GetStatements.Clear();
				codeMemberProperty.GetStatements.AddRange(CodeGenerationService.BuildOptionSetAttributeGet(attribute, codeMemberProperty.Type));
			}
			if (codeMemberProperty != null && codeMemberProperty.HasSet)
			{
				codeMemberProperty.SetStatements.Clear();
				codeMemberProperty.SetStatements.AddRange(CodeGenerationService.BuildOptionSetAttributeSet(attribute, codeMemberProperty.Name));
			}
		}

		private static CodeStatementCollection BuildOptionSetAttributeGet(AttributeMetadata attribute, CodeTypeReference attributeType)
		{
			var codeTypeReference = attributeType;
			if (codeTypeReference.TypeArguments.Count > 0)
			{
				codeTypeReference = codeTypeReference.TypeArguments[0];
			}
			return new CodeStatementCollection(new CodeStatement[]
			{
				CodeGenerationService.Var(typeof(OptionSetValue), "optionSet", CodeGenerationService.ThisMethodInvoke("GetAttributeValue", CodeGenerationService.TypeRef(typeof(OptionSetValue)), CodeGenerationService.StringLiteral(attribute.LogicalName))),
				CodeGenerationService.If(CodeGenerationService.NotNull(CodeGenerationService.VarRef("optionSet")), CodeGenerationService.Return(CodeGenerationService.Cast(codeTypeReference, CodeGenerationService.ConvertEnum(codeTypeReference, "optionSet"))), CodeGenerationService.Return(CodeGenerationService.Null()))
			});
		}

		private static CodeStatementCollection BuildOptionSetAttributeSet(AttributeMetadata attribute, string propertyName)
		{
			return new CodeStatementCollection
			{
				CodeGenerationService.ThisMethodInvoke("OnPropertyChanging", CodeGenerationService.StringLiteral(propertyName)),
				CodeGenerationService.If(CodeGenerationService.ValueNull(), CodeGenerationService.ThisMethodInvoke("SetAttributeValue", CodeGenerationService.StringLiteral(attribute.LogicalName), CodeGenerationService.Null()), CodeGenerationService.ThisMethodInvoke("SetAttributeValue", CodeGenerationService.StringLiteral(attribute.LogicalName), CodeGenerationService.New(CodeGenerationService.TypeRef(typeof(OptionSetValue)), CodeGenerationService.Cast(CodeGenerationService.TypeRef(typeof(int)), CodeGenerationService.VarRef("value"))))),
				CodeGenerationService.ThisMethodInvoke("OnPropertyChanged", CodeGenerationService.StringLiteral(propertyName))
			};
		}

		private static async Task<CodeTypeMember> BuildCalendarRuleAttributeAsync(EntityMetadata entity, EntityMetadata otherEntity, OneToManyRelationshipMetadata oneToMany, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeMemberProperty = CodeGenerationService.PropertyGet(CodeGenerationService.IEnumerable(serviceProvider.TypeMappingService.GetTypeForRelationship(oneToMany, otherEntity, serviceProvider)), "CalendarRules", Array.Empty<CodeStatement>());
			codeMemberProperty.GetStatements.AddRange(CodeGenerationService.BuildEntityCollectionAttributeGet("calendarrules", codeMemberProperty.Type));
			codeMemberProperty.SetStatements.Add(CodeGenerationService.ThisMethodInvoke("OnPropertyChanging", CodeGenerationService.StringLiteral(codeMemberProperty.Name)));
			codeMemberProperty.SetStatements.Add(CodeGenerationService.BuildEntityCollectionAttributeSet("calendarrules"));
			codeMemberProperty.SetStatements.Add(CodeGenerationService.ThisMethodInvoke("OnPropertyChanged", CodeGenerationService.StringLiteral(codeMemberProperty.Name)));
			codeMemberProperty.CustomAttributes.Add(CodeGenerationService.Attribute(CodeGenerationService.AttributeLogicalNameAttribute, CodeGenerationService.AttributeArg("calendarrules")));
			codeMemberProperty.Comments.AddRange(CodeGenerationService.CommentSummary("1:N " + oneToMany.SchemaName));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.Attribute {2} defined", MethodBase.GetCurrentMethod().Name, entity.LogicalName, codeMemberProperty.Name);
			return codeMemberProperty;
		}

		private static async Task<CodeTypeMemberCollection> BuildOneToManyRelationshipsAsync(EntityMetadata entity, ServiceProvider serviceProvider)
		{
			var codeTypeMemberCollection = new CodeTypeMemberCollection();
			if (entity.OneToManyRelationships == null)
			{
				return codeTypeMemberCollection;
			}
			foreach (var oneToManyRelationshipMetadata in from metadata in entity.OneToManyRelationships
			orderby metadata.SchemaName
			select metadata)
			{
				var entityMetadata = await CodeGenerationService.GetEntityMetadataAsync(oneToManyRelationshipMetadata.ReferencingEntity, serviceProvider);
				if (entityMetadata == null)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.OneToMany {1} from being generated. Correlating entity not returned.", entity.LogicalName, oneToManyRelationshipMetadata.SchemaName);
				}
				else if (string.Equals(oneToManyRelationshipMetadata.SchemaName, "calendar_calendar_rules", StringComparison.Ordinal) || string.Equals(oneToManyRelationshipMetadata.SchemaName, "service_calendar_rules", StringComparison.Ordinal))
				{
					codeTypeMemberCollection.Add(await CodeGenerationService.BuildCalendarRuleAttributeAsync(entity, entityMetadata, oneToManyRelationshipMetadata, serviceProvider));
				}
				else if (await serviceProvider.CodeFilterService.GenerateEntityAsync(entityMetadata, serviceProvider) && await serviceProvider.CodeFilterService.GenerateRelationshipAsync(oneToManyRelationshipMetadata, entityMetadata, serviceProvider))
				{
					codeTypeMemberCollection.Add(await CodeGenerationService.BuildOneToManyAsync(entity, entityMetadata, oneToManyRelationshipMetadata, serviceProvider));
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.OneToMany {1} from being generated.", entity.LogicalName, oneToManyRelationshipMetadata.SchemaName);
				}
			}
			return codeTypeMemberCollection;
		}

		private static async Task<CodeTypeMember> BuildOneToManyAsync(EntityMetadata entity, EntityMetadata otherEntity, OneToManyRelationshipMetadata oneToMany, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var typeForRelationship = serviceProvider.TypeMappingService.GetTypeForRelationship(oneToMany, otherEntity, serviceProvider);
			var entityRole = (oneToMany.ReferencingEntity == entity.LogicalName) ? new EntityRole?(EntityRole.Referenced) : null;
			var codeMemberProperty = CodeGenerationService.PropertyGet(CodeGenerationService.IEnumerable(typeForRelationship), serviceProvider.NamingService.GetNameForRelationship(entity, oneToMany, entityRole, serviceProvider), Array.Empty<CodeStatement>());
			codeMemberProperty.GetStatements.Add(CodeGenerationService.BuildRelationshipGet("GetRelatedEntities", oneToMany, typeForRelationship, entityRole));
			codeMemberProperty.SetStatements.AddRange(CodeGenerationService.BuildRelationshipSet("SetRelatedEntities", oneToMany, typeForRelationship, codeMemberProperty.Name, entityRole));
			codeMemberProperty.CustomAttributes.Add(CodeGenerationService.BuildRelationshipSchemaNameAttribute(oneToMany.SchemaName, entityRole));
			codeMemberProperty.Comments.AddRange(CodeGenerationService.CommentSummary("1:N " + oneToMany.SchemaName));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.OneToMany {2} defined", MethodBase.GetCurrentMethod().Name, entity.LogicalName, codeMemberProperty.Name);
			return codeMemberProperty;
		}

		private static async Task<CodeTypeMemberCollection> BuildManyToManyRelationshipsAsync(EntityMetadata entity, ServiceProvider serviceProvider)
		{
			var codeTypeMemberCollection = new CodeTypeMemberCollection();
			if (entity.ManyToManyRelationships == null)
			{
				return codeTypeMemberCollection;
			}
			foreach (var manyToManyRelationshipMetadata in from metadata in entity.ManyToManyRelationships
			orderby metadata.SchemaName
			select metadata)
			{
				var entityMetadata = await CodeGenerationService.GetEntityMetadataAsync((entity.LogicalName != manyToManyRelationshipMetadata.Entity1LogicalName) ? manyToManyRelationshipMetadata.Entity1LogicalName : manyToManyRelationshipMetadata.Entity2LogicalName, serviceProvider);
				if (entityMetadata == null)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.ManyToMany {1} from being generated. Correlating entity not returned.", entity.LogicalName, manyToManyRelationshipMetadata.SchemaName);
				}
				else if (await serviceProvider.CodeFilterService.GenerateEntityAsync(entityMetadata, serviceProvider) && await serviceProvider.CodeFilterService.GenerateRelationshipAsync(manyToManyRelationshipMetadata, entityMetadata, serviceProvider))
				{
					if (entityMetadata.LogicalName != entity.LogicalName)
					{
						var nameForRelationship = serviceProvider.NamingService.GetNameForRelationship(entity, manyToManyRelationshipMetadata, null, serviceProvider);
						var value = await CodeGenerationService.BuildManyToManyAsync(entity, entityMetadata, manyToManyRelationshipMetadata, nameForRelationship, null, serviceProvider);
						codeTypeMemberCollection.Add(value);
					}
					else
					{
						var nameForRelationship2 = serviceProvider.NamingService.GetNameForRelationship(entity, manyToManyRelationshipMetadata, EntityRole.Referencing, serviceProvider);
						var value2 = await CodeGenerationService.BuildManyToManyAsync(entity, entityMetadata, manyToManyRelationshipMetadata, nameForRelationship2, EntityRole.Referencing, serviceProvider);
						codeTypeMemberCollection.Add(value2);
						var nameForRelationship3 = serviceProvider.NamingService.GetNameForRelationship(entity, manyToManyRelationshipMetadata, EntityRole.Referenced, serviceProvider);
						var value3 = await CodeGenerationService.BuildManyToManyAsync(entity, entityMetadata, manyToManyRelationshipMetadata, nameForRelationship3, EntityRole.Referenced, serviceProvider);
						codeTypeMemberCollection.Add(value3);
					}
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.ManyToMany {1} from being generated.", entity.LogicalName, manyToManyRelationshipMetadata.SchemaName);
				}
			}
			return codeTypeMemberCollection;
		}

		private static async Task<CodeTypeMember> BuildManyToManyAsync(EntityMetadata entity, EntityMetadata otherEntity, ManyToManyRelationshipMetadata manyToMany, string propertyName, EntityRole? entityRole, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var typeForRelationship = serviceProvider.TypeMappingService.GetTypeForRelationship(manyToMany, otherEntity, serviceProvider);
			var codeMemberProperty = CodeGenerationService.PropertyGet(CodeGenerationService.IEnumerable(typeForRelationship), propertyName, Array.Empty<CodeStatement>());
			codeMemberProperty.GetStatements.Add(CodeGenerationService.BuildRelationshipGet("GetRelatedEntities", manyToMany, typeForRelationship, entityRole));
			codeMemberProperty.SetStatements.AddRange(CodeGenerationService.BuildRelationshipSet("SetRelatedEntities", manyToMany, typeForRelationship, propertyName, entityRole));
			codeMemberProperty.CustomAttributes.Add(CodeGenerationService.BuildRelationshipSchemaNameAttribute(manyToMany.SchemaName, entityRole));
			codeMemberProperty.Comments.AddRange(CodeGenerationService.CommentSummary("N:N " + manyToMany.SchemaName));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.ManyToMany {2} defined", MethodBase.GetCurrentMethod().Name, entity.LogicalName, propertyName);
			return codeMemberProperty;
		}

		private static async Task<CodeTypeMemberCollection> BuildManyToOneRelationshipsAsync(EntityMetadata entity, ServiceProvider serviceProvider)
		{
			var codeTypeMemberCollection = new CodeTypeMemberCollection();
			if (entity.ManyToOneRelationships == null)
			{
				return codeTypeMemberCollection;
			}
			foreach (var oneToManyRelationshipMetadata in from metadata in entity.ManyToOneRelationships
			orderby metadata.SchemaName
			select metadata)
			{
				var entityMetadata = await CodeGenerationService.GetEntityMetadataAsync(oneToManyRelationshipMetadata.ReferencedEntity, serviceProvider);
				if (entityMetadata == null)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.ManyToOne {1} from being generated. Correlating entity not returned.", entity.LogicalName, oneToManyRelationshipMetadata.SchemaName);
				}
				else if (await serviceProvider.CodeFilterService.GenerateEntityAsync(entityMetadata, serviceProvider) && await serviceProvider.CodeFilterService.GenerateRelationshipAsync(oneToManyRelationshipMetadata, entityMetadata, serviceProvider))
				{
					var codeTypeMember = await CodeGenerationService.BuildManyToOneAsync(entity, entityMetadata, oneToManyRelationshipMetadata, serviceProvider);
					if (codeTypeMember != null)
					{
						codeTypeMemberCollection.Add(codeTypeMember);
					}
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.ManyToOne {1} from being generated.", entity.LogicalName, oneToManyRelationshipMetadata.SchemaName);
				}
			}
			return codeTypeMemberCollection;
		}

		private static async Task<CodeTypeMember> BuildManyToOneAsync(EntityMetadata entity, EntityMetadata otherEntity, OneToManyRelationshipMetadata manyToOne, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var typeForRelationship = serviceProvider.TypeMappingService.GetTypeForRelationship(manyToOne, otherEntity, serviceProvider);
			var entityRole = (otherEntity.LogicalName == entity.LogicalName) ? new EntityRole?(EntityRole.Referencing) : null;
			var codeMemberProperty = CodeGenerationService.PropertyGet(typeForRelationship, serviceProvider.NamingService.GetNameForRelationship(entity, manyToOne, entityRole, serviceProvider), Array.Empty<CodeStatement>());
			codeMemberProperty.GetStatements.Add(CodeGenerationService.BuildRelationshipGet("GetRelatedEntity", manyToOne, typeForRelationship, entityRole));
			var attributeMetadata = entity.Attributes.SingleOrDefault(attribute => attribute.LogicalName == manyToOne.ReferencingAttribute);
			if (attributeMetadata == null)
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.ManyToOne {2} not generated since referencing attribute is not generated.", MethodBase.GetCurrentMethod().Name, entity.LogicalName, manyToOne.SchemaName);
				return null;
			}
			if (attributeMetadata.IsValidForCreate.GetValueOrDefault() || attributeMetadata.IsValidForUpdate.GetValueOrDefault())
			{
				codeMemberProperty.SetStatements.AddRange(CodeGenerationService.BuildRelationshipSet("SetRelatedEntity", manyToOne, typeForRelationship, codeMemberProperty.Name, entityRole));
			}
			codeMemberProperty.CustomAttributes.Add(CodeGenerationService.Attribute(CodeGenerationService.AttributeLogicalNameAttribute, CodeGenerationService.AttributeArg(manyToOne.ReferencingAttribute)));
			codeMemberProperty.CustomAttributes.Add(CodeGenerationService.BuildRelationshipSchemaNameAttribute(manyToOne.SchemaName, entityRole));
			codeMemberProperty.Comments.AddRange(CodeGenerationService.CommentSummary("N:1 " + manyToOne.SchemaName));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.ManyToOne {2} defined", MethodBase.GetCurrentMethod().Name, entity.LogicalName, codeMemberProperty.Name);
			return codeMemberProperty;
		}

		private static CodeStatement BuildRelationshipGet(string methodName, RelationshipMetadataBase relationship, CodeTypeReference targetType, EntityRole? entityRole)
		{
			var codeExpression = (entityRole != null) ? (CodeGenerationService.FieldRef(typeof(EntityRole), entityRole.ToString())) as CodeExpression : CodeGenerationService.Null();
			return CodeGenerationService.Return(CodeGenerationService.ThisMethodInvoke(methodName, targetType, CodeGenerationService.StringLiteral(relationship.SchemaName), codeExpression));
		}

		private static CodeStatementCollection BuildRelationshipSet(string methodName, RelationshipMetadataBase relationship, CodeTypeReference targetType, string propertyName, EntityRole? entityRole)
		{
			var codeStatementCollection = new CodeStatementCollection();
			var codeExpression = (entityRole != null) ? CodeGenerationService.FieldRef(typeof(EntityRole), entityRole.ToString()) as CodeExpression : CodeGenerationService.Null();
			codeStatementCollection.Add(CodeGenerationService.ThisMethodInvoke("OnPropertyChanging", CodeGenerationService.StringLiteral(propertyName)));
			codeStatementCollection.Add(CodeGenerationService.ThisMethodInvoke(methodName, targetType, CodeGenerationService.StringLiteral(relationship.SchemaName), codeExpression, CodeGenerationService.VarRef("value")));
			codeStatementCollection.Add(CodeGenerationService.ThisMethodInvoke("OnPropertyChanged", CodeGenerationService.StringLiteral(propertyName)));
			return codeStatementCollection;
		}

		private static CodeAttributeDeclaration BuildRelationshipSchemaNameAttribute(string relationshipSchemaName, EntityRole? entityRole)
		{
			if (entityRole != null)
			{
				return CodeGenerationService.Attribute(CodeGenerationService.RelationshipSchemaNameAttribute, CodeGenerationService.AttributeArg(relationshipSchemaName), CodeGenerationService.AttributeArg(CodeGenerationService.FieldRef(typeof(EntityRole), entityRole.ToString())));
			}
			return CodeGenerationService.Attribute(CodeGenerationService.RelationshipSchemaNameAttribute, CodeGenerationService.AttributeArg(relationshipSchemaName));
		}

		private static async Task<EntityMetadata> GetEntityMetadataAsync(string entityLogicalName, ServiceProvider serviceProvider)
		{
			if (serviceProvider.MetadataProviderService is IMetadataProviderService2 providerService2)
			{
				return (await providerService2.LoadMetadataAsync(serviceProvider)).Entities.SingleOrDefault(e => e.LogicalName == entityLogicalName);
			}
			return (await serviceProvider.MetadataProviderService.LoadMetadataAsync()).Entities.SingleOrDefault(e => e.LogicalName == entityLogicalName);
		}

		private static async Task<CodeTypeDeclarationCollection> BuildServiceContextAsync(EntityMetadata[] entityMetadata, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclarationCollection = new CodeTypeDeclarationCollection();
			if (serviceProvider.CodeFilterService.GenerateServiceContext(serviceProvider))
			{
				var codeTypeDeclaration = CodeGenerationService.Class(serviceProvider.NamingService.GetNameForServiceContext(serviceProvider), CodeGenerationService.ServiceContextBaseType, Array.Empty<CodeAttributeDeclaration>());
				codeTypeDeclaration.Members.Add(CodeGenerationService.ServiceContextConstructor());
				codeTypeDeclaration.Comments.AddRange(CodeGenerationService.CommentSummary("Represents a source of entities bound to a CRM service. It tracks and manages changes made to the retrieved entities."));
				foreach (var entityMetadata2 in from metadata in entityMetadata
				orderby metadata.LogicalName
				select metadata)
				{
					if (await serviceProvider.CodeFilterService.GenerateEntityAsync(entityMetadata2, serviceProvider) && !string.Equals(entityMetadata2.LogicalName, "calendarrule", StringComparison.Ordinal))
					{
						codeTypeDeclaration.Members.Add(await CodeGenerationService.BuildEntitySetAsync(entityMetadata2, serviceProvider));
					}
					else
					{
                        await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0} entity set and AddTo method from being generated.", entityMetadata2.LogicalName);
					}
				}
				codeTypeDeclarationCollection.Add(codeTypeDeclaration);
			}
			else
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping data context from being generated.", Array.Empty<object>());
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
			return codeTypeDeclarationCollection;
		}

		private static CodeTypeMember ServiceContextConstructor()
		{
			var codeConstructor = CodeGenerationService.Constructor(CodeGenerationService.Param(CodeGenerationService.TypeRef(typeof(IOrganizationService)), "service"), Array.Empty<CodeStatement>());
			codeConstructor.BaseConstructorArgs.Add(CodeGenerationService.VarRef("service"));
			codeConstructor.Comments.AddRange(CodeGenerationService.CommentSummary("Constructor."));
			return codeConstructor;
		}

		private static async Task<CodeTypeMember> BuildEntitySetAsync(EntityMetadata entity, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var typeForEntity = serviceProvider.TypeMappingService.GetTypeForEntity(entity, serviceProvider);
			var codeMemberProperty = CodeGenerationService.PropertyGet(CodeGenerationService.IQueryable(typeForEntity), serviceProvider.NamingService.GetNameForEntitySet(entity, serviceProvider), CodeGenerationService.Return(CodeGenerationService.ThisMethodInvoke("CreateQuery", typeForEntity, Array.Empty<CodeExpression>())));
			codeMemberProperty.Comments.AddRange(CodeGenerationService.CommentSummary(string.Format(CultureInfo.InvariantCulture, "Gets a binding to the set of all <see cref=\"{0}\"/> entities.", typeForEntity.BaseType)));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1} entity set '{2}' defined", MethodBase.GetCurrentMethod().Name, entity.LogicalName, codeMemberProperty.Name);
			return codeMemberProperty;
		}

		private static async Task<CodeTypeDeclarationCollection> BuildMessagesAsync(SdkMessages sdkMessages, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclarationCollection = new CodeTypeDeclarationCollection();
			foreach (var sdkMessage in sdkMessages.MessageCollection.Values)
			{
				if (serviceProvider.CodeMessageFilterService.GenerateSdkMessage(sdkMessage, serviceProvider))
				{
					codeTypeDeclarationCollection.AddRange(await CodeGenerationService.BuildMessageAsync(sdkMessage, serviceProvider));
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping SDK Message {0} from being generated.", sdkMessage.Name);
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
			return codeTypeDeclarationCollection;
		}

		private static async Task<CodeTypeDeclarationCollection> BuildMessageAsync(SdkMessage message, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclarationCollection = new CodeTypeDeclarationCollection();
			foreach (var sdkMessagePair in message.SdkMessagePairs.Values)
			{
				if (serviceProvider.CodeMessageFilterService.GenerateSdkMessagePair(sdkMessagePair, serviceProvider))
				{
					codeTypeDeclarationCollection.Add(await CodeGenerationService.BuildMessageRequestAsync(sdkMessagePair, sdkMessagePair.Request, serviceProvider));
					codeTypeDeclarationCollection.Add(await CodeGenerationService.BuildMessageResponseAsync(sdkMessagePair, sdkMessagePair.Response, serviceProvider));
				}
				else
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping {0}.Message Pair from being generated.", message.Name, sdkMessagePair.Request.Name);
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
			return codeTypeDeclarationCollection;
		}

		private static async Task<CodeTypeDeclaration> BuildMessageRequestAsync(SdkMessagePair messagePair, SdkMessageRequest sdkMessageRequest, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclaration = CodeGenerationService.Class(string.Format(CultureInfo.InvariantCulture, "{0}{1}", serviceProvider.NamingService.GetNameForMessagePair(messagePair, serviceProvider), CodeGenerationService.RequestClassSuffix), CodeGenerationService.RequestClassBaseType, CodeGenerationService.Attribute(typeof(DataContractAttribute), CodeGenerationService.AttributeArg("Namespace", messagePair.MessageNamespace)), CodeGenerationService.Attribute(typeof(RequestProxyAttribute), CodeGenerationService.AttributeArg(null, messagePair.Request.Name)));
			var flag = false;
			var codeStatementCollection = new CodeStatementCollection();
			if (sdkMessageRequest.RequestFields != null & sdkMessageRequest.RequestFields?.Count > 0)
			{
				foreach (var sdkMessageRequestField in sdkMessageRequest.RequestFields.Values)
				{
					var codeMemberProperty = await CodeGenerationService.BuildRequestFieldAsync(sdkMessageRequest, sdkMessageRequestField, serviceProvider);
					if (codeMemberProperty.Type.Options == CodeTypeReferenceOptions.GenericTypeParameter)
					{
                        await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Request Field {0} is generic.  Adding generic parameter to the {1} class.", codeMemberProperty.Name, codeTypeDeclaration.Name);
						flag = true;
						CodeGenerationService.ConvertRequestToGeneric(messagePair, codeTypeDeclaration, codeMemberProperty);
					}
					codeTypeDeclaration.Members.Add(codeMemberProperty);
					if (!sdkMessageRequestField.IsOptional)
					{
						codeStatementCollection.Add(CodeGenerationService.AssignProp(codeMemberProperty.Name, new CodeDefaultValueExpression(codeMemberProperty.Type)));
					}
				}
			}
			if (!flag)
			{
				var codeConstructor = CodeGenerationService.Constructor(Array.Empty<CodeExpression>());
				codeConstructor.Statements.Add(CodeGenerationService.AssignProp(CodeGenerationService.RequestNamePropertyName, new CodePrimitiveExpression(messagePair.Request.Name)));
				codeConstructor.Statements.AddRange(codeStatementCollection);
				codeTypeDeclaration.Members.Add(codeConstructor);
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: SDK Request Class {1} defined", MethodBase.GetCurrentMethod().Name, codeTypeDeclaration.Name);
			return codeTypeDeclaration;
		}

		private static void ConvertRequestToGeneric(SdkMessagePair messagePair, CodeTypeDeclaration requestClass, CodeMemberProperty requestField)
		{
            var codeTypeParameter = new CodeTypeParameter(requestField.Type.BaseType) {HasConstructorConstraint = true};
            codeTypeParameter.Constraints.Add(new CodeTypeReference(CodeGenerationService.EntityClassBaseType));
			requestClass.TypeParameters.Add(codeTypeParameter);
			requestClass.Members.Add(CodeGenerationService.Constructor(CodeGenerationService.New(requestField.Type, Array.Empty<CodeExpression>())));
			var codeConstructor = CodeGenerationService.Constructor(CodeGenerationService.Param(requestField.Type, "target"), CodeGenerationService.AssignProp(requestField.Name, CodeGenerationService.VarRef("target")));
			codeConstructor.Statements.Add(CodeGenerationService.AssignProp(CodeGenerationService.RequestNamePropertyName, new CodePrimitiveExpression(messagePair.Request.Name)));
			requestClass.Members.Add(codeConstructor);
		}

		private static async Task<CodeTypeDeclaration> BuildMessageResponseAsync(SdkMessagePair messagePair, SdkMessageResponse sdkMessageResponse, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var codeTypeDeclaration = CodeGenerationService.Class(string.Format(CultureInfo.InvariantCulture, "{0}{1}", serviceProvider.NamingService.GetNameForMessagePair(messagePair, serviceProvider), CodeGenerationService.ResponseClassSuffix), CodeGenerationService.ResponseClassBaseType, CodeGenerationService.Attribute(typeof(DataContractAttribute), CodeGenerationService.AttributeArg("Namespace", messagePair.MessageNamespace)), CodeGenerationService.Attribute(typeof(ResponseProxyAttribute), CodeGenerationService.AttributeArg(null, messagePair.Request.Name)));
			codeTypeDeclaration.Members.Add(CodeGenerationService.Constructor(Array.Empty<CodeExpression>()));
			if (sdkMessageResponse != null && (sdkMessageResponse.ResponseFields != null & sdkMessageResponse.ResponseFields?.Count > 0))
			{
				using (var enumerator = sdkMessageResponse.ResponseFields.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						var field = enumerator.Current;
						codeTypeDeclaration.Members.Add(await CodeGenerationService.BuildResponseFieldAsync(sdkMessageResponse, field, serviceProvider));
					}
					goto IL_144;
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("SDK Response Class {0} has not fields", codeTypeDeclaration.Name);
			IL_144:
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: SDK Response Class {1} defined", MethodBase.GetCurrentMethod().Name, codeTypeDeclaration.Name);
			return codeTypeDeclaration;
		}

		private static async Task<CodeMemberProperty> BuildRequestFieldAsync(SdkMessageRequest request, SdkMessageRequestField field, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var typeForRequestField = serviceProvider.TypeMappingService.GetTypeForRequestField(field, serviceProvider);
			var codeMemberProperty = CodeGenerationService.PropertyGet(typeForRequestField, serviceProvider.NamingService.GetNameForRequestField(request, field, serviceProvider), Array.Empty<CodeStatement>());
			codeMemberProperty.HasSet = true;
			codeMemberProperty.HasGet = true;
			codeMemberProperty.GetStatements.Add(CodeGenerationService.BuildRequestFieldGetStatement(field, typeForRequestField));
			codeMemberProperty.SetStatements.Add(CodeGenerationService.BuildRequestFieldSetStatement(field));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.Request Property {2} defined", MethodBase.GetCurrentMethod().Name, request.Name, codeMemberProperty.Name);
			return codeMemberProperty;
		}

		private static CodeStatement BuildRequestFieldGetStatement(SdkMessageRequestField field, CodeTypeReference targetType)
		{
			return CodeGenerationService.If(CodeGenerationService.ContainsParameter(field.Name), CodeGenerationService.Return(CodeGenerationService.Cast(targetType, CodeGenerationService.PropertyIndexer(CodeGenerationService.ParametersPropertyName, field.Name))), CodeGenerationService.Return(new CodeDefaultValueExpression(targetType)));
		}

		private static CodeAssignStatement BuildRequestFieldSetStatement(SdkMessageRequestField field)
		{
			return CodeGenerationService.AssignValue(CodeGenerationService.PropertyIndexer(CodeGenerationService.ParametersPropertyName, field.Name));
		}

		private static async Task<CodeMemberProperty> BuildResponseFieldAsync(SdkMessageResponse response, SdkMessageResponseField field, ServiceProvider serviceProvider)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var typeForResponseField = serviceProvider.TypeMappingService.GetTypeForResponseField(field, serviceProvider);
			var codeMemberProperty = CodeGenerationService.PropertyGet(typeForResponseField, serviceProvider.NamingService.GetNameForResponseField(response, field, serviceProvider), Array.Empty<CodeStatement>());
			codeMemberProperty.HasSet = false;
			codeMemberProperty.HasGet = true;
			codeMemberProperty.GetStatements.Add(CodeGenerationService.BuildResponseFieldGetStatement(field, typeForResponseField));
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}: {1}.Response Property {2} defined", MethodBase.GetCurrentMethod().Name, response.Id, codeMemberProperty.Name);
			return codeMemberProperty;
		}

		private static CodeStatement BuildResponseFieldGetStatement(SdkMessageResponseField field, CodeTypeReference targetType)
		{
			return CodeGenerationService.If(CodeGenerationService.ContainsResult(field.Name), CodeGenerationService.Return(CodeGenerationService.Cast(targetType, CodeGenerationService.PropertyIndexer(CodeGenerationService.ResultsPropertyName, field.Name))), CodeGenerationService.Return(new CodeDefaultValueExpression(targetType)));
		}

		private static CodeNamespace Namespace(string name)
		{
			return new CodeNamespace(name);
		}

		private static CodeTypeDeclaration Class(string name, Type baseType, params CodeAttributeDeclaration[] attrs)
		{
			return CodeGenerationService.Class(name, CodeGenerationService.TypeRef(baseType), attrs);
		}

		private static CodeTypeDeclaration Class(string name, CodeTypeReference baseType, params CodeAttributeDeclaration[] attrs)
		{
            var codeTypeDeclaration = new CodeTypeDeclaration(name)
            {
                IsClass = true, TypeAttributes = TypeAttributes.Public
            };
            codeTypeDeclaration.BaseTypes.Add(baseType);
			if (attrs != null)
			{
				codeTypeDeclaration.CustomAttributes.AddRange(attrs);
			}
			codeTypeDeclaration.IsPartial = true;
			codeTypeDeclaration.CustomAttributes.Add(CodeGenerationService.Attribute(typeof(GeneratedCodeAttribute), CodeGenerationService.AttributeArg(CrmSvcUtil.ApplicationName), CodeGenerationService.AttributeArg(CrmSvcUtil.ApplicationVersion)));
			return codeTypeDeclaration;
		}

		private static CodeTypeDeclaration Enum(string name, params CodeAttributeDeclaration[] attrs)
		{
            var codeTypeDeclaration = new CodeTypeDeclaration(name)
            {
                IsEnum = true, TypeAttributes = TypeAttributes.Public
            };
            if (attrs != null)
			{
				codeTypeDeclaration.CustomAttributes.AddRange(attrs);
			}
			codeTypeDeclaration.CustomAttributes.Add(CodeGenerationService.Attribute(typeof(GeneratedCodeAttribute), CodeGenerationService.AttributeArg(CrmSvcUtil.ApplicationName), CodeGenerationService.AttributeArg(CrmSvcUtil.ApplicationVersion)));
			return codeTypeDeclaration;
		}

		private static CodeTypeReference TypeRef(Type type)
		{
			return new CodeTypeReference(type);
		}

		private static CodeAttributeDeclaration Attribute(Type type)
		{
			return new CodeAttributeDeclaration(CodeGenerationService.TypeRef(type));
		}

		private static CodeAttributeDeclaration Attribute(Type type, params CodeAttributeArgument[] args)
		{
			return new CodeAttributeDeclaration(CodeGenerationService.TypeRef(type), args);
		}

		private static CodeAttributeArgument AttributeArg(object value)
		{
            if (value is CodeExpression codeExpression)
			{
				return CodeGenerationService.AttributeArg(null, codeExpression);
			}
			return CodeGenerationService.AttributeArg(null, value);
		}

		private static CodeAttributeArgument AttributeArg(string name, object value)
		{
			return CodeGenerationService.AttributeArg(name, new CodePrimitiveExpression(value));
		}

		private static CodeAttributeArgument AttributeArg(string name, CodeExpression value)
		{
			if (!string.IsNullOrEmpty(name))
			{
				return new CodeAttributeArgument(name, value);
			}
			return new CodeAttributeArgument(value);
		}

		private static CodeMemberProperty PropertyGet(CodeTypeReference type, string name, params CodeStatement[] stmts)
		{
            var codeMemberProperty = new CodeMemberProperty
            {
                Type = type,
                Name = name,
                Attributes = (MemberAttributes) 24578,
                HasGet = true,
                HasSet = false
            };
            codeMemberProperty.GetStatements.AddRange(stmts);
			return codeMemberProperty;
		}

		private static CodeMemberEvent Event(string name, Type type, Type implementationType)
		{
            var codeMemberEvent = new CodeMemberEvent
            {
                Name = name, Type = CodeGenerationService.TypeRef(type), Attributes = (MemberAttributes) 24578
            };
            if (implementationType != null)
			{
				codeMemberEvent.ImplementationTypes.Add(CodeGenerationService.TypeRef(implementationType));
			}
			return codeMemberEvent;
		}

		private static CodeMemberMethod RaiseEvent(string methodName, string eventName, Type eventArgsType)
		{
			var codeMemberMethod = new CodeMemberMethod
			{
				Name = methodName
			};
			codeMemberMethod.Parameters.Add(CodeGenerationService.Param(CodeGenerationService.TypeRef(typeof(string)), "propertyName"));
			var codeEventReferenceExpression = new CodeEventReferenceExpression(CodeGenerationService.This(), eventName);
			codeMemberMethod.Statements.Add(CodeGenerationService.If(CodeGenerationService.NotNull(codeEventReferenceExpression), new CodeDelegateInvokeExpression(codeEventReferenceExpression, CodeGenerationService.This(), CodeGenerationService.New(CodeGenerationService.TypeRef(eventArgsType), CodeGenerationService.VarRef("propertyName")))));
			return codeMemberMethod;
		}

		private static CodeMethodInvokeExpression ContainsParameter(string parameterName)
		{
			return new CodeMethodInvokeExpression(CodeGenerationService.ThisProp(CodeGenerationService.ParametersPropertyName), "Contains", CodeGenerationService.StringLiteral(parameterName));
		}

		private static CodeMethodInvokeExpression ContainsResult(string resultName)
		{
			return new CodeMethodInvokeExpression(CodeGenerationService.ThisProp(CodeGenerationService.ResultsPropertyName), "Contains", CodeGenerationService.StringLiteral(resultName));
		}

		private static CodeConditionStatement If(CodeExpression condition, CodeExpression trueCode)
		{
			return CodeGenerationService.If(condition, new CodeExpressionStatement(trueCode), null);
		}

		private static CodeConditionStatement If(CodeExpression condition, CodeExpression trueCode, CodeExpression falseCode)
		{
			return CodeGenerationService.If(condition, new CodeExpressionStatement(trueCode), new CodeExpressionStatement(falseCode));
		}

		private static CodeConditionStatement If(CodeExpression condition, CodeStatement trueStatement)
		{
			return CodeGenerationService.If(condition, trueStatement, null);
		}

		private static CodeConditionStatement If(CodeExpression condition, CodeStatement trueStatement, CodeStatement falseStatement)
		{
			var codeConditionStatement = new CodeConditionStatement(condition, trueStatement);
			if (falseStatement != null)
			{
				codeConditionStatement.FalseStatements.Add(falseStatement);
			}
			return codeConditionStatement;
		}

		private static CodeFieldReferenceExpression FieldRef(Type targetType, string fieldName)
		{
			return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(targetType), fieldName);
		}

		private static CodeMemberField Field(string name, Type type, object value, params CodeAttributeDeclaration[] attrs)
		{
            var codeMemberField = new CodeMemberField(type, name) {InitExpression = new CodePrimitiveExpression(value)};
            if (attrs != null)
			{
				codeMemberField.CustomAttributes.AddRange(attrs);
			}
			return codeMemberField;
		}

		private static CodeParameterDeclarationExpression Param(CodeTypeReference type, string name)
		{
			return new CodeParameterDeclarationExpression(type, name);
		}

		private static CodeTypeParameter TypeParam(string name, params Type[] constraints)
		{
			var codeTypeParameter = new CodeTypeParameter(name);
			if (constraints != null)
			{
				codeTypeParameter.Constraints.AddRange(constraints.Select(CodeGenerationService.TypeRef).ToArray());
			}
			return codeTypeParameter;
		}

		private static CodeVariableReferenceExpression VarRef(string name)
		{
			return new CodeVariableReferenceExpression(name);
		}

		private static CodeVariableDeclarationStatement Var(Type type, string name, CodeExpression init)
		{
			return new CodeVariableDeclarationStatement(type, name, init);
		}

		private static CodeConstructor Constructor(params CodeExpression[] thisArgs)
		{
            var codeConstructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            if (thisArgs != null)
			{
				codeConstructor.ChainedConstructorArgs.AddRange(thisArgs);
			}
			return codeConstructor;
		}

		private static CodeConstructor Constructor(CodeParameterDeclarationExpression arg, params CodeStatement[] statements)
		{
            var codeConstructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            if (arg != null)
			{
				codeConstructor.Parameters.Add(arg);
			}
			if (statements != null)
			{
				codeConstructor.Statements.AddRange(statements);
			}
			return codeConstructor;
		}

		private static CodeObjectCreateExpression New(CodeTypeReference createType, params CodeExpression[] args)
		{
			return new CodeObjectCreateExpression(createType, args);
		}

		private static CodeAssignStatement AssignProp(string propName, CodeExpression value)
		{
			return new CodeAssignStatement
			{
				Left = CodeGenerationService.ThisProp(propName),
				Right = value
			};
		}

		private static CodeAssignStatement AssignValue(CodeExpression target)
		{
			return CodeGenerationService.AssignValue(target, new CodeVariableReferenceExpression("value"));
		}

		private static CodeAssignStatement AssignValue(CodeExpression target, CodeExpression value)
		{
			return new CodeAssignStatement(target, value);
		}

		private static CodePropertyReferenceExpression BaseProp(string propertyName)
		{
			return new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), propertyName);
		}

		private static CodeIndexerExpression PropertyIndexer(string propertyName, string index)
		{
			return new CodeIndexerExpression(CodeGenerationService.ThisProp(propertyName), new CodePrimitiveExpression(index));
		}

		private static CodePropertyReferenceExpression PropRef(CodeExpression expression, string propertyName)
		{
			return new CodePropertyReferenceExpression(expression, propertyName);
		}

		private static CodePropertyReferenceExpression ThisProp(string propertyName)
		{
			return new CodePropertyReferenceExpression(CodeGenerationService.This(), propertyName);
		}

		private static CodeThisReferenceExpression This()
		{
			return new CodeThisReferenceExpression();
		}

		private static CodeMethodInvokeExpression ThisMethodInvoke(string methodName, params CodeExpression[] parameters)
		{
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(CodeGenerationService.This(), methodName), parameters);
		}

		private static CodeMethodInvokeExpression ThisMethodInvoke(string methodName, CodeTypeReference typeParameter, params CodeExpression[] parameters)
		{
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(CodeGenerationService.This(), methodName, typeParameter), parameters);
		}

		private static CodeMethodInvokeExpression StaticMethodInvoke(Type targetObject, string methodName, params CodeExpression[] parameters)
		{
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(targetObject), methodName), parameters);
		}

		private static CodeMethodInvokeExpression StaticMethodInvoke(Type targetObject, string methodName, CodeTypeReference typeParameter, params CodeExpression[] parameters)
		{
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(targetObject), methodName, typeParameter), parameters);
		}

        private static CodeCastExpression Cast(CodeTypeReference targetType, CodeExpression expression)
		{
			return new CodeCastExpression(targetType, expression);
		}

		private static CodeCommentStatementCollection CommentSummary(Label comment)
		{
			return CodeGenerationService.CommentSummary((comment.UserLocalizedLabel != null) ? comment.UserLocalizedLabel.Label : (comment.LocalizedLabels.Any() ? comment.LocalizedLabels.First().Label : string.Empty));
		}

		private static CodeCommentStatementCollection CommentSummary(string comment)
		{
			return new CodeCommentStatementCollection
			{
				new CodeCommentStatement("<summary>", true),
				new CodeCommentStatement(comment, true),
				new CodeCommentStatement("</summary>", true)
			};
		}

		private static CodePrimitiveExpression StringLiteral(string value)
		{
			return new CodePrimitiveExpression(value);
		}

		private static CodeMethodReturnStatement Return()
		{
			return new CodeMethodReturnStatement();
		}

		private static CodeMethodReturnStatement Return(CodeExpression expression)
		{
			return new CodeMethodReturnStatement(expression);
		}

		private static CodeMethodInvokeExpression ConvertEnum(CodeTypeReference type, string variableName)
		{
			return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(CodeGenerationService.TypeRef(typeof(Enum))), "ToObject", new CodeTypeOfExpression(type), new CodePropertyReferenceExpression(CodeGenerationService.VarRef(variableName), "Value"));
		}

		private static CodeExpression ValueNull()
		{
			return new CodeBinaryOperatorExpression(CodeGenerationService.VarRef("value"), CodeBinaryOperatorType.IdentityEquality, CodeGenerationService.Null());
		}

		private static CodePrimitiveExpression Null()
		{
			return new CodePrimitiveExpression(null);
		}

		private static CodeBinaryOperatorExpression NotNull(CodeExpression expression)
		{
			return new CodeBinaryOperatorExpression(expression, CodeBinaryOperatorType.IdentityInequality, CodeGenerationService.Null());
		}

		private static CodeExpression GuidEmpty()
		{
			return CodeGenerationService.PropRef(new CodeTypeReferenceExpression(typeof(Guid)), "Empty");
		}

		private static CodeExpression False()
		{
			return new CodePrimitiveExpression(false);
		}

		private static CodeTypeReference IEnumerable(CodeTypeReference typeParameter)
		{
			return new CodeTypeReference(typeof(IEnumerable<>).FullName, typeParameter);
		}

		private static CodeTypeReference IQueryable(CodeTypeReference typeParameter)
		{
			return new CodeTypeReference(typeof(IQueryable<>).FullName, typeParameter);
		}

		private static CodeThrowExceptionStatement ThrowArgumentNull(string paramName)
		{
			return new CodeThrowExceptionStatement(CodeGenerationService.New(CodeGenerationService.TypeRef(typeof(ArgumentNullException)), CodeGenerationService.StringLiteral(paramName)));
		}

		private static CodeBinaryOperatorExpression Or(CodeExpression left, CodeExpression right)
		{
			return new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanOr, right);
		}

		private static CodeBinaryOperatorExpression Equal(CodeExpression left, CodeExpression right)
		{
			return new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityEquality, right);
		}

		private static CodeBinaryOperatorExpression And(CodeExpression left, CodeExpression right)
		{
			return new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanAnd, right);
		}

		private static readonly Type AttributeLogicalNameAttribute = typeof(AttributeLogicalNameAttribute);

		private static readonly Type EntityLogicalNameAttribute = typeof(EntityLogicalNameAttribute);

		private static readonly Type RelationshipSchemaNameAttribute = typeof(RelationshipSchemaNameAttribute);

		private static readonly Type ObsoleteFieldAttribute = typeof(ObsoleteAttribute);

		private static readonly Type ServiceContextBaseType = typeof(OrganizationServiceContext);

		private static readonly Type EntityClassBaseType = typeof(Entity);

		private static readonly Type RequestClassBaseType = typeof(OrganizationRequest);

		private static readonly Type ResponseClassBaseType = typeof(OrganizationResponse);

		private static readonly string RequestClassSuffix = "Request";

		private static readonly string ResponseClassSuffix = "Response";

		private static readonly string RequestNamePropertyName = "RequestName";

		private static readonly string ParametersPropertyName = "Parameters";

		private static readonly string ResultsPropertyName = "Results";
	}
}
