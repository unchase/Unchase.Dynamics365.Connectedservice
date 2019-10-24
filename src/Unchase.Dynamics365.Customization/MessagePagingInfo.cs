using System;
using System.Globalization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Message paging info
	/// </summary>
    public sealed class MessagePagingInfo
	{
		/// <summary>
		/// Gets or sets the paging cookie
		/// </summary>
        public string PagingCookie { get; set; }

		/// <summary>
		/// Gets or sets whether the paging info has more records
		/// </summary>
        public bool HasMoreRecords { get; set; }

		/// <summary>
		/// Gets the message paging info from a set of message results
		/// </summary>
        public static MessagePagingInfo FromResultSet(ResultSet resultSet)
		{
			return new MessagePagingInfo
			{
				PagingCookie = resultSet.PagingCookie,
				HasMoreRecords = Convert.ToBoolean(resultSet.MoreRecords, CultureInfo.InvariantCulture)
			};
		}
	}
}
