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
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Pkcs
{
	/**
	 * Utility class for reencoding PKCS#12 files to definite length.
	 */
	public class Pkcs12Utilities
	{
		/**
		 * Just re-encode the outer layer of the PKCS#12 file to definite length encoding.
		 *
		 * @param berPKCS12File - original PKCS#12 file
		 * @return a byte array representing the DER encoding of the PFX structure
		 * @throws IOException
		 */
		public static byte[] ConvertToDefiniteLength(
			byte[] berPkcs12File)
		{
			Pfx pfx = new Pfx(Asn1Sequence.GetInstance(Asn1Object.FromByteArray(berPkcs12File)));

			return pfx.GetEncoded(Asn1Encodable.Der);
		}

		/**
		* Re-encode the PKCS#12 structure to definite length encoding at the inner layer
		* as well, recomputing the MAC accordingly.
		*
		* @param berPKCS12File - original PKCS12 file.
		* @param provider - provider to use for MAC calculation.
		* @return a byte array representing the DER encoding of the PFX structure.
		* @throws IOException on parsing, encoding errors.
		*/
		public static byte[] ConvertToDefiniteLength(
			byte[]	berPkcs12File,
			char[]	passwd)
		{
			Pfx pfx = new Pfx(Asn1Sequence.GetInstance(Asn1Object.FromByteArray(berPkcs12File)));

			ContentInfo info = pfx.AuthSafe;

			Asn1OctetString content = Asn1OctetString.GetInstance(info.Content);
			Asn1Object obj = Asn1Object.FromByteArray(content.GetOctets());

			info = new ContentInfo(info.ContentType, new DerOctetString(obj.GetEncoded(Asn1Encodable.Der)));

			MacData mData = pfx.MacData;

			try
			{
				int itCount = mData.IterationCount.IntValue;
				byte[] data = Asn1OctetString.GetInstance(info.Content).GetOctets();
				byte[] res = Pkcs12Store.CalculatePbeMac(
					mData.Mac.AlgorithmID.ObjectID, mData.GetSalt(), itCount, passwd, false, data);

				AlgorithmIdentifier algId = new AlgorithmIdentifier(
					mData.Mac.AlgorithmID.ObjectID, DerNull.Instance);
				DigestInfo dInfo = new DigestInfo(algId, res);

				mData = new MacData(dInfo, mData.GetSalt(), itCount);
			}
			catch (Exception e)
			{
				throw new IOException("error constructing MAC: " + e.ToString());
			}

			pfx = new Pfx(info, mData);

			return pfx.GetEncoded(Asn1Encodable.Der);
		}
	}
}