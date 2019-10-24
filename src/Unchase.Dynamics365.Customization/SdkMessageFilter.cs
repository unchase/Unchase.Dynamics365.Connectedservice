using System;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// An SDK message filter
	/// </summary>
    public sealed class SdkMessageFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id">Message filter id</param>
        public SdkMessageFilter(Guid id)
		{
			this.Id = id;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id">Message filter id</param>
		/// <param name="primaryObjectTypeCode">Primary object type code</param>
		/// <param name="secondaryObjectTypeCode">Secondary object type code</param>
		/// <param name="isVisible">Whether the message filter is visible</param>
        public SdkMessageFilter(Guid id, int primaryObjectTypeCode, int secondaryObjectTypeCode, bool isVisible)
		{
			this.Id = id;
			this.PrimaryObjectTypeCode = primaryObjectTypeCode;
			this.SecondaryObjectTypeCode = secondaryObjectTypeCode;
			this.IsVisible = isVisible;
		}

		/// <summary>
		/// Gets the message filter id
		/// </summary>
        public Guid Id { get; }

        /// <summary>
		/// Gets or sets the message filter primary object type code
		/// </summary>
        public int PrimaryObjectTypeCode { get; private set; }

        /// <summary>
		/// Gets or sets the message secondary object type code
		/// </summary>
        public int SecondaryObjectTypeCode { get; private set; }

        /// <summary>
		/// Gets or sets whether the message filter is visible
		/// </summary>
        public bool IsVisible { get; private set; }

        internal void Fill(Result result)
		{
			this.PrimaryObjectTypeCode = result.SdkMessagePrimaryOTCFilter;
			this.SecondaryObjectTypeCode = result.SdkMessageSecondaryOTCFilter;
			this.IsVisible = false;
		}
    }
}
