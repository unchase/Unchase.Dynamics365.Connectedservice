using System;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// An SDK message request field
	/// </summary>
    public sealed class SdkMessageRequestField
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="request">SDK message request</param>
		/// <param name="index">Request field index</param>
		/// <param name="name">Request field name</param>
		/// <param name="clrFormatter">Request field CLR formatter</param>
		/// <param name="isOptional">Whether the request field is optional</param>
        public SdkMessageRequestField(SdkMessageRequest request, int index, string name, string clrFormatter, bool isOptional)
		{
			this.Request = request;
			this.CLRFormatter = clrFormatter;
			this.Name = name;
			this.Index = index;
			this.IsOptional = isOptional;
		}

		/// <summary>
		/// Gets the SDK message request
		/// </summary>
        public SdkMessageRequest Request { get; }

        /// <summary>
		/// Gets the message request field index
		/// </summary>
        public int Index { get; }

        /// <summary>
		/// Gets the message request field name
		/// </summary>
        public string Name { get; }

        /// <summary>
		/// Gets the message request field CLR formatter
		/// </summary>
        public string CLRFormatter { get; }

        /// <summary>
		/// Gets whether the message field is optional
		/// </summary>
        public bool IsOptional { get; }

        /// <summary>
		/// Gets whether the message request field is generic
		/// </summary>
        public bool IsGeneric => string.Equals(this.CLRFormatter, "Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk", StringComparison.Ordinal) && this.Request.MessagePair.Message.SdkMessageFilters.Count > 1;

        private const string EntityTypeName = "Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk";
    }
}
