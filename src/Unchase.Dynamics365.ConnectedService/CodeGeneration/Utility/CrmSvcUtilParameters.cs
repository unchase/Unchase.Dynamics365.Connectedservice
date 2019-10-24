using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class CrmSvcUtilParameters : ICommandLineArgumentSource
    {
        private readonly MethodTracer _methodTracer;

        internal CrmSvcUtilParameters(MethodTracer methodTracer)
        {
            _methodTracer = methodTracer;
            this.Parser = new CommandLineParser(this);
			this.GenerateMessages = false;
			this.GenerateCustomActions = false;
			this._unknownParameters = new Dictionary<string, string>();
			this.Language = "CS";
		}

        [CommandLineArgument(ArgumentType.Optional, "language", Shortcut = "l", Description = "The language to use for the generated proxy code.  This can be either 'CS' or 'VB'.  The default language is 'CS'.", ParameterDescription = "<language>")]
		internal string Language
		{
			get => this._language;
            set
			{
				if (!CodeDomProvider.IsDefinedLanguage(value))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Language {0} is not a support CodeDom Language.", value));
				}
				this._language = value;
			}
		}

		[CommandLineArgument(ArgumentType.Required, "url", Description = "A url or path to the SDK endpoint to contact for metadata.", ParameterDescription = "<url>", SampleUsageValue = "http://localhost/Organization1/XRMServices/2011/Organization.svc")]
		internal string Url { get; set; }

        [CommandLineArgument(ArgumentType.Required, "out", Description = "The filename for the generated proxy code.", ParameterDescription = "<filename>", Shortcut = "o", SampleUsageValue = "GeneratedCode.cs")]
		internal string OutputFile { get; set; }

        [CommandLineArgument(ArgumentType.Optional, "namespace", Description = "The namespace for the generated proxy code.  The default namespace is the global namespace.", ParameterDescription = "<namespace>", Shortcut = "n")]
		internal string Namespace { get; set; }

        /// <summary>
		/// Used to raise the interactive dialog to login.
		/// </summary>
        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Binary, "interactivelogin", Shortcut = "il", Description = "Presents a login dialog to log into the service with, if passed all other connect info is ignored.")]
		public bool UseInteractiveLogin { get; set; }

		/// <summary>
		/// Used to create a connection utilizing a passed in connection string.
		/// </summary>
        [CommandLineArgument(ArgumentType.Optional, "connectionstring", Shortcut = "connstr", Description = "Connection String to use when connecting to CRM. If provided, all other connect info is ignored.")]
		public string ConnectionString { get; set; }

		/// <summary>
		/// Used to login via OAuth to CRM Online, Hidden for initial ship... but here to allow for complex auth situations.
		/// </summary>
        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "useoauth", Description = "when set, try to login with oAuth to CRM Online")]
		public bool UseOAuth { get; set; }

		[CommandLineArgument(ArgumentType.Optional, "username", Description = "Username to use when connecting to the server for authentication.", ParameterDescription = "<username>", Shortcut = "u")]
		internal string UserName { get; set; }

        [CommandLineArgument(ArgumentType.Optional, "password", Description = "Password to use when connecting to the server for authentication.", ParameterDescription = "<password>", Shortcut = "p")]
		internal string Password { get; set; }

        [CommandLineArgument(ArgumentType.Optional, "domain", Description = "Domain to authenticate against when connecting to the server.", ParameterDescription = "<domain>", Shortcut = "d")]
		internal string Domain { get; set; }

        [CommandLineArgument(ArgumentType.Optional, "serviceContextName", Description = "The name for the generated service context. If a value is passed in, it will be used for the Service Context.  If not, no Service Context will be generated", ParameterDescription = "<service context name>")]
		internal string ServiceContextName { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "messageNamespace", Description = "Namespace of messages to generate.", ParameterDescription = "<message namespace>", Shortcut = "m")]
		internal string MessageNamespace { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "codecustomization", Description = "Full name of the type to use as the ICustomizeCodeDomService", ParameterDescription = "<typename>")]
		internal string CodeCustomizationService { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "codewriterfilter", Description = "Full name of the type to use as the ICodeWriterFilterService", ParameterDescription = "<typename>")]
		internal string CodeWriterFilterService { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "codewritermessagefilter", Description = "Full name of the type to use as the ICodeWriterMessageFilterService", ParameterDescription = "<typename>")]
		internal string CodeWriterMessageFilterService { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "metadataproviderservice", Description = "Full name of the type to use as the IMetadataProviderService", ParameterDescription = "<typename>")]
		internal string MetadataProviderService { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "metadataproviderqueryservice", Description = "Full name of the type to use as the IMetaDataProviderQueryService", ParameterDescription = "<typename>")]
		internal string MetadataQueryProvider { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "codegenerationservice", Description = "Full name of the type to use as the ICodeGenerationService", ParameterDescription = "<typename>")]
		internal string CodeGenerationService { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "namingservice", Description = "Full name of the type to use as the INamingService", ParameterDescription = "<typename>")]
		internal string NamingService { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Binary | ArgumentType.Hidden, "private", Description = "Generate unsupported classes", ParameterDescription = "<private>")]
		internal bool Private { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Binary, "generateActions", Description = "Generate wrapper classes for custom actions", Shortcut = "a")]
		internal bool GenerateCustomActions { get; set; }

        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Binary | ArgumentType.Hidden, "includeMessages", Description = "Generate messages")]
		internal bool GenerateMessages { get; set; }

        [CommandLineArgument(ArgumentType.Optional, "deviceid", Description = "Device ID to use when connecting to the online server for authentication.", ParameterDescription = "<deviceid>", Shortcut = "di")]
		internal string DeviceID { get; set; }

        [CommandLineArgument(ArgumentType.Optional, "devicepassword", Description = "Device Password to use when connecting to the online server for authentication.", ParameterDescription = "<devicepassword>", Shortcut = "dp")]
		internal string DevicePassword { get; set; }

        /// <summary>
		/// Hidden... Used by devToolkit to set the Connection profile to use for this call.
		/// </summary>
        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "connectionprofilename", Description = "connection profile name used")]
		public string ConnectionProfileName { get; set; }

		/// <summary>
		/// Hidden... Used by the devToolkit to set the appName whos connection is being used.
		/// </summary>
        [CommandLineArgument(ArgumentType.Optional | ArgumentType.Hidden, "connectionname", Description = "Application Name whose connection to use")]
		public string ConnectionAppName { get; set; }

		internal IDictionary<string, string> ToDictionary()
		{
			if (this._parametersAsDictionary == null)
			{
				this._parametersAsDictionary = new Dictionary<string, string>();
				foreach (var propertyInfo in typeof(CrmSvcUtilParameters).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					var customAttributes = propertyInfo.GetCustomAttributes(typeof(CommandLineArgumentAttribute), false);
					if (customAttributes.Length != 0)
					{
						var commandLineArgumentAttribute = (CommandLineArgumentAttribute)customAttributes[0];
						var value = propertyInfo.GetValue(this, null);
						if (value != null)
						{
							this._parametersAsDictionary.Add(commandLineArgumentAttribute.Name, value.ToString());
						}
					}
				}
				foreach (var key in this._unknownParameters.Keys)
				{
					this._parametersAsDictionary.Add(key, this._unknownParameters[key]);
				}
			}
			return this._parametersAsDictionary;
		}

		private CommandLineParser Parser { get; }

        private bool ContainsUnknownParameters => this._unknownParameters.Count != 0 && string.IsNullOrWhiteSpace(this.CodeCustomizationService) && string.IsNullOrWhiteSpace(this.CodeGenerationService) && string.IsNullOrWhiteSpace(this.MetadataQueryProvider) && string.IsNullOrWhiteSpace(this.CodeWriterFilterService) && string.IsNullOrWhiteSpace(this.CodeWriterMessageFilterService) && string.IsNullOrWhiteSpace(this.MetadataProviderService) && string.IsNullOrWhiteSpace(this.NamingService);

        internal async Task LoadArgumentsAsync(string[] args)
		{
            await this._methodTracer.EnterAsync();
			await this.Parser.ParseArgumentsAsync(args);
            await this._methodTracer.ExitAsync();
		}

		internal async Task<bool> VerifyArgumentsAsync()
		{
            await this._methodTracer.EnterAsync();
			if (!await this.Parser.VerifyArgumentsAsync())
			{
                await this._methodTracer.LogWarningAsync("Exiting {0} with false return value due to the parser finding invalid arguments");
				return false;
			}
			if (this.ContainsUnknownParameters)
			{
                await this._methodTracer.LogWarningAsync("Exiting {0} with false return value due to finding unknown parameters");
				return false;
			}
			if (!string.IsNullOrEmpty(this.UserName) && string.IsNullOrEmpty(this.Password))
			{
                await this._methodTracer.LogWarningAsync("Exiting {0} with false return value due to invalid credentials");
				return false;
			}
            await this._methodTracer.LogMessageAsync("Exiting {0} with true return value");
			return true;
		}

        async Task ICommandLineArgumentSource.OnUnknownArgumentAsync(string argumentName, string argumentValue)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("{0}: Found unknown argument {1}{2}.", MethodBase.GetCurrentMethod().Name, '/', argumentName);
			this._unknownParameters[argumentName] = argumentValue;
		}

		async Task ICommandLineArgumentSource.OnInvalidArgumentAsync(string argument)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Exiting {0}: Found string {1} in arguments array that could not be parsed.", MethodBase.GetCurrentMethod().Name, argument);
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Argument '{0}' could not be parsed.", argument));
		}

        private string _language;

        private readonly Dictionary<string, string> _unknownParameters;

		private Dictionary<string, string> _parametersAsDictionary;
	}
}
