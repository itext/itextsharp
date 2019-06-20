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

namespace Org.BouncyCastle.Asn1.X509
{
	/// <remarks>Generator for X.509 extensions</remarks>
	public class X509ExtensionsGenerator
	{
		private IDictionary extensions = Platform.CreateHashtable();
        private IList extOrdering = Platform.CreateArrayList();

		/// <summary>Reset the generator</summary>
		public void Reset()
		{
            extensions = Platform.CreateHashtable();
            extOrdering = Platform.CreateArrayList();
		}

		/// <summary>
		/// Add an extension with the given oid and the passed in value to be included
		/// in the OCTET STRING associated with the extension.
		/// </summary>
		/// <param name="oid">OID for the extension.</param>
		/// <param name="critical">True if critical, false otherwise.</param>
		/// <param name="extValue">The ASN.1 object to be included in the extension.</param>
		public void AddExtension(
			DerObjectIdentifier	oid,
			bool				critical,
			Asn1Encodable		extValue)
		{
			byte[] encoded;
			try
			{
				encoded = extValue.GetDerEncoded();
			}
			catch (Exception e)
			{
				throw new ArgumentException("error encoding value: " + e);
			}

			this.AddExtension(oid, critical, encoded);
		}

		/// <summary>
		/// Add an extension with the given oid and the passed in byte array to be wrapped
		/// in the OCTET STRING associated with the extension.
		/// </summary>
		/// <param name="oid">OID for the extension.</param>
		/// <param name="critical">True if critical, false otherwise.</param>
		/// <param name="extValue">The byte array to be wrapped.</param>
		public void AddExtension(
			DerObjectIdentifier	oid,
			bool				critical,
			byte[]				extValue)
		{
			if (extensions.Contains(oid))
			{
				throw new ArgumentException("extension " + oid + " already added");
			}

			extOrdering.Add(oid);
			extensions.Add(oid, new X509Extension(critical, new DerOctetString(extValue)));
		}

		/// <summary>Return true if there are no extension present in this generator.</summary>
		/// <returns>True if empty, false otherwise</returns>
		public bool IsEmpty
		{
			get { return extOrdering.Count < 1; }
		}

		/// <summary>Generate an X509Extensions object based on the current state of the generator.</summary>
		/// <returns>An <c>X509Extensions</c> object</returns>
		public X509Extensions Generate()
		{
			return new X509Extensions(extOrdering, extensions);
		}
	}
}
