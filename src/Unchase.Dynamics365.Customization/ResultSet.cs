using System;
using System.Xml.Serialization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Set of SDK message results
	/// </summary>
    [XmlType(Namespace = "")]
	[XmlRoot(ElementName = "resultset", Namespace = "")]
	[Serializable]
	public sealed class ResultSet
	{
		/// <summary>
		/// Gets or sets an array of results
		/// </summary>
        [XmlElement("result")]
		public Result[] Results
		{
			get => this._results;
            set => this._results = value;
        }

		/// <summary>
		/// Gets or sets the paging cookie
		/// </summary>
        [XmlAttribute("paging-cookie")]
		public string PagingCookie
		{
			get => this._pagingCookie;
            set => this._pagingCookie = value;
        }

		/// <summary>
		/// Gets or sets a flag for more records
		/// </summary>
        [XmlAttribute("morerecords")]
		public int MoreRecords
		{
			get => this._moreRecords;
            set => this._moreRecords = value;
        }

		private Result[] _results;

		private string _pagingCookie;

		private int _moreRecords;
	}
}
