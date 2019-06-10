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

namespace Org.BouncyCastle.Asn1.X500
{
	public class DirectoryString
		: Asn1Encodable, IAsn1Choice, IAsn1String
	{
		private readonly DerStringBase str;

		public static DirectoryString GetInstance(
			object obj)
		{
			if (obj is DirectoryString)
			{
				return (DirectoryString) obj;
			}

			if (obj is DerStringBase)
			{
				if (obj is DerT61String
					|| obj is DerPrintableString
					|| obj is DerUniversalString
					|| obj is DerUtf8String
					|| obj is DerBmpString)
				{
					return new DirectoryString((DerStringBase) obj);
				}
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public static DirectoryString GetInstance(
			Asn1TaggedObject	obj,
			bool				isExplicit)
		{
			if (!isExplicit)
				throw new ArgumentException("choice item must be explicitly tagged");

			return GetInstance(obj.GetObject());
		}

		private DirectoryString(
			DerStringBase str)
		{
			this.str = str;
		}

		public DirectoryString(
			string str)
		{
			this.str = new DerUtf8String(str);
		}

		public string GetString()
		{
			return str.GetString();
		}

		/**
		 * <pre>
		 *  DirectoryString ::= CHOICE {
		 *    teletexString               TeletexString (SIZE (1..MAX)),
		 *    printableString             PrintableString (SIZE (1..MAX)),
		 *    universalString             UniversalString (SIZE (1..MAX)),
		 *    utf8String                  UTF8String (SIZE (1..MAX)),
		 *    bmpString                   BMPString (SIZE (1..MAX))  }
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			return str.ToAsn1Object();
		}
	}
}
