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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.X509
{
	public abstract class X509ExtensionBase
		: IX509Extension
	{
		protected abstract X509Extensions GetX509Extensions();

		protected virtual ISet GetExtensionOids(
			bool critical)
		{
			X509Extensions extensions = GetX509Extensions();
			if (extensions != null)
			{
				HashSet set = new HashSet();
				foreach (DerObjectIdentifier oid in extensions.ExtensionOids)
				{
					X509Extension ext = extensions.GetExtension(oid);
					if (ext.IsCritical == critical)
					{
						set.Add(oid.Id);
					}
				}

				return set;
			}

			return null;
		}

		/// <summary>
		/// Get non critical extensions.
		/// </summary>
		/// <returns>A set of non critical extension oids.</returns>
		public virtual ISet GetNonCriticalExtensionOids()
		{
			return GetExtensionOids(false);
		}

		/// <summary>
		/// Get any critical extensions.
		/// </summary>
		/// <returns>A sorted list of critical entension.</returns>
		public virtual ISet GetCriticalExtensionOids()
		{
			return GetExtensionOids(true);
		}

		/// <summary>
		/// Get the value of a given extension.
		/// </summary>
		/// <param name="oid">The object ID of the extension. </param>
		/// <returns>An Asn1OctetString object if that extension is found or null if not.</returns>
		[Obsolete("Use version taking a DerObjectIdentifier instead")]
		public Asn1OctetString GetExtensionValue(
			string oid)
		{
			return GetExtensionValue(new DerObjectIdentifier(oid));
		}

		public virtual Asn1OctetString GetExtensionValue(
			DerObjectIdentifier oid)
		{
			X509Extensions exts = GetX509Extensions();
			if (exts != null)
			{
				X509Extension ext = exts.GetExtension(oid);
				if (ext != null)
				{
					return ext.Value;
				}
			}

			return null;
		}
	}
}
