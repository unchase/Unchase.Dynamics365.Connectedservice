using Microsoft.Xrm.Tooling.Connector;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Device requestration request
	/// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("DeviceAddRequest")]
	public sealed class DeviceRegistrationRequest
	{
		/// <summary>
		/// Default constructor
		/// </summary>
        public DeviceRegistrationRequest()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="applicationId">Application id</param>
		/// <param name="device">Device to register</param>
        public DeviceRegistrationRequest(Guid applicationId, LiveDevice device) : this()
		{
			if (device == null)
			{
				throw new ArgumentNullException("device");
			}
			this.ClientInfo = new DeviceRegistrationClientInfo
			{
				ApplicationId = applicationId,
				Version = "1.0"
			};
			this.Authentication = new DeviceRegistrationAuthentication
			{
				MemberName = device.User.DeviceId,
				Password = device.User.DecryptedPassword
			};
		}

		/// <summary>
		/// Gets or sets the device registration client info
		/// </summary>
        [XmlElement("ClientInfo")]
		public DeviceRegistrationClientInfo ClientInfo { get; set; }

		/// <summary>
		/// Gets or sets the device registration authentication
		/// </summary>
        [XmlElement("Authentication")]
		public DeviceRegistrationAuthentication Authentication { get; set; }
	}
}
