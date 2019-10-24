using System;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
	/// <remarks>
	/// Represents a command line argument.
	/// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class CommandLineArgumentAttribute : Attribute
	{
		/// <summary>
		/// Creates a new command line argument attribute.
		/// </summary>
		/// <param name="argType">Type of argument represented by the property.</param>
		/// <param name="name">Switch used by the command line argument</param>
        internal CommandLineArgumentAttribute(ArgumentType argType, string name)
		{
			this.Type = argType;
			this.Name = name;
			this.Shortcut = string.Empty;
			this.Description = string.Empty;
			this.ParameterDescription = string.Empty;
		}

		/// <summary>
		/// Type of command line argument
		/// </summary>
        public ArgumentType Type { get; }

        /// <summary>
		/// Switch used to represent the argument.
		/// </summary>
        public string Name { get; set; }

        /// <summary>
		/// Shortcut switch used to represent the argument.
		/// </summary>
        public string Shortcut { get; set; }

        /// <summary>
		/// Description of the command line argument.
		/// </summary>
        public string Description { get; set; }

        /// <summary>
		/// Description of the parameter.
		/// </summary>
        public string ParameterDescription { get; set; }

        public string SampleUsageValue { get; set; }
    }
}
