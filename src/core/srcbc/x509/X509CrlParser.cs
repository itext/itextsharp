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
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.X509
{
	public class X509CrlParser
	{
		private static readonly PemParser PemCrlParser = new PemParser("CRL");

		private readonly bool lazyAsn1;

		private Asn1Set	sCrlData;
		private int		sCrlDataObjectCount;
		private Stream	currentCrlStream;

		public X509CrlParser()
			: this(false)
		{
		}

		public X509CrlParser(
			bool lazyAsn1)
		{
			this.lazyAsn1 = lazyAsn1;
		}

		private X509Crl ReadPemCrl(
			Stream inStream)
		{
			Asn1Sequence seq = PemCrlParser.ReadPemObject(inStream);

			return seq == null
				?	null
				:	CreateX509Crl(CertificateList.GetInstance(seq));
		}

		private X509Crl ReadDerCrl(
			Asn1InputStream dIn)
		{
			Asn1Sequence seq = (Asn1Sequence)dIn.ReadObject();

			if (seq.Count > 1 && seq[0] is DerObjectIdentifier)
			{
				if (seq[0].Equals(PkcsObjectIdentifiers.SignedData))
				{
					sCrlData = SignedData.GetInstance(
						Asn1Sequence.GetInstance((Asn1TaggedObject) seq[1], true)).Crls;

					return GetCrl();
				}
			}

			return CreateX509Crl(CertificateList.GetInstance(seq));
		}

		private X509Crl GetCrl()
		{
			if (sCrlData == null || sCrlDataObjectCount >= sCrlData.Count)
			{
				return null;
			}

			return CreateX509Crl(
				CertificateList.GetInstance(
					sCrlData[sCrlDataObjectCount++]));
		}

		protected virtual X509Crl CreateX509Crl(
			CertificateList c)
		{
			return new X509Crl(c);
		}

		/// <summary>
		/// Create loading data from byte array.
		/// </summary>
		/// <param name="input"></param>
		public X509Crl ReadCrl(
			byte[] input)
		{
			return ReadCrl(new MemoryStream(input, false));
		}

		/// <summary>
		/// Create loading data from byte array.
		/// </summary>
		/// <param name="input"></param>
		public ICollection ReadCrls(
			byte[] input)
		{
			return ReadCrls(new MemoryStream(input, false));
		}

		/**
		 * Generates a certificate revocation list (CRL) object and initializes
		 * it with the data read from the input stream inStream.
		 */
		public X509Crl ReadCrl(
			Stream inStream)
		{
			if (inStream == null)
				throw new ArgumentNullException("inStream");
			if (!inStream.CanRead)
				throw new ArgumentException("inStream must be read-able", "inStream");

			if (currentCrlStream == null)
			{
				currentCrlStream = inStream;
				sCrlData = null;
				sCrlDataObjectCount = 0;
			}
			else if (currentCrlStream != inStream) // reset if input stream has changed
			{
				currentCrlStream = inStream;
				sCrlData = null;
				sCrlDataObjectCount = 0;
			}

			try
			{
				if (sCrlData != null)
				{
					if (sCrlDataObjectCount != sCrlData.Count)
					{
						return GetCrl();
					}

					sCrlData = null;
					sCrlDataObjectCount = 0;
					return null;
				}

				PushbackStream pis = new PushbackStream(inStream);
				int tag = pis.ReadByte();

				if (tag < 0)
					return null;

				pis.Unread(tag);

				if (tag != 0x30)	// assume ascii PEM encoded.
				{
					return ReadPemCrl(pis);
				}

				Asn1InputStream asn1 = lazyAsn1
					?	new LazyAsn1InputStream(pis)
					:	new Asn1InputStream(pis);

				return ReadDerCrl(asn1);
			}
			catch (CrlException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new CrlException(e.ToString());
			}
		}

		/**
		 * Returns a (possibly empty) collection view of the CRLs read from
		 * the given input stream inStream.
		 *
		 * The inStream may contain a sequence of DER-encoded CRLs, or
		 * a PKCS#7 CRL set.  This is a PKCS#7 SignedData object, with the
		 * only significant field being crls.  In particular the signature
		 * and the contents are ignored.
		 */
		public ICollection ReadCrls(
			Stream inStream)
		{
			X509Crl crl;
			IList crls = Platform.CreateArrayList();

			while ((crl = ReadCrl(inStream)) != null)
			{
				crls.Add(crl);
			}

			return crls;
		}
	}
}
