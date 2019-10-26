using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class TypeMappingService : ITypeMappingService
	{
        internal TypeMappingService(CrmSvcUtilParameters parameters)
		{
			this.Namespace = parameters.Namespace;
            this._attributeTypeMapping = new Dictionary<AttributeTypeCode, Type>
            {
                {AttributeTypeCode.Boolean, typeof(bool)},
                {AttributeTypeCode.ManagedProperty, typeof(BooleanManagedProperty)},
                {AttributeTypeCode.CalendarRules, typeof(object)},
                {AttributeTypeCode.Customer, typeof(EntityReference)},
                {AttributeTypeCode.DateTime, typeof(DateTime)},
                {AttributeTypeCode.Decimal, typeof(decimal)},
                {AttributeTypeCode.Double, typeof(double)},
                {AttributeTypeCode.Integer, typeof(int)},
                {AttributeTypeCode.EntityName, typeof(string)},
                {AttributeTypeCode.BigInt, typeof(long)},
                {AttributeTypeCode.Lookup, typeof(EntityReference)},
                {AttributeTypeCode.Memo, typeof(string)},
                {AttributeTypeCode.Money, typeof(Money)},
                {AttributeTypeCode.Owner, typeof(EntityReference)},
                {AttributeTypeCode.String, typeof(string)},
                {AttributeTypeCode.Uniqueidentifier, typeof(Guid)}
            };
        }

		private string Namespace { get; }

        async Task<CodeTypeReference> ITypeMappingService.GetTypeForEntityAsync(EntityMetadata entityMetadata,
            IServiceProvider services)
		{
			var nameForEntity = await ((INamingService)services?.GetService(typeof(INamingService)))?.GetNameForEntityAsync(entityMetadata, services);
			return this.TypeRef(nameForEntity);
		}

		async Task<CodeTypeReference> ITypeMappingService.GetTypeForAttributeTypeAsync(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
		{
			var type = typeof(object);
			if (attributeMetadata.AttributeType != null)
			{
				var value = attributeMetadata.AttributeType.Value;
				if (this._attributeTypeMapping.ContainsKey(value))
				{
					type = this._attributeTypeMapping[value];
				}
				else
				{
					if (value == AttributeTypeCode.PartyList)
					{
						return await this.BuildCodeTypeReferenceForPartyListAsync(services);
					}
					if (attributeMetadata is ImageAttributeMetadata)
					{
						type = typeof(byte[]);
					}
					else
					{
						var attributeOptionSet = TypeMappingService.GetAttributeOptionSet(attributeMetadata);
						if (attributeOptionSet != null)
						{
							var codeTypeReference = await this.BuildCodeTypeReferenceForOptionSetAsync(attributeMetadata.LogicalName, entityMetadata, attributeOptionSet, services);
							if (!codeTypeReference.BaseType.Equals("System.Object"))
							{
								return codeTypeReference;
							}
							if (value.Equals(AttributeTypeCode.Picklist) || value.Equals(AttributeTypeCode.Status))
							{
								type = typeof(OptionSetValue);
								if (type.IsValueType)
								{
									type = typeof(Nullable<>).MakeGenericType(type);
								}
							}
						}
					}
				}
				if (type.IsValueType)
				{
					type = typeof(Nullable<>).MakeGenericType(type);
				}
			}
			return TypeMappingService.TypeRef(type);
		}

		async Task<CodeTypeReference> ITypeMappingService.GetTypeForRelationshipAsync(
            RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata,
            IServiceProvider services)
		{
			var nameForEntity = await ((INamingService)services?.GetService(typeof(INamingService)))?.GetNameForEntityAsync(otherEntityMetadata, services);
			return this.TypeRef(nameForEntity);
		}

        async Task<CodeTypeReference> ITypeMappingService.GetTypeForRequestFieldAsync(SdkMessageRequestField requestField,
            IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return this.GetTypeForField(requestField.CLRFormatter, requestField.IsGeneric);
		}

        async Task<CodeTypeReference> ITypeMappingService.GetTypeForResponseFieldAsync(SdkMessageResponseField responseField,
            IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return this.GetTypeForField(responseField.CLRFormatter, false);
		}

		private async Task<CodeTypeReference> BuildCodeTypeReferenceForOptionSetAsync(string attributeName, EntityMetadata entityMetadata, OptionSetMetadataBase attributeOptionSet, IServiceProvider services)
		{
			var codeWriterFilterService = (ICodeWriterFilterService)services?.GetService(typeof(ICodeWriterFilterService));
			var namingService = (INamingService)services?.GetService(typeof(INamingService));
			var codeGenerationService = (ICodeGenerationService)services?.GetService(typeof(ICodeGenerationService));
			if (codeWriterFilterService != null && await codeWriterFilterService.GenerateOptionSetAsync(attributeOptionSet, services))
			{
				var nameForOptionSet = await namingService.GetNameForOptionSetAsync(entityMetadata, attributeOptionSet, services);
				var typeForOptionSet = await codeGenerationService.GetTypeForOptionSetAsync(entityMetadata, attributeOptionSet, services);
				if (typeForOptionSet == CodeGenerationType.Class)
				{
					return this.TypeRef(nameForOptionSet);
				}
				if (typeForOptionSet == CodeGenerationType.Enum || typeForOptionSet == CodeGenerationType.Struct)
				{
					return TypeMappingService.TypeRef(typeof(Nullable<>), this.TypeRef(nameForOptionSet));
				}
				await CrmSvcUtil.CrmSvcUtilLogger.TraceWarningAsync("Cannot map type for atttribute {0} with OptionSet type {1} which has CodeGenerationType {2}", attributeName, attributeOptionSet.Name, typeForOptionSet);
			}
			return TypeMappingService.TypeRef(typeof(object));
		}

		private async Task<CodeTypeReference> BuildCodeTypeReferenceForPartyListAsync(IServiceProvider services)
		{
			var filterService = (ICodeWriterFilterService)services?.GetService(typeof(ICodeWriterFilterService));
			var namingService = (INamingService)services?.GetService(typeof(INamingService));
			var entityMetadata = (await ((IMetadataProviderService)services?.GetService(typeof(IMetadataProviderService)))?.LoadMetadataAsync()).Entities.FirstOrDefault((EntityMetadata entity) => string.Equals(entity.LogicalName, "activityparty", StringComparison.Ordinal) && filterService?.GenerateEntityAsync(entity, services).GetAwaiter().GetResult() == true);
			if (entityMetadata == null)
			{
				return TypeMappingService.TypeRef(typeof(IEnumerable<>), TypeMappingService.TypeRef(typeof(Entity)));
			}
			return TypeMappingService.TypeRef(typeof(IEnumerable<>), this.TypeRef(await namingService?.GetNameForEntityAsync(entityMetadata, services)));
		}

		internal static OptionSetMetadataBase GetAttributeOptionSet(AttributeMetadata attribute)
		{
			OptionSetMetadataBase result = null;
			var type = attribute.GetType();
			if (type == typeof(BooleanAttributeMetadata))
			{
				result = ((BooleanAttributeMetadata)attribute).OptionSet;
			}
			else if (type == typeof(StateAttributeMetadata))
			{
				result = ((StateAttributeMetadata)attribute).OptionSet;
			}
			else if (type == typeof(PicklistAttributeMetadata))
			{
				result = ((PicklistAttributeMetadata)attribute).OptionSet;
			}
			else if (type == typeof(StatusAttributeMetadata))
			{
				result = ((StatusAttributeMetadata)attribute).OptionSet;
			}
			return result;
		}

		private CodeTypeReference GetTypeForField(string clrFormatter, bool isGeneric)
		{
			var result = TypeMappingService.TypeRef(typeof(object));
			if (isGeneric)
			{
				result = new CodeTypeReference(new CodeTypeParameter("T"));
			}
			else if (!string.IsNullOrEmpty(clrFormatter))
			{
				var type = Type.GetType(clrFormatter, false);
				if (type != null)
				{
					result = TypeMappingService.TypeRef(type);
				}
				else
				{
					var array = clrFormatter.Split(new[]
					{
						','
					}, StringSplitOptions.RemoveEmptyEntries);
					if (array.Length != 0)
					{
						result = new CodeTypeReference(array[0]);
					}
				}
			}
			return result;
		}

		private CodeTypeReference TypeRef(string typeName)
		{
			if (!string.IsNullOrWhiteSpace(this.Namespace))
			{
				return new CodeTypeReference(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.Namespace, typeName));
			}
			return new CodeTypeReference(typeName);
		}

		private static CodeTypeReference TypeRef(Type type)
		{
			return new CodeTypeReference(type);
		}

		private static CodeTypeReference TypeRef(Type type, CodeTypeReference typeParameter)
		{
			return new CodeTypeReference(type.FullName, typeParameter);
		}

		private readonly Dictionary<AttributeTypeCode, Type> _attributeTypeMapping;
    }
}
