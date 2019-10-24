using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility.Enums;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
	/// <summary>
	/// Indicates that Device Registration failed
	/// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
	[Serializable]
	internal sealed class DeviceRegistrationFailedException : Exception
	{
		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
        internal DeviceRegistrationFailedException()
		{
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="code">Error code that occurred</param>
		/// <param name="subCode">Subcode that occurred</param>
        internal DeviceRegistrationFailedException(DeviceRegistrationErrorCode code, string subCode) : this(code, subCode, null)
		{
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="code">Error code that occurred</param>
		/// <param name="subCode">Subcode that occurred</param>
		/// <param name="innerException">Inner exception</param>
        internal DeviceRegistrationFailedException(DeviceRegistrationErrorCode code, string subCode, Exception innerException) : base(code.ToString() + ": " + subCode, innerException)
		{
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="si"></param>
		/// <param name="sc"></param>
        private DeviceRegistrationFailedException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}
	}
}
