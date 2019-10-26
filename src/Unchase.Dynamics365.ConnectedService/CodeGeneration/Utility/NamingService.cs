using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class NamingService : INamingService
	{
        internal NamingService(CrmSvcUtilParameters parameters)
		{
			if (!string.IsNullOrWhiteSpace(parameters.ServiceContextName))
			{
				this._serviceContextName = parameters.ServiceContextName;
			}
			else
			{
				this._serviceContextName = typeof(OrganizationServiceContext).Name + "1";
			}
			this._nameMap = new Dictionary<string, int>();
			this._knowNames = new Dictionary<string, string>();
			this._reservedAttributeNames = new List<string>();
			foreach (var propertyInfo in typeof(Entity).GetProperties())
			{
				this._reservedAttributeNames.Add(propertyInfo.Name);
			}
		}

        async Task<string> INamingService.GetNameForOptionSetAsync(EntityMetadata entityMetadata,
            OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            if (optionSetMetadata.MetadataId != null && this._knowNames.ContainsKey(optionSetMetadata.MetadataId.Value.ToString()))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[optionSetMetadata.MetadataId.Value.ToString()];
			}

            var text = optionSetMetadata.OptionSetType != null && optionSetMetadata.OptionSetType.Value == OptionSetType.State ? this.CreateValidTypeName(entityMetadata.SchemaName + "State") : this.CreateValidTypeName(optionSetMetadata.Name);
            if (optionSetMetadata.MetadataId != null)
                this._knowNames.Add(optionSetMetadata.MetadataId.Value.ToString(), text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

        async Task<string> INamingService.GetNameForOptionAsync(OptionSetMetadataBase optionSetMetadata,
            OptionMetadata optionMetadata, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            if (optionMetadata.Value != null && (optionSetMetadata.MetadataId != null && this._knowNames.ContainsKey(optionSetMetadata.MetadataId.Value.ToString() + optionMetadata.Value.Value.ToString(CultureInfo.InvariantCulture))))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[optionSetMetadata.MetadataId.Value.ToString() + optionMetadata.Value.Value.ToString(CultureInfo.InvariantCulture)];
			}
			var text = string.Empty;
            if (optionMetadata is StateOptionMetadata stateOptionMetadata)
			{
				text = stateOptionMetadata.InvariantName;
			}
			else
			{
				foreach (var localizedLabel in optionMetadata.Label.LocalizedLabels)
				{
					if (localizedLabel.LanguageCode == 1033)
					{
						text = localizedLabel.Label;
					}
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = string.Format(CultureInfo.InvariantCulture, "UnknownLabel{0}", optionMetadata.Value.Value);
			}
			text = NamingService.CreateValidName(text);
            if (optionSetMetadata.MetadataId != null)
                if (optionMetadata.Value != null)
                    this._knowNames.Add(
                        optionSetMetadata.MetadataId.Value.ToString() +
                        optionMetadata.Value.Value.ToString(CultureInfo.InvariantCulture), text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

        async Task<string> INamingService.GetNameForEntityAsync(EntityMetadata entityMetadata, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            if (entityMetadata.MetadataId != null && this._knowNames.ContainsKey(entityMetadata.MetadataId.Value.ToString()))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[entityMetadata.MetadataId.Value.ToString()];
			}
			var name = string.IsNullOrEmpty(StaticNamingService.GetNameForEntity(entityMetadata)) ? entityMetadata.SchemaName : StaticNamingService.GetNameForEntity(entityMetadata);
			var text = this.CreateValidTypeName(name);
            if (entityMetadata.MetadataId != null)
                this._knowNames.Add(entityMetadata.MetadataId.Value.ToString(), text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

        async Task<string> INamingService.GetNameForAttributeAsync(EntityMetadata entityMetadata,
            AttributeMetadata attributeMetadata, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            if (attributeMetadata.MetadataId != null && (entityMetadata.MetadataId != null && this._knowNames.ContainsKey(entityMetadata.MetadataId.Value.ToString() + attributeMetadata.MetadataId.Value)))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[entityMetadata.MetadataId.Value.ToString() + attributeMetadata.MetadataId.Value];
			}
			var text = StaticNamingService.GetNameForAttribute(attributeMetadata) ?? attributeMetadata.SchemaName;
			text = NamingService.CreateValidName(text);
			var namingService = (INamingService)services?.GetService(typeof(INamingService));
			if (this._reservedAttributeNames.Contains(text) || text == await namingService.GetNameForEntityAsync(entityMetadata, services))
			{
				text += "1";
			}

            if (entityMetadata.MetadataId != null)
                if (attributeMetadata.MetadataId != null)
                    this._knowNames.Add(entityMetadata.MetadataId.Value.ToString() + attributeMetadata.MetadataId.Value,
                        text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

        async Task<string> INamingService.GetNameForRelationshipAsync(EntityMetadata entityMetadata,
            RelationshipMetadataBase relationshipMetadata, EntityRole? reflexiveRole, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            var arg = (reflexiveRole != null) ? reflexiveRole.Value.ToString() : string.Empty;
			if (relationshipMetadata.MetadataId != null && (entityMetadata.MetadataId != null && this._knowNames.ContainsKey(entityMetadata.MetadataId.Value.ToString() + relationshipMetadata.MetadataId.Value + arg)))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[entityMetadata.MetadataId.Value.ToString() + relationshipMetadata.MetadataId.Value + arg];
			}
			var text = (reflexiveRole == null) ? relationshipMetadata.SchemaName : ((reflexiveRole.Value == EntityRole.Referenced) ? ("Referenced" + relationshipMetadata.SchemaName) : ("Referencing" + relationshipMetadata.SchemaName));
			text = NamingService.CreateValidName(text);
			var dictionary = (from d in this._knowNames
			where entityMetadata.MetadataId != null && d.Key.StartsWith(entityMetadata.MetadataId.Value.ToString())
			select d).ToDictionary((KeyValuePair<string, string> d) => d.Key, (KeyValuePair<string, string> d) => d.Value);
			var namingService = (INamingService)services?.GetService(typeof(INamingService));
			if (this._reservedAttributeNames.Contains(text) || text == await namingService.GetNameForEntityAsync(entityMetadata, services) || dictionary.ContainsValue(text))
			{
				text += "1";
			}

            if (entityMetadata.MetadataId != null)
                if (relationshipMetadata.MetadataId != null)
                    this._knowNames.Add(
                        entityMetadata.MetadataId.Value.ToString() + relationshipMetadata.MetadataId.Value + arg, text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

        async Task<string> INamingService.GetNameForServiceContextAsync(IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return this._serviceContextName;
		}

		async Task<string> INamingService.GetNameForEntitySetAsync(EntityMetadata entityMetadata, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return await ((INamingService)services?.GetService(typeof(INamingService)))?.GetNameForEntityAsync(entityMetadata, services) + "Set";
		}

        async Task<string> INamingService.GetNameForMessagePairAsync(SdkMessagePair messagePair, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            if (this._knowNames.ContainsKey(messagePair.Id.ToString()))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[messagePair.Id.ToString()];
			}
			var text = this.CreateValidTypeName(messagePair.Request.Name);
			this._knowNames.Add(messagePair.Id.ToString(), text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

        async Task<string> INamingService.GetNameForRequestFieldAsync(SdkMessageRequest request,
            SdkMessageRequestField requestField, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            if (this._knowNames.ContainsKey(request.Id.ToString() + requestField.Index.ToString(CultureInfo.InvariantCulture)))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[request.Id.ToString() + requestField.Index.ToString(CultureInfo.InvariantCulture)];
			}
			var text = NamingService.CreateValidName(requestField.Name);
			this._knowNames.Add(request.Id.ToString() + requestField.Index.ToString(CultureInfo.InvariantCulture), text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

        async Task<string> INamingService.GetNameForResponseFieldAsync(SdkMessageResponse response,
            SdkMessageResponseField responseField, IServiceProvider services)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            if (this._knowNames.ContainsKey(response.Id.ToString() + responseField.Index.ToString(CultureInfo.InvariantCulture)))
			{
                await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
                return this._knowNames[response.Id.ToString() + responseField.Index.ToString(CultureInfo.InvariantCulture)];
			}
			var text = NamingService.CreateValidName(responseField.Name);
			this._knowNames.Add(response.Id.ToString() + responseField.Index.ToString(CultureInfo.InvariantCulture), text);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
            return text;
		}

		private string CreateValidTypeName(string name)
		{
			var text = NamingService.CreateValidName(name);
			if (this._nameMap.ContainsKey(text))
			{
				var num = this._nameMap[text];
				num++;
				this._nameMap[text] = num;
				return string.Format(CultureInfo.InvariantCulture, "{0}{1}", text, num);
			}
			this._nameMap.Add(name, 0);
			return text;
		}

		private static string CreateValidName(string name)
		{
			var input = name.Replace("$", "CurrencySymbol_").Replace("(", "_");
			var stringBuilder = new StringBuilder();
			var match = NamingService.NameRegex.Match(input);
			while (match.Success)
			{
				stringBuilder.Append(match.Value);
				match = match.NextMatch();
			}
			return stringBuilder.ToString();
		}

		private const string ConflictResolutionSuffix = "1";

		private const string ReferencingReflexiveRelationshipPrefix = "Referencing";

		private const string ReferencedReflexiveRelationshipPrefix = "Referenced";

		private readonly string _serviceContextName;

		private readonly Dictionary<string, int> _nameMap;

		private readonly Dictionary<string, string> _knowNames;

		private readonly List<string> _reservedAttributeNames;

		private static readonly Regex NameRegex = new Regex("[a-z0-9_]*", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
	}
}
