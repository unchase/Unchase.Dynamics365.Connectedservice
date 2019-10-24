using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Live device
	/// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("Data")]
	public sealed class LiveDevice
	{
		/// <summary>
		/// Gets or sets the device version
		/// </summary>
        [XmlAttribute("version")]
		public int Version { get; set; }

		/// <summary>
		/// Gets or sets the device user name
		/// </summary>
        [XmlElement("User")]
		public DeviceUserName User { get; set; }

		/// <summary>
		/// Gets or sets the device token
		/// </summary>
        [XmlElement("Token")]
		public XmlNode Token { get; set; }

		/// <summary>
		/// Gets or sets the device token expiry
		/// </summary>
        [XmlElement("Expiry")]
		public string Expiry { get; set; }

		/// <summary>
		/// Gets or sets the device clock skew
		/// </summary>
        [XmlElement("ClockSkew")]
		public string ClockSkew { get; set; }
	}
}
