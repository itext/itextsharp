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



namespace Org.BouncyCastle.Bcpg.Sig
{
    /**
    * packet giving signature creation time.
    */
    public class IssuerKeyId
        : SignatureSubpacket
    {
        protected static byte[] KeyIdToBytes(
            long    keyId)
        {
            byte[]    data = new byte[8];

            data[0] = (byte)(keyId >> 56);
            data[1] = (byte)(keyId >> 48);
            data[2] = (byte)(keyId >> 40);
            data[3] = (byte)(keyId >> 32);
            data[4] = (byte)(keyId >> 24);
            data[5] = (byte)(keyId >> 16);
            data[6] = (byte)(keyId >> 8);
            data[7] = (byte)keyId;

            return data;
        }

        public IssuerKeyId(
            bool    critical,
            byte[]     data)
            : base(SignatureSubpacketTag.IssuerKeyId, critical, data)
        {
        }

        public IssuerKeyId(
            bool    critical,
            long       keyId)
            : base(SignatureSubpacketTag.IssuerKeyId, critical, KeyIdToBytes(keyId))
        {
        }

        public long KeyId
        {
			get
			{
				long keyId = ((long)(data[0] & 0xff) << 56)
					| ((long)(data[1] & 0xff) << 48)
					| ((long)(data[2] & 0xff) << 40)
					| ((long)(data[3] & 0xff) << 32)
					| ((long)(data[4] & 0xff) << 24)
					| ((long)(data[5] & 0xff) << 16)
					| ((long)(data[6] & 0xff) << 8)
					| ((long)data[7] & 0xff);

				return keyId;
			}
        }
    }
}
