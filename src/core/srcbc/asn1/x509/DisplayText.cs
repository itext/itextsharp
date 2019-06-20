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

namespace Org.BouncyCastle.Asn1.X509
{
	/**
	 * <code>DisplayText</code> class, used in
	 * <code>CertificatePolicies</code> X509 V3 extensions (in policy qualifiers).
	 *
	 * <p>It stores a string in a chosen encoding.
	 * <pre>
	 * DisplayText ::= CHOICE {
	 *      ia5String        IA5String      (SIZE (1..200)),
	 *      visibleString    VisibleString  (SIZE (1..200)),
	 *      bmpString        BMPString      (SIZE (1..200)),
	 *      utf8String       UTF8String     (SIZE (1..200)) }
	 * </pre></p>
	 * @see PolicyQualifierInfo
	 * @see PolicyInformation
	 */
	public class DisplayText
		: Asn1Encodable, IAsn1Choice
	{
		/**
		 * Constant corresponding to ia5String encoding.
		 *
		 */
		public const int ContentTypeIA5String = 0;
		/**
		 * Constant corresponding to bmpString encoding.
		 *
		 */
		public const int ContentTypeBmpString = 1;
		/**
		 * Constant corresponding to utf8String encoding.
		 *
		 */
		public const int ContentTypeUtf8String = 2;
		/**
		 * Constant corresponding to visibleString encoding.
		 *
		 */
		public const int ContentTypeVisibleString = 3;
		/**
		 * Describe constant <code>DisplayTextMaximumSize</code> here.
		 *
		 */
		public const int DisplayTextMaximumSize = 200;

		internal readonly int contentType;
		internal readonly IAsn1String contents;

		/**
		 * Creates a new <code>DisplayText</code> instance.
		 *
		 * @param type the desired encoding type for the text.
		 * @param text the text to store. Strings longer than 200
		 * characters are truncated.
		 */
		public DisplayText(
			int		type,
			string	text)
		{
			if (text.Length > DisplayTextMaximumSize)
			{
				// RFC3280 limits these strings to 200 chars
				// truncate the string
				text = text.Substring(0, DisplayTextMaximumSize);
			}

			contentType = type;
			switch (type)
			{
				case ContentTypeIA5String:
					contents = (IAsn1String)new DerIA5String (text);
					break;
				case ContentTypeUtf8String:
					contents = (IAsn1String)new DerUtf8String(text);
					break;
				case ContentTypeVisibleString:
					contents = (IAsn1String)new DerVisibleString(text);
					break;
				case ContentTypeBmpString:
					contents = (IAsn1String)new DerBmpString(text);
					break;
				default:
					contents = (IAsn1String)new DerUtf8String(text);
					break;
			}
		}

//		/**
//		 * return true if the passed in string can be represented without
//		 * loss as a PrintableString, false otherwise.
//		 */
//		private bool CanBePrintable(
//			string str)
//		{
//			for (int i = str.Length - 1; i >= 0; i--)
//			{
//				if (str[i] > 0x007f)
//				{
//					return false;
//				}
//			}
//
//			return true;
//		}

		/**
		 * Creates a new <code>DisplayText</code> instance.
		 *
		 * @param text the text to encapsulate. Strings longer than 200
		 * characters are truncated.
		 */
		public DisplayText(
			string text)
		{
			// by default use UTF8String
			if (text.Length > DisplayTextMaximumSize)
			{
				text = text.Substring(0, DisplayTextMaximumSize);
			}

			contentType = ContentTypeUtf8String;
			contents = new DerUtf8String(text);
		}

		/**
		 * Creates a new <code>DisplayText</code> instance.
		 * <p>Useful when reading back a <code>DisplayText</code> class
		 * from it's Asn1Encodable form.</p>
		 *
		 * @param contents an <code>Asn1Encodable</code> instance.
		 */
		public DisplayText(
			IAsn1String contents)
		{
			this.contents = contents;
		}

		public static DisplayText GetInstance(
			object obj)
		{
			if (obj is IAsn1String)
			{
				return new DisplayText((IAsn1String) obj);
			}

			if (obj is DisplayText)
			{
				return (DisplayText) obj;
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public override Asn1Object ToAsn1Object()
		{
			return (Asn1Object) contents;
		}

		/**
		 * Returns the stored <code>string</code> object.
		 *
		 * @return the stored text as a <code>string</code>.
		 */
		public string GetString()
		{
			return contents.GetString();
		}
	}
}
