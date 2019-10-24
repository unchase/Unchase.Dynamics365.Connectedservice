using System.ComponentModel;
using System.Xml.Serialization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Device registration response
	/// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("DeviceAddResponse")]
	public sealed class DeviceRegistrationResponse
	{
		/// <summary>
		/// Gets or sets whether the device registration was successful
		/// </summary>
        [XmlElement("success")]
		public bool IsSuccess { get; set; }

		/// <summary>
		/// Gets or sets the device registration puid
		/// </summary>
        [XmlElement("puid")]
		public string Puid { get; set; }

		/// <summary>
		/// Gets or sets the device registration error
		/// </summary>
        [XmlElement("Error")]
		public DeviceRegistrationResponseError Error { get; set; }

		/// <summary>
		/// Gets or sets the device registration error sub code
		/// </summary>
        [XmlElement("ErrorSubcode")]
		public string ErrorSubCode { get; set; }
	}
}
