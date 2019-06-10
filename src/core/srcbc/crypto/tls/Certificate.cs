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
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
	/**
	* A representation for a certificate chain.
	*/
	public class Certificate
	{
		public static readonly Certificate EmptyChain = new Certificate(new X509CertificateStructure[0]);

		/**
		* The certificates.
		*/
		internal X509CertificateStructure[] certs;

		/**
		* Parse the ServerCertificate message.
		*
		* @param inStr The stream where to parse from.
		* @return A Certificate object with the certs, the server has sended.
		* @throws IOException If something goes wrong during parsing.
		*/
		internal static Certificate Parse(
			Stream inStr)
		{
			int left = TlsUtilities.ReadUint24(inStr);
			if (left == 0)
			{
				return EmptyChain;
			}
			IList tmp = Platform.CreateArrayList();
			while (left > 0)
			{
				int size = TlsUtilities.ReadUint24(inStr);
				left -= 3 + size;
				byte[] buf = new byte[size];
				TlsUtilities.ReadFully(buf, inStr);
				MemoryStream bis = new MemoryStream(buf, false);
				Asn1Object o = Asn1Object.FromStream(bis);
				tmp.Add(X509CertificateStructure.GetInstance(o));
				if (bis.Position < bis.Length)
				{
					throw new ArgumentException("Sorry, there is garbage data left after the certificate");
				}
			}
            X509CertificateStructure[] certs = new X509CertificateStructure[tmp.Count];
            for (int i = 0; i < tmp.Count; ++i)
            {
                certs[i] = (X509CertificateStructure)tmp[i];
            }
			return new Certificate(certs);
		}

		/**
		 * Encodes version of the ClientCertificate message
		 *
		 * @param outStr stream to write the message to
		 * @throws IOException If something goes wrong
		 */
		internal void Encode(
			Stream outStr)
		{
			IList encCerts = Platform.CreateArrayList();
			int totalSize = 0;
			foreach (X509CertificateStructure cert in certs)
			{
				byte[] encCert = cert.GetEncoded(Asn1Encodable.Der);
				encCerts.Add(encCert);
				totalSize += encCert.Length + 3;
			}

			TlsUtilities.WriteUint24(totalSize, outStr);

			foreach (byte[] encCert in encCerts)
			{
				TlsUtilities.WriteOpaque24(encCert, outStr);
			}
		}

		/**
		* Private constructor from a cert array.
		*
		* @param certs The certs the chain should contain.
		*/
		public Certificate(X509CertificateStructure[] certs)
		{
			if (certs == null)
				throw new ArgumentNullException("certs");

			this.certs = certs;
		}

		/// <returns>An array which contains the certs, this chain contains.</returns>
		public X509CertificateStructure[] GetCerts()
		{
			return (X509CertificateStructure[]) certs.Clone();
		}

		public bool IsEmpty
		{
			get { return certs.Length == 0; }
		}
	}
}
