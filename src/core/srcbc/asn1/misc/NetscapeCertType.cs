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
using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Misc
{
    /**
     * The NetscapeCertType object.
     * <pre>
     *    NetscapeCertType ::= BIT STRING {
     *         SSLClient               (0),
     *         SSLServer               (1),
     *         S/MIME                  (2),
     *         Object Signing          (3),
     *         Reserved                (4),
     *         SSL CA                  (5),
     *         S/MIME CA               (6),
     *         Object Signing CA       (7) }
     * </pre>
     */
    public class NetscapeCertType
        : DerBitString
    {
        public const int SslClient        = (1 << 7);
        public const int SslServer        = (1 << 6);
        public const int Smime            = (1 << 5);
        public const int ObjectSigning    = (1 << 4);
        public const int Reserved         = (1 << 3);
        public const int SslCA            = (1 << 2);
        public const int SmimeCA          = (1 << 1);
        public const int ObjectSigningCA  = (1 << 0);

		/**
         * Basic constructor.
         *
         * @param usage - the bitwise OR of the Key Usage flags giving the
         * allowed uses for the key.
         * e.g. (X509NetscapeCertType.sslCA | X509NetscapeCertType.smimeCA)
         */
        public NetscapeCertType(int usage)
			: base(GetBytes(usage), GetPadBits(usage))
        {
        }

		public NetscapeCertType(DerBitString usage)
			: base(usage.GetBytes(), usage.PadBits)
        {
        }

		public override string ToString()
        {
			byte[] data = GetBytes();
			return "NetscapeCertType: 0x" + (data[0] & 0xff).ToString("X");
        }
    }
}
