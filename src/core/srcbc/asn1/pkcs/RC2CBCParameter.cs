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
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs
{
    public class RC2CbcParameter
        : Asn1Encodable
    {
        internal DerInteger			version;
        internal Asn1OctetString	iv;

		public static RC2CbcParameter GetInstance(
            object obj)
        {
            if (obj is Asn1Sequence)
            {
                return new RC2CbcParameter((Asn1Sequence) obj);
            }

			throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
		}

		public RC2CbcParameter(
            byte[] iv)
        {
            this.iv = new DerOctetString(iv);
        }

		public RC2CbcParameter(
            int		parameterVersion,
            byte[]	iv)
        {
            this.version = new DerInteger(parameterVersion);
            this.iv = new DerOctetString(iv);
        }

		private RC2CbcParameter(
            Asn1Sequence seq)
        {
            if (seq.Count == 1)
            {
                iv = (Asn1OctetString)seq[0];
            }
            else
            {
                version = (DerInteger)seq[0];
                iv = (Asn1OctetString)seq[1];
            }
        }

		public BigInteger RC2ParameterVersion
        {
            get
            {
				return version == null ? null : version.Value;
            }
        }

		public byte[] GetIV()
        {
			return Arrays.Clone(iv.GetOctets());
        }

		public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			if (version != null)
            {
                v.Add(version);
            }

			v.Add(iv);

			return new DerSequence(v);
        }
    }
}
