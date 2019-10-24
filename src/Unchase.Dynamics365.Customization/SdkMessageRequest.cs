using System;
using System.Collections.Generic;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// An SDK message request
	/// </summary>
    public sealed class SdkMessageRequest
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">SDK Message</param>
		/// <param name="id">Message request id</param>
		/// <param name="name">Message request name</param>
        public SdkMessageRequest(SdkMessagePair message, Guid id, string name)
		{
			this.Id = id;
			this.Name = name;
			this.MessagePair = message;
			this.RequestFields = new Dictionary<int, SdkMessageRequestField>();
		}

		/// <summary>
		/// Gets the message request id
		/// </summary>
        public Guid Id { get; }

        /// <summary>
		/// Gets the message pair of the request
		/// </summary>
        public SdkMessagePair MessagePair { get; }

        /// <summary>
		/// Gets the message request name
		/// </summary>
        public string Name { get; }

        /// <summary>
		/// Gets a dictionary of message request fields
		/// </summary>
        public Dictionary<int, SdkMessageRequestField> RequestFields { get; }

        internal void Fill(Result result)
		{
			if (result.SdkMessageRequestFieldPosition == null)
			{
				return;
			}
			if (!this.RequestFields.ContainsKey(result.SdkMessageRequestFieldPosition.Value))
			{
				var value = new SdkMessageRequestField(this, result.SdkMessageRequestFieldPosition.Value, result.SdkMessageRequestFieldName, result.SdkMessageRequestFieldClrParser, result.SdkMessageRequestFieldIsOptional);
				this.RequestFields.Add(result.SdkMessageRequestFieldPosition.Value, value);
			}
			var sdkMessageRequestField = this.RequestFields[result.SdkMessageRequestFieldPosition.Value];
		}
    }
}
