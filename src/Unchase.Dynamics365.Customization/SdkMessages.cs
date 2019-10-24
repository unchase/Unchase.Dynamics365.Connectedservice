using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// SDK Messages
	/// </summary>
    public sealed class SdkMessages
	{
        private SdkMessages() : this(null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="messageCollection">SDK message collection</param>
        public SdkMessages(Dictionary<Guid, SdkMessage> messageCollection)
		{
			this.MessageCollection = (messageCollection ?? new Dictionary<Guid, SdkMessage>());
		}

		/// <summary>
		/// Gets the message collection
		/// </summary>
        public Dictionary<Guid, SdkMessage> MessageCollection { get; }

        private void Fill(ResultSet resultSet)
		{
			if (resultSet.Results == null)
			{
				return;
			}
			foreach (var result in resultSet.Results)
			{
				SdkMessage sdkMessage;
				if (result.SdkMessageId != Guid.Empty && !this.MessageCollection.ContainsKey(result.SdkMessageId))
				{
					sdkMessage = new SdkMessage(result.SdkMessageId, result.Name, result.IsPrivate, result.CustomizationLevel);
					this.MessageCollection.Add(result.SdkMessageId, sdkMessage);
				}
				sdkMessage = this.MessageCollection[result.SdkMessageId];
				sdkMessage.Fill(result);
			}
		}

		/// <summary>
		/// Gets the MessagePagingInfo for a given collection SDK messages
		/// </summary>
        public static MessagePagingInfo FromFetchResult(SdkMessages messages, string xml)
		{
			ResultSet resultSet;
			using (var stringReader = new StringReader(xml))
			{
				resultSet = (new XmlSerializer(typeof(ResultSet), string.Empty).Deserialize(stringReader) as ResultSet);
			}
			messages.Fill(resultSet);
			return MessagePagingInfo.FromResultSet(resultSet);
		}
    }
}
