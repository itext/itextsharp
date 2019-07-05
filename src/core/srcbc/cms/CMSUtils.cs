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
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Cms
{
    internal class CmsUtilities
    {
		// TODO Is there a .NET equivalent to this?
//		private static readonly Runtime RUNTIME = Runtime.getRuntime();

		internal static int MaximumMemory
		{
			get
			{
				// TODO Is there a .NET equivalent to this?
				long maxMem = int.MaxValue;//RUNTIME.maxMemory();

				if (maxMem > int.MaxValue)
				{
					return int.MaxValue;
				}

				return (int)maxMem;
			}
		}

		internal static ContentInfo ReadContentInfo(
			byte[] input)
		{
			// enforce limit checking as from a byte array
			return ReadContentInfo(new Asn1InputStream(input));
		}

		internal static ContentInfo ReadContentInfo(
			Stream input)
		{
			// enforce some limit checking
			return ReadContentInfo(new Asn1InputStream(input, MaximumMemory));
		}

		private static ContentInfo ReadContentInfo(
			Asn1InputStream aIn)
		{
			try
			{
				return ContentInfo.GetInstance(aIn.ReadObject());
			}
			catch (IOException e)
			{
				throw new CmsException("IOException reading content.", e);
			}
			catch (InvalidCastException e)
			{
				throw new CmsException("Malformed content.", e);
			}
			catch (ArgumentException e)
			{
				throw new CmsException("Malformed content.", e);
			}
		}

		public static byte[] StreamToByteArray(
            Stream inStream)
        {
			return Streams.ReadAll(inStream);
        }

		public static byte[] StreamToByteArray(
            Stream	inStream,
			int		limit)
        {
			return Streams.ReadAllLimited(inStream, limit);
        }

		public static IList GetCertificatesFromStore(
			IX509Store certStore)
		{
			try
			{
				IList certs = Platform.CreateArrayList();

				if (certStore != null)
				{
					foreach (X509Certificate c in certStore.GetMatches(null))
					{
						certs.Add(
							X509CertificateStructure.GetInstance(
								Asn1Object.FromByteArray(c.GetEncoded())));
					}
				}

				return certs;
			}
			catch (CertificateEncodingException e)
			{
				throw new CmsException("error encoding certs", e);
			}
			catch (Exception e)
			{
				throw new CmsException("error processing certs", e);
			}
		}

		public static IList GetCrlsFromStore(
			IX509Store crlStore)
		{
			try
			{
                IList crls = Platform.CreateArrayList();

				if (crlStore != null)
				{
					foreach (X509Crl c in crlStore.GetMatches(null))
					{
						crls.Add(
							CertificateList.GetInstance(
								Asn1Object.FromByteArray(c.GetEncoded())));
					}
				}

				return crls;
			}
			catch (CrlException e)
			{
				throw new CmsException("error encoding crls", e);
			}
			catch (Exception e)
			{
				throw new CmsException("error processing crls", e);
			}
		}

		public static Asn1Set CreateBerSetFromList(
			IList berObjects)
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			foreach (Asn1Encodable ae in berObjects)
			{
				v.Add(ae);
			}

			return new BerSet(v);
		}

		public static Asn1Set CreateDerSetFromList(
			IList derObjects)
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			foreach (Asn1Encodable ae in derObjects)
			{
				v.Add(ae);
			}

			return new DerSet(v);
		}

		internal static Stream CreateBerOctetOutputStream(Stream s, int tagNo, bool isExplicit, int bufferSize)
		{
			BerOctetStringGenerator octGen = new BerOctetStringGenerator(s, tagNo, isExplicit);
			return octGen.GetOctetOutputStream(bufferSize);
		}

		internal static TbsCertificateStructure GetTbsCertificateStructure(X509Certificate cert)
		{
			return TbsCertificateStructure.GetInstance(Asn1Object.FromByteArray(cert.GetTbsCertificate()));
		}

		internal static IssuerAndSerialNumber GetIssuerAndSerialNumber(X509Certificate cert)
		{
			TbsCertificateStructure tbsCert = GetTbsCertificateStructure(cert);
			return new IssuerAndSerialNumber(tbsCert.Issuer, tbsCert.SerialNumber.Value);
		}
	}
}
