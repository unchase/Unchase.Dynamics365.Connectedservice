﻿using Microsoft.Xrm.Sdk.Query;
using System.Xml.Linq;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Extensions for Microsoft.Xrm.Sdk.Query types.
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Universal method to set Query paging parameters to next page.
        /// </summary>
        public static void NextPage(this QueryBase query, string pagingCookie)
        {
            if (query is QueryExpression qe)
            {
                qe.NextPage(pagingCookie);
            }
            else if (query is QueryByAttribute qa)
            {
                qa.NextPage(pagingCookie);
            }
            else if (query is FetchExpression fe)
            {
                fe.NextPage(pagingCookie);
            }
        }


        /// <summary>
        /// Sets QueryExpression paging parameters to next page.
        /// </summary>
        public static void NextPage(this QueryExpression query, string pagingCookie)
        {
            query.PageInfo.PageNumber++;
            query.PageInfo.PagingCookie = pagingCookie;
        }


        /// <summary>
        /// Sets QueryByAttribute paging parameters to next page.
        /// </summary>
        public static void NextPage(this QueryByAttribute query, string pagingCookie)
        {
            query.PageInfo.PageNumber++;
            query.PageInfo.PagingCookie = pagingCookie;
        }


        /// <summary>
        /// Sets FetchExpression paging parameters to next page.
        /// </summary>
        public static void NextPage(this FetchExpression query, string pagingCookie)
        {
            XDocument xDocument = XDocument.Parse(query.Query);
            NextPage(query, xDocument, pagingCookie);
        }


        /// <summary>
        /// Internal method for better FetchExpression paging performance.
        /// </summary>
        internal static void NextPage(this FetchExpression query, XDocument xDocument, string pagingCookie)
        {
            /// No paging mean PageInfo.Page = 0
            /// for FetchXml it mean no "page" attribute or 0 value
            string page = xDocument.Root.Attribute("page")?.Value;
            int pageNumber = page != null ? int.Parse(page) : 0;

            xDocument.Root.SetAttributeValue("paging-cookie", pagingCookie);
            xDocument.Root.SetAttributeValue("page", ++pageNumber);

            query.Query = xDocument.ToString();
        }
    }
}
