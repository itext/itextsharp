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
using System.IO;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms
{
    public class SignerInformationStore
    {
		private readonly IList all; //ArrayList[SignerInformation]
		private readonly IDictionary table = Platform.CreateHashtable(); // Hashtable[SignerID, ArrayList[SignerInformation]]

		public SignerInformationStore(
            ICollection signerInfos)
        {
            foreach (SignerInformation signer in signerInfos)
            {
                SignerID sid = signer.SignerID;
                IList list = (IList)table[sid];

				if (list == null)
				{
					table[sid] = list = Platform.CreateArrayList(1);
				}

				list.Add(signer);
            }

            this.all = Platform.CreateArrayList(signerInfos);
        }

        /**
        * Return the first SignerInformation object that matches the
        * passed in selector. Null if there are no matches.
        *
        * @param selector to identify a signer
        * @return a single SignerInformation object. Null if none matches.
        */
        public SignerInformation GetFirstSigner(
            SignerID selector)
        {
			IList list = (IList) table[selector];

			return list == null ? null : (SignerInformation) list[0];
        }

		/// <summary>The number of signers in the collection.</summary>
		public int Count
        {
			get { return all.Count; }
        }

		/// <returns>An ICollection of all signers in the collection</returns>
        public ICollection GetSigners()
        {
            return Platform.CreateArrayList(all);
        }

		/**
        * Return possible empty collection with signers matching the passed in SignerID
        *
        * @param selector a signer id to select against.
        * @return a collection of SignerInformation objects.
        */
        public ICollection GetSigners(
            SignerID selector)
        {
			IList list = (IList) table[selector];

            return list == null ? Platform.CreateArrayList() : Platform.CreateArrayList(list);
        }
    }
}
