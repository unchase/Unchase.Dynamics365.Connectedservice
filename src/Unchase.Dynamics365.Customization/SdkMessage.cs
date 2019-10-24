using System;
using System.Collections.Generic;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// An SDK Message
	/// </summary>
    public sealed class SdkMessage
	{
		/// <summary>
		/// Default constructor
		/// </summary>
        public SdkMessage(Guid id, string name, bool isPrivate) : this(id, name, isPrivate, 0)
		{
		}

		internal SdkMessage(Guid id, string name, bool isPrivate, byte customizationLevel)
		{
			this.Id = id;
			this.IsPrivate = isPrivate;
			this.Name = name;
			this.IsCustomAction = (customizationLevel > 0);
			this._sdkMessagePairs = new Dictionary<Guid, SdkMessagePair>();
			this.SdkMessageFilters = new Dictionary<Guid, SdkMessageFilter>();
		}

		/// <summary>
		/// Gets the SDK message name
		/// </summary>
        public string Name { get; }

        /// <summary>
		/// Gets the SDK message id
		/// </summary>
        public Guid Id { get; }

        /// <summary>
		/// Gets whether the SDK message is private
		/// </summary>
        public bool IsPrivate { get; }

        /// <summary>
		/// Gets whether the SDK message is a custom action
		/// </summary>
        public bool IsCustomAction { get; }

        /// <summary>
		/// Gets a dictionary of message pairs
		/// </summary>
        public Dictionary<Guid, SdkMessagePair> SdkMessagePairs => this._sdkMessagePairs;

        /// <summary>
		/// Gets a dictionary of message filters
		/// </summary>
        public Dictionary<Guid, SdkMessageFilter> SdkMessageFilters { get; }

        /// <summary>
		/// Fills an SDK message from a given result
		/// </summary>
        internal void Fill(Result result)
		{
			SdkMessagePair sdkMessagePair;
			if (!this.SdkMessagePairs.ContainsKey(result.SdkMessagePairId))
			{
				sdkMessagePair = new SdkMessagePair(this, result.SdkMessagePairId, result.SdkMessagePairNamespace);
				this._sdkMessagePairs.Add(sdkMessagePair.Id, sdkMessagePair);
			}
			sdkMessagePair = this.SdkMessagePairs[result.SdkMessagePairId];
			sdkMessagePair.Fill(result);
			SdkMessageFilter sdkMessageFilter;
			if (!this.SdkMessageFilters.ContainsKey(result.SdkMessageFilterId))
			{
				sdkMessageFilter = new SdkMessageFilter(result.SdkMessageFilterId);
				this.SdkMessageFilters.Add(result.SdkMessageFilterId, sdkMessageFilter);
			}
			sdkMessageFilter = this.SdkMessageFilters[result.SdkMessageFilterId];
			sdkMessageFilter.Fill(result);
		}

        private readonly Dictionary<Guid, SdkMessagePair> _sdkMessagePairs;
    }
}
