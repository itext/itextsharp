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

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class PkiFreeText
		: Asn1Encodable
	{
		internal Asn1Sequence strings;

		public static PkiFreeText GetInstance(
			Asn1TaggedObject	obj,
			bool				isExplicit)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
		}

		public static PkiFreeText GetInstance(
			object obj)
		{
			if (obj is PkiFreeText)
			{
				return (PkiFreeText)obj;
			}
			else if (obj is Asn1Sequence)
			{
				return new PkiFreeText((Asn1Sequence)obj);
			}

			throw new ArgumentException("Unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public PkiFreeText(
			Asn1Sequence seq)
		{
			foreach (object o in seq)
			{
				if (!(o is DerUtf8String))
				{
					throw new ArgumentException("attempt to insert non UTF8 STRING into PkiFreeText");
				}
			}

			this.strings = seq;
		}

		public PkiFreeText(
			DerUtf8String p)
		{
			strings = new DerSequence(p);
		}

		/**
		 * Return the number of string elements present.
		 *
		 * @return number of elements present.
		 */
		[Obsolete("Use 'Count' property instead")]
		public int Size
		{
			get { return strings.Count; }
		}

		public int Count
		{
			get { return strings.Count; }
		}

		/**
		 * Return the UTF8STRING at index.
		 *
		 * @param index index of the string of interest
		 * @return the string at index.
		 */
		public DerUtf8String this[int index]
		{
			get { return (DerUtf8String) strings[index]; }
		}

		[Obsolete("Use 'object[index]' syntax instead")]
		public DerUtf8String GetStringAt(
			int index)
		{
			return this[index];
		}

		/**
		 * <pre>
		 * PkiFreeText ::= SEQUENCE SIZE (1..MAX) OF UTF8String
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			return strings;
		}
	}
}
