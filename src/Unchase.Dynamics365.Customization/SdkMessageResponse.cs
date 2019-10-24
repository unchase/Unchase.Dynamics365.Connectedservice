using System;
using System.Collections.Generic;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// An SDK message response
	/// </summary>
    public sealed class SdkMessageResponse
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id">Message response id</param>
        public SdkMessageResponse(Guid id)
		{
			this.Id = id;
			this.ResponseFields = new Dictionary<int, SdkMessageResponseField>();
		}

		/// <summary>
		/// Gets the message response id
		/// </summary>
        public Guid Id { get; }

        /// <summary>
		/// Gets the message response fields
		/// </summary>
        public Dictionary<int, SdkMessageResponseField> ResponseFields { get; }

        internal void Fill(Result result)
		{
			if (result.SdkMessageResponseFieldPosition == null)
			{
				return;
			}
			if (!this.ResponseFields.ContainsKey(result.SdkMessageResponseFieldPosition.Value))
			{
				var value = new SdkMessageResponseField(result.SdkMessageResponseFieldPosition.Value, result.SdkMessageResponseFieldName, result.SdkMessageResponseFieldClrFormatter, result.SdkMessageResponseFieldValue);
				this.ResponseFields.Add(result.SdkMessageResponseFieldPosition.Value, value);
			}
			var sdkMessageResponseField = this.ResponseFields[result.SdkMessageResponseFieldPosition.Value];
		}
    }
}
