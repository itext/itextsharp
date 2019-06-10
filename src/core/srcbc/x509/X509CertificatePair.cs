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
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509
{
	/// <remarks>
	/// This class contains a cross certificate pair. Cross certificates pairs may
	/// contain two cross signed certificates from two CAs. A certificate from the
	/// other CA to this CA is contained in the forward certificate, the certificate
	/// from this CA to the other CA is contained in the reverse certificate.
	/// </remarks>
	public class X509CertificatePair
	{
		private readonly X509Certificate forward;
		private readonly X509Certificate reverse;

		/// <summary>Constructor</summary>
		/// <param name="forward">Certificate from the other CA to this CA.</param>
		/// <param name="reverse">Certificate from this CA to the other CA.</param>
		public X509CertificatePair(
			X509Certificate	forward,
			X509Certificate	reverse)
		{
			this.forward = forward;
			this.reverse = reverse;
		}

		/// <summary>Constructor from a ASN.1 CertificatePair structure.</summary>
		/// <param name="pair">The <c>CertificatePair</c> ASN.1 object.</param>
		public X509CertificatePair(
			CertificatePair pair)
		{
			if (pair.Forward != null)
			{
				this.forward = new X509Certificate(pair.Forward);
			}
			if (pair.Reverse != null)
			{
				this.reverse = new X509Certificate(pair.Reverse);
			}
		}

		public byte[] GetEncoded()
		{
			try
			{
				X509CertificateStructure f = null, r = null;

				if (forward != null)
				{
					f = X509CertificateStructure.GetInstance(
						Asn1Object.FromByteArray(forward.GetEncoded()));

					if (f == null)
						throw new CertificateEncodingException("unable to get encoding for forward");
				}

				if (reverse != null)
				{
					r = X509CertificateStructure.GetInstance(
						Asn1Object.FromByteArray(reverse.GetEncoded()));

					if (r == null)
						throw new CertificateEncodingException("unable to get encoding for reverse");
				}

				return new CertificatePair(f, r).GetDerEncoded();
			}
			catch (Exception e)
			{
				// TODO
//				throw new ExtCertificateEncodingException(e.toString(), e);
				throw new CertificateEncodingException(e.Message, e);
			}
		}

		/// <summary>Returns the certificate from the other CA to this CA.</summary>
		public X509Certificate Forward
		{
			get { return forward; }
		}

		/// <summary>Returns the certificate from this CA to the other CA.</summary>
		public X509Certificate Reverse
		{
			get { return reverse; }
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			X509CertificatePair other = obj as X509CertificatePair;

			if (other == null)
				return false;

			return Platform.Equals(this.forward, other.forward)
				&& Platform.Equals(this.reverse, other.reverse);
		}

		public override int GetHashCode()
		{
			int hash = -1;
			if (forward != null)
			{
				hash ^= forward.GetHashCode();
			}
			if (reverse != null)
			{
				hash *= 17;
				hash ^= reverse.GetHashCode();
			}
			return hash;
		}
	}
}
