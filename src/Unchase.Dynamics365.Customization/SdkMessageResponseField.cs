namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// An SDK message response field
	/// </summary>
    public sealed class SdkMessageResponseField
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="index">Field index</param>
		/// <param name="name">Field name</param>
		/// <param name="clrFormatter">Field CLR formatter</param>
		/// <param name="value">Field value</param>
        public SdkMessageResponseField(int index, string name, string clrFormatter, string value)
		{
			this.CLRFormatter = clrFormatter;
			this.Index = index;
			this.Name = name;
			this.Value = value;
		}

		/// <summary>
		/// Gets the message response field index
		/// </summary>
        public int Index { get; }

        /// <summary>
		/// Gets the message response field name
		/// </summary>
        public string Name { get; }

        /// <summary>
		/// Gets the message response field CLR formatter
		/// </summary>
        public string CLRFormatter { get; }

        /// <summary>
		/// Gets the message response field value
		/// </summary>
        public string Value { get; }
    }
}
