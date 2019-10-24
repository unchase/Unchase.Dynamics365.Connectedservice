using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Device registration response error
	/// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("Error")]
	public sealed class DeviceRegistrationResponseError
	{
		/// <summary>
		/// Gets or sets the device registration error code
		/// </summary>
        [XmlAttribute("Code")]
		public string Code
		{
			get => this._code;
            set
			{
				this._code = value;
                if (!string.IsNullOrEmpty(value) && value.StartsWith("dc", StringComparison.Ordinal) && int.TryParse(value.Substring(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var num) && Enum.IsDefined(typeof(DeviceRegistrationErrorCode), num))
				{
					this.RegistrationErrorCode = (DeviceRegistrationErrorCode)Enum.ToObject(typeof(DeviceRegistrationErrorCode), num);
				}
			}
		}

		[XmlIgnore]
		public DeviceRegistrationErrorCode RegistrationErrorCode { get; private set; }

		private string _code;
	}
}
