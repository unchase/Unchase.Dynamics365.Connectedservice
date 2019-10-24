using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    /// <summary>
    /// Utility class to parse command line arguments.
    /// </summary>
    internal sealed class CommandLineParser
	{
        /// <summary>
        /// Creates a new command line parser for the given object.
        /// </summary>
        /// <param name="argsSource">The object containing the properties representing the command line args to set.</param>
        internal CommandLineParser(ICommandLineArgumentSource argsSource)
		{
			this.ArgumentsSource = argsSource;
			this.Arguments = new List<CommandLineArgument>();
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                this.ArgumentsMap = await this.GetPropertyMapAsync();
            });
        }

        /// <summary>
        /// The object that contains the properties to set.
        /// </summary>
        private ICommandLineArgumentSource ArgumentsSource { get; }

        /// <summary>
        /// A list of all of the arguments that are supported
        /// </summary>
        private List<CommandLineArgument> Arguments { get; }

        /// <summary>
        /// A mapping of argument switches to command line arguments.
        /// </summary>
        private Dictionary<string, CommandLineArgument> ArgumentsMap { get; set; }

        internal async Task ParseArgumentsAsync(string[] args)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			if (args != null)
			{
				foreach (var argument in args)
				{
					if (CommandLineParser.IsArgument(argument))
					{
                        var argumentName = CommandLineParser.GetArgumentName(argument, out var text);
						if (!string.IsNullOrEmpty(argumentName) && this.ArgumentsMap.ContainsKey(argumentName))
						{
							await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Setting argument {0} to value {1}", argumentName, text);
							await this.ArgumentsMap[argumentName].SetValueAsync(this.ArgumentsSource, text);
						}
						else
						{
							await this.ArgumentsSource.OnUnknownArgumentAsync(argumentName, text);
						}
					}
					else
					{
						await this.ArgumentsSource.OnInvalidArgumentAsync(argument);
					}
				}
			}
			await this.ParseConfigArgumentsAsync();
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
		}

		private async Task ParseConfigArgumentsAsync()
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			foreach (var text in ConfigurationManager.AppSettings.AllKeys)
			{
				var text2 = text.ToUpperInvariant();
				var text3 = ConfigurationManager.AppSettings[text];
				if (this.ArgumentsMap.ContainsKey(text2) && !this.ArgumentsMap[text2].IsSet)
				{
					await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Setting argument {0} to config value {1}", text2, text3);
					await this.ArgumentsMap[text2].SetValueAsync(this.ArgumentsSource, text3);
				}
				else
				{
					await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping config value {0} as it is an unknown argument.", text2);
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
		}

		internal async Task<bool> VerifyArgumentsAsync()
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			if (this.ArgumentsMap.ContainsKey("CONNECTIONNAME") && this.ArgumentsMap["CONNECTIONNAME"].IsSet && ((this.ArgumentsMap.ContainsKey("OUT") && this.ArgumentsMap["OUT"].IsSet) || (this.ArgumentsMap.ContainsKey("O") && this.ArgumentsMap["O"].IsSet)))
			{
				return true;
			}
			if (this.ArgumentsMap.ContainsKey("INTERACTIVELOGIN") && this.ArgumentsMap["INTERACTIVELOGIN"].IsSet && ((this.ArgumentsMap.ContainsKey("OUT") && this.ArgumentsMap["OUT"].IsSet) || (this.ArgumentsMap.ContainsKey("O") && this.ArgumentsMap["O"].IsSet)))
			{
				return true;
			}
			if (this.ArgumentsMap.ContainsKey("CONNECTIONSTRING") && this.ArgumentsMap["CONNECTIONSTRING"].IsSet && ((this.ArgumentsMap.ContainsKey("OUT") && this.ArgumentsMap["OUT"].IsSet) || (this.ArgumentsMap.ContainsKey("O") && this.ArgumentsMap["O"].IsSet)))
			{
				return true;
			}
			foreach (var commandLineArgument in this.ArgumentsMap.Values)
			{
				if (commandLineArgument.IsRequired && !commandLineArgument.IsSet)
				{
                    await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0} with false return value because argument {1} is not set.", MethodBase.GetCurrentMethod().Name, commandLineArgument.Name);
					return false;
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0} with true return value", MethodBase.GetCurrentMethod().Name);
			return true;
		}

		/// <summary>
		/// Populates the command line arguments map.
		/// </summary>
        private async Task<Dictionary<string, CommandLineArgument>> GetPropertyMapAsync()
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var dictionary = new Dictionary<string, CommandLineArgument>();
			foreach (var propertyInfo in this.ArgumentsSource.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty))
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Checking property {0} for command line attribution", propertyInfo.Name);
				var commandLineAttribute = CommandLineParser.GetCommandLineAttribute(propertyInfo);
				if (commandLineAttribute == null)
				{
					await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Skipping property {0} since it does not have command line attribution", propertyInfo.Name);
				}
				else
				{
					await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Creating CommandLineArgument for Property {0}", propertyInfo.Name);
					var commandLineArgument = new CommandLineArgument(propertyInfo, commandLineAttribute);
					this.Arguments.Add(commandLineArgument);
					await CommandLineParser.CreateMapEntryAsync(dictionary, propertyInfo, commandLineArgument, "shortcut", commandLineArgument.Shortcut);
                    await CommandLineParser.CreateMapEntryAsync(dictionary, propertyInfo, commandLineArgument, "name", commandLineArgument.Name);
				}
			}
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0} with PropertyMap length of {1} ", MethodBase.GetCurrentMethod().Name, dictionary.Count);
			return dictionary;
		}

		private static async Task<bool> CreateMapEntryAsync(Dictionary<string, CommandLineArgument> propertyMap, PropertyInfo property, CommandLineArgument argument, string type, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Property {0} has defined a {1} {2}", property.Name, type, value);
				propertyMap.Add(value.ToUpperInvariant(), argument);
				return true;
			}
            return false;
		}

		private static CommandLineArgumentAttribute GetCommandLineAttribute(PropertyInfo property)
		{
			var customAttributes = property.GetCustomAttributes(typeof(CommandLineArgumentAttribute), false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			return (CommandLineArgumentAttribute)customAttributes[0];
		}

		private static bool IsArgument(string argument)
		{
			return argument[0] == '/';
		}

		private static string GetArgumentName(string argument, out string argumentValue)
		{
			argumentValue = null;
			string text = null;
			if (argument[0] == '/')
			{
				var num = argument.IndexOf(':');
				if (num != -1)
				{
					text = argument.Substring(1, num - 1);
					argumentValue = argument.Substring(num + 1);
				}
				else
				{
					text = argument.Substring(1);
				}
			}
			return text?.ToUpperInvariant();
		}
    }
}
