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

namespace Org.BouncyCastle.X509.Store
{
	public sealed class X509StoreFactory
	{
		private X509StoreFactory()
		{
		}

		public static IX509Store Create(
			string					type,
			IX509StoreParameters	parameters)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			string[] parts = Platform.ToUpperInvariant(type).Split('/');

            if (parts.Length < 2)
				throw new ArgumentException("type");

			if (parts[1] != "COLLECTION")
				throw new NoSuchStoreException("X.509 store type '" + type + "' not available.");

			X509CollectionStoreParameters p = (X509CollectionStoreParameters) parameters;
			ICollection coll = p.GetCollection();

			switch (parts[0])
			{
				case "ATTRIBUTECERTIFICATE":
					checkCorrectType(coll, typeof(IX509AttributeCertificate));
					break;
				case "CERTIFICATE":
					checkCorrectType(coll, typeof(X509Certificate));
					break;
				case "CERTIFICATEPAIR":
					checkCorrectType(coll, typeof(X509CertificatePair));
					break;
				case "CRL":
					checkCorrectType(coll, typeof(X509Crl));
					break;
				default:
					throw new NoSuchStoreException("X.509 store type '" + type + "' not available.");
			}

			return new X509CollectionStore(coll);
		}

		private static void checkCorrectType(ICollection coll, Type t)
		{
			foreach (object o in coll)
			{
				if (!t.IsInstanceOfType(o))
					throw new InvalidCastException("Can't cast object to type: " + t.FullName);
			}
		}
	}
}
