using System;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// An SDK message pair
	/// </summary>
    public sealed class SdkMessagePair
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">SDK message</param>
		/// <param name="id">Message pair id</param>
		/// <param name="messageNamespace">Message namespace</param>
        public SdkMessagePair(SdkMessage message, Guid id, string messageNamespace)
		{
			this.Message = message;
			this.Id = id;
			this.MessageNamespace = messageNamespace;
		}

		/// <summary>
		/// Gets the message pair id
		/// </summary>
        public Guid Id { get; }

        /// <summary>
		/// Gets the message namespace
		/// </summary>
        public string MessageNamespace { get; }

        /// <summary>
		/// Gets or sets the message
		/// </summary>
        public SdkMessage Message { get; set; }

        /// <summary>
		/// Gets or sets the message request
		/// </summary>
        public SdkMessageRequest Request { get; set; }

        /// <summary>
		/// Gets or sets the message response
		/// </summary>
        public SdkMessageResponse Response { get; set; }

        internal void Fill(Result result)
		{
			if (result.SdkMessageRequestId != Guid.Empty)
			{
				if (this.Request == null)
				{
					this.Request = new SdkMessageRequest(this, result.SdkMessageRequestId, result.SdkMessageRequestName);
				}
				this.Request.Fill(result);
			}
			if (result.SdkMessageResponseId != Guid.Empty)
			{
				if (this.Response == null)
				{
					this.Response = new SdkMessageResponse(result.SdkMessageResponseId);
				}
				this.Response.Fill(result);
			}
		}
    }
}
