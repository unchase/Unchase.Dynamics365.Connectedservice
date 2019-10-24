using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    /// <summary>
    /// Wrapper class for Command Line Argument PropertyInfos.
    /// </summary>
    internal sealed class CommandLineArgument
	{
        internal CommandLineArgument(PropertyInfo argumentProperty, CommandLineArgumentAttribute argumentAttribute)
		{
            this._argumentAttribute = argumentAttribute;
			this.ArgumentProperty = argumentProperty;
        }

		internal bool HasShortcut => !string.IsNullOrEmpty(this._argumentAttribute.Shortcut);

        internal string Shortcut => this._argumentAttribute.Shortcut;

        internal string Name => this._argumentAttribute.Name;

        internal string ParameterDescription => this._argumentAttribute.ParameterDescription;

        internal bool IsSet { get; private set; }

        internal bool IsCollection => this.ArgumentProperty.PropertyType.GetInterface("IList", true) != null || this.ArgumentProperty.PropertyType.GetInterface(typeof(IList<>).FullName, true) != null;

        internal bool IsRequired => (this._argumentAttribute.Type & ArgumentType.Required) == ArgumentType.Required;

        internal bool SupportsMultiple => (this._argumentAttribute.Type & ArgumentType.Multiple) == ArgumentType.Multiple;

        internal bool IsFlag => (this._argumentAttribute.Type & ArgumentType.Binary) == ArgumentType.Binary;

        internal bool IsHidden => (this._argumentAttribute.Type & ArgumentType.Hidden) == ArgumentType.Hidden;

        internal string Description => this._argumentAttribute.Description;

        internal string SampleUsageValue => this._argumentAttribute.SampleUsageValue;

        private PropertyInfo ArgumentProperty { get; }

        private static int? WrapLength
		{
			get
			{
				int? result;
				try
				{
					result = Console.WindowWidth;
				}
				catch (IOException)
				{
					result = null;
				}
				return result;
			}
		}

		internal async Task SetValueAsync(object argTarget, string argValue)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Attempting to set the Argument {0} with the value {1}", argTarget.ToString(), CommandLineArgument.ToNullableString(argValue));
			if (this.IsSet && !this.SupportsMultiple)
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("Attempt to set argument {0} multiple times", this.ArgumentProperty.Name);
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot set command line argument {0} multiple times", this.ArgumentProperty.Name));
			}
			if (this.IsCollection)
			{
				await this.PopulateCollectionParameterAsync(argTarget, argValue);
			}
			else if (this.IsFlag)
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Setting flag property {0} to true", this.ArgumentProperty.Name);
				this.ArgumentProperty.SetValue(argTarget, true, null);
			}
			else
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Setting property {0} to value {1}", this.ArgumentProperty.Name, CommandLineArgument.ToNullableString(argValue));
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Converting parameter value as ArgumentProperty {0} is defined as type {1}.", this.ArgumentProperty.Name, this.ArgumentProperty.PropertyType.Name);
				var value = Convert.ChangeType(argValue, this.ArgumentProperty.PropertyType, CultureInfo.InvariantCulture);
				this.ArgumentProperty.SetValue(argTarget, value, null);
			}
			this.IsSet = true;
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
		}

		private async Task PopulateCollectionParameterAsync(object argTarget, string argValue)
		{
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
			var list = this.ArgumentProperty.GetValue(argTarget, null) as IList;
			if (list == null)
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceErrorAsync("ArgumentProperty {0} did not return an IList as expected", this.ArgumentProperty.ToString());
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "ArgumentProperty {0} did not return an IList as expected.", this.ArgumentProperty.ToString()));
			}
			var genericArguments = this.ArgumentProperty.PropertyType.GetGenericArguments();
			if (genericArguments.Length == 0)
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Adding parameter value directly as ArgumentProperty {0} is not defined as a generic.", this.ArgumentProperty.Name);
				list.Add(argValue);
			}
			else
			{
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Casting parameter value as ArgumentProperty {0} is defined as a generic of type {1}.", this.ArgumentProperty.Name, genericArguments[0].Name);
				var value = Convert.ChangeType(argValue, genericArguments[0], CultureInfo.InvariantCulture);
				await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Argument value casted to {0} successfully.", genericArguments[0].Name);
				list.Add(value);
			}
			await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
		}

		public override string ToString()
		{
            var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(this.ToDescriptionString());
			var stringBuilder2 = new StringBuilder("  ");
			stringBuilder2.Append(this.Description);
			if (this.HasShortcut)
			{
				var arg = ':'.ToString();
				if (this.IsFlag)
				{
					arg = string.Empty;
				}
				var arg2 = string.Format(CultureInfo.InvariantCulture, "Short form is '{0}{1}{2}'.", '/', this.Shortcut, arg);
				stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "  {0}", arg2);
			}
			stringBuilder.AppendLine(CommandLineArgument.WrapLine(stringBuilder2.ToString()));
            return stringBuilder.ToString();
		}

		internal string ToSampleString()
		{
			return this.ToSwitchString(this.SampleUsageValue);
		}

		private string ToDescriptionString()
		{
			if (this.IsFlag)
			{
				return this.ToSwitchString(string.Empty);
			}
			return this.ToSwitchString(this.ParameterDescription);
		}

		private string ToSwitchString(string value)
		{
			var format = " {0}{1}{2}{3}";
			var text = ':'.ToString();
			if (this.IsFlag)
			{
				text = string.Empty;
			}
			return string.Format(CultureInfo.InvariantCulture, format, '/', this.Name, text, value);
		}

		private static string ToNullableString(object value)
		{
			if (value == null)
			{
				return "<NULL>";
			}
			return value.ToString();
		}

		internal static string WrapLine(string text)
		{
			var wrapLength = CommandLineArgument.WrapLength;
			if (wrapLength == null)
			{
				return text;
			}
			var array = text.Split(null);
			var stringBuilder = new StringBuilder();
			var num = 0;
			foreach (var text2 in array)
			{
				var length = text2.Length;
				var num2 = num + length + 1;
				var num3 = wrapLength;
				if (num2 >= num3.GetValueOrDefault() & num3 != null)
				{
					num = length + 1;
					stringBuilder.Append("\n  " + text2);
				}
				else
				{
					num += length + 1;
					stringBuilder.Append(text2);
				}
				stringBuilder.Append(' ');
			}
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Character used to start a new command line parameter.
		/// </summary>
        internal const char ArgumentStartChar = '/';

		/// <summary>
		/// Character used to separate command line parameter and value.
		/// </summary>
        internal const char ArgumentSeparatorChar = ':';

		/// <summary>
		/// Format to use when constructing the short form description for an argument.
		/// </summary>
        private const string ShortFormFormat = "Short form is '{0}{1}{2}'.";

        private readonly CommandLineArgumentAttribute _argumentAttribute;
    }
}
