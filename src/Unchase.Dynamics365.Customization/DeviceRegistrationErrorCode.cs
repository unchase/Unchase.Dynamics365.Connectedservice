namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Indicates an error during registration
	/// </summary>
    public enum DeviceRegistrationErrorCode
	{
		/// <summary>
		/// Unspecified or Unknown Error occurred
		/// </summary>
        Unknown,

		/// <summary>
		/// Interface Disabled
		/// </summary>
        InterfaceDisabled,

		/// <summary>
		/// Invalid Request Format
		/// </summary>
        InvalidRequestFormat = 3,

		/// <summary>
		/// Unknown Client Version
		/// </summary>
        UnknownClientVersion,

		/// <summary>
		/// Blank Password
		/// </summary>
        BlankPassword = 6,

		/// <summary>
		/// Missing Device User Name or Password
		/// </summary>
        MissingDeviceUserNameOrPassword,

		/// <summary>
		/// Invalid Parameter Syntax
		/// </summary>
        InvalidParameterSyntax,

		/// <summary>
		/// Internal Error
		/// </summary>
        InternalError = 11,

		/// <summary>
		/// Device Already Exists
		/// </summary>
        DeviceAlreadyExists = 13
	}
}
