using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceModel.Description;
using System.Xml.Serialization;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
	/// <summary>
	/// Management utility for the Device Id
	/// </summary>
    internal static class DeviceIdManager
	{
		/// <summary>
		/// Loads the device credentials (if they exist). If they don't
		/// </summary>
		/// <param name="applicationId">Application id</param>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
        public static ClientCredentials LoadOrRegisterDevice(Guid applicationId, Uri issuerUri)
		{
			var clientCredentials = DeviceIdManager.LoadDeviceCredentials(issuerUri) ?? DeviceIdManager.RegisterDevice(applicationId, issuerUri);
            return clientCredentials;
		}

		/// <summary>
		/// Registers the given device with Live ID
		/// </summary>
		/// <param name="applicationId">ID for the application</param>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <returns>ClientCredentials that were registered</returns>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
        public static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri)
		{
			return DeviceIdManager.RegisterDevice(applicationId, issuerUri, null, null);
		}

		/// <summary>
		/// Registers the given device with Live ID
		/// </summary>
		/// <param name="applicationId">ID for the application</param>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <param name="deviceName">Device name that should be registered</param>
		/// <param name="devicePassword">Device password that should be registered</param>
		/// <returns>ClientCredentials that were registered</returns>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
        public static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri, string deviceName, string devicePassword)
		{
			if (string.IsNullOrWhiteSpace(deviceName) != string.IsNullOrWhiteSpace(devicePassword))
			{
				throw new ArgumentNullException("deviceName", "Either deviceName/devicePassword should both be specified or they should be null.");
			}
			DeviceUserName userName;
			if (string.IsNullOrWhiteSpace(deviceName))
			{
				userName = DeviceIdManager.GenerateDeviceUserName();
			}
			else
			{
				userName = new DeviceUserName
				{
					DeviceName = deviceName,
					DecryptedPassword = devicePassword
				};
			}
			return DeviceIdManager.RegisterDevice(applicationId, issuerUri, userName);
		}

		/// <summary>
		/// Loads the device's credentials from the file system
		/// </summary>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <returns>Device Credentials (if set) or null</returns>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
        public static ClientCredentials LoadDeviceCredentials(Uri issuerUri)
		{
			var liveDevice = DeviceIdManager.ReadExistingDevice(DeviceIdManager.DiscoverEnvironment(issuerUri));
            return liveDevice?.User?.ToClientCredentials();
		}

		private static void Serialize<T>(Stream stream, T value)
		{
			var xmlSerializer = new XmlSerializer(typeof(T), string.Empty);
			var xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add(string.Empty, string.Empty);
			xmlSerializer.Serialize(stream, value, xmlSerializerNamespaces);
		}

		private static T Deserialize<T>(Stream stream)
		{
			return (T)((object)new XmlSerializer(typeof(T), string.Empty).Deserialize(stream));
		}

		private static FileInfo GetDeviceFile(string environment)
		{
			return new FileInfo(string.Format(CultureInfo.InvariantCulture, LiveIdConstants.LiveDeviceFileNameFormat, string.IsNullOrWhiteSpace(environment) ? null : ("-" + environment.ToUpperInvariant())));
		}

		private static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri, DeviceUserName userName)
		{
			var text = DeviceIdManager.DiscoverEnvironment(issuerUri);
			var liveDevice = new LiveDevice
			{
				User = userName,
				Version = 1
			};
			var registrationRequest = new DeviceRegistrationRequest(applicationId, liveDevice);
			var deviceRegistrationResponse = DeviceIdManager.ExecuteRegistrationRequest(string.Format(CultureInfo.InvariantCulture, "https://login.live{0}.com/ppsecure/DeviceAddCredential.srf", string.IsNullOrWhiteSpace(text) ? null : ("-" + text)), registrationRequest);
			if (!deviceRegistrationResponse.IsSuccess)
			{
				throw new DeviceRegistrationFailedException((Microsoft.Xrm.Tooling.Connector.DeviceRegistrationErrorCode)deviceRegistrationResponse.Error.RegistrationErrorCode, deviceRegistrationResponse.ErrorSubCode);
			}
			DeviceIdManager.WriteDevice(text, liveDevice);
			return liveDevice.User.ToClientCredentials();
		}

		private static LiveDevice ReadExistingDevice(string environment)
		{
			var deviceFile = DeviceIdManager.GetDeviceFile(environment);
			if (!deviceFile.Exists)
			{
				return null;
			}
			LiveDevice result;
			using (var fileStream = deviceFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				result = DeviceIdManager.Deserialize<LiveDevice>(fileStream);
			}
			return result;
		}

		private static void WriteDevice(string environment, LiveDevice device)
		{
			var deviceFile = DeviceIdManager.GetDeviceFile(environment);
			if (deviceFile.Directory != null && !deviceFile.Directory.Exists)
			{
				deviceFile.Directory.Create();
			}
			using (var fileStream = deviceFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				DeviceIdManager.Serialize<LiveDevice>(fileStream, device);
			}
		}

		private static DeviceRegistrationResponse ExecuteRegistrationRequest(string url, DeviceRegistrationRequest registrationRequest)
		{
			var webRequest = WebRequest.Create(url);
			webRequest.ContentType = "application/soap+xml; charset=UTF-8";
			webRequest.Method = "POST";
			webRequest.Timeout = 180000;
			using (var requestStream = webRequest.GetRequestStream())
			{
				DeviceIdManager.Serialize<DeviceRegistrationRequest>(requestStream, registrationRequest);
			}
			DeviceRegistrationResponse result;
			try
			{
				using (var response = webRequest.GetResponse())
				{
					using (var responseStream = response.GetResponseStream())
					{
						result = DeviceIdManager.Deserialize<DeviceRegistrationResponse>(responseStream);
					}
				}
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					using (var responseStream2 = ex.Response.GetResponseStream())
					{
						return DeviceIdManager.Deserialize<DeviceRegistrationResponse>(responseStream2);
					}
				}
				throw;
			}
			return result;
		}

		private static DeviceUserName GenerateDeviceUserName()
		{
			return new DeviceUserName
			{
				DeviceName = DeviceIdManager.GenerateRandomString("0123456789abcdefghijklmnopqrstuvqxyz", 24),
				DecryptedPassword = DeviceIdManager.GenerateRandomString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^*()-_=+;,./?`~", 24)
			};
		}

		private static string GenerateRandomString(string characterSet, int count)
		{
			var array = new char[count];
			var array2 = characterSet.ToCharArray();
			var randomInstance = DeviceIdManager.RandomInstance;
			lock (randomInstance)
			{
				for (var i = 0; i < count; i++)
				{
					array[i] = array2[DeviceIdManager.RandomInstance.Next(0, array2.Length)];
				}
			}
			return new string(array);
		}

		private static string DiscoverEnvironment(Uri issuerUri)
		{
			if (null == issuerUri)
			{
				return null;
			}
			if (issuerUri.Host.Length > "login.live".Length && issuerUri.Host.StartsWith("login.live", StringComparison.OrdinalIgnoreCase))
			{
				var text = issuerUri.Host.Substring("login.live".Length);
				if ('-' == text[0])
				{
					var num = text.IndexOf('.', 1);
					if (-1 != num)
					{
						return text.Substring(1, num - 1);
					}
				}
			}
			return null;
		}

		private static readonly Random RandomInstance = new Random();
	}
}
