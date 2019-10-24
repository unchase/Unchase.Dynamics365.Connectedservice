namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Type of code to generate
	/// </summary>
    public enum CodeGenerationType
	{
		/// <summary>
		/// Type Class
		/// </summary>
        Class,

		/// <summary>
		/// Type Enum
		/// </summary>
        Enum,

		/// <summary>
		/// Type Field
		/// </summary>
        Field,

		/// <summary>
		/// Type Method
		/// </summary>
        Method,

		/// <summary>
		/// Type Property
		/// </summary>
        Property,

		/// <summary>
		/// Type Struct
		/// </summary>
        Struct,

		/// <summary>
		/// Type Parameter
		/// </summary>
        Parameter
	}
}
