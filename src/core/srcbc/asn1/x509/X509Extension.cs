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
     * an object for the elements in the X.509 V3 extension block.
     */
    public class X509Extension
    {
        internal bool				critical;
        internal Asn1OctetString	value;

		public X509Extension(
            DerBoolean		critical,
            Asn1OctetString	value)
        {
            if (critical == null)
            {
                throw new ArgumentNullException("critical");
            }

			this.critical = critical.IsTrue;
            this.value = value;
        }

		public X509Extension(
            bool			critical,
            Asn1OctetString	value)
        {
            this.critical = critical;
            this.value = value;
        }

		public bool IsCritical { get { return critical; } }

		public Asn1OctetString Value { get { return value; } }

		public Asn1Encodable GetParsedValue()
		{
			return ConvertValueToObject(this);
		}

		public override int GetHashCode()
        {
			int vh = this.Value.GetHashCode();

			return IsCritical ? vh : ~vh;
        }

		public override bool Equals(
            object obj)
        {
            X509Extension other = obj as X509Extension;
            if (other == null)
            {
                return false;
            }

			return Value.Equals(other.Value) && IsCritical == other.IsCritical;
        }

		/// <sumary>Convert the value of the passed in extension to an object.</sumary>
		/// <param name="ext">The extension to parse.</param>
		/// <returns>The object the value string contains.</returns>
		/// <exception cref="ArgumentException">If conversion is not possible.</exception>
		public static Asn1Object ConvertValueToObject(
			X509Extension ext)
		{
			try
			{
				return Asn1Object.FromByteArray(ext.Value.GetOctets());
			}
			catch (Exception e)
			{
				throw new ArgumentException("can't convert extension", e);
			}
		}
	}
}
