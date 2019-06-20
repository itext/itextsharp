/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;
using System.Collections;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms
{
	public class RecipientInformationStore
	{
		private readonly IList all; //ArrayList[RecipientInformation]
		private readonly IDictionary table = Platform.CreateHashtable(); // Hashtable[RecipientID, ArrayList[RecipientInformation]]

		public RecipientInformationStore(
			ICollection recipientInfos)
		{
			foreach (RecipientInformation recipientInformation in recipientInfos)
			{
				RecipientID rid = recipientInformation.RecipientID;
                IList list = (IList)table[rid];

				if (list == null)
				{
					table[rid] = list = Platform.CreateArrayList(1);
				}

				list.Add(recipientInformation);
			}

            this.all = Platform.CreateArrayList(recipientInfos);
		}

		public RecipientInformation this[RecipientID selector]
		{
			get { return GetFirstRecipient(selector); }
		}

		/**
		* Return the first RecipientInformation object that matches the
		* passed in selector. Null if there are no matches.
		*
		* @param selector to identify a recipient
		* @return a single RecipientInformation object. Null if none matches.
		*/
		public RecipientInformation GetFirstRecipient(
			RecipientID selector)
		{
			IList list = (IList) table[selector];

			return list == null ? null : (RecipientInformation) list[0];
		}

		/**
		* Return the number of recipients in the collection.
		*
		* @return number of recipients identified.
		*/
		public int Count
		{
			get { return all.Count; }
		}

		/**
		* Return all recipients in the collection
		*
		* @return a collection of recipients.
		*/
		public ICollection GetRecipients()
		{
			return Platform.CreateArrayList(all);
		}

		/**
		* Return possible empty collection with recipients matching the passed in RecipientID
		*
		* @param selector a recipient id to select against.
		* @return a collection of RecipientInformation objects.
		*/
		public ICollection GetRecipients(
			RecipientID selector)
		{
            IList list = (IList)table[selector];

            return list == null ? Platform.CreateArrayList() : Platform.CreateArrayList(list);
		}
	}
}
