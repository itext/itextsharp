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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509
{
	/// <summary>An implementation of a version 2 X.509 Attribute Certificate.</summary>
	public class X509V2AttributeCertificate
		: X509ExtensionBase, IX509AttributeCertificate
	{
		private readonly AttributeCertificate cert;
		private readonly DateTime notBefore;
		private readonly DateTime notAfter;

		private static AttributeCertificate GetObject(Stream input)
		{
			try
			{
				return AttributeCertificate.GetInstance(Asn1Object.FromStream(input));
			}
			catch (IOException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new IOException("exception decoding certificate structure", e);
			}
		}

		public X509V2AttributeCertificate(
			Stream encIn)
			: this(GetObject(encIn))
		{
		}

		public X509V2AttributeCertificate(
			byte[] encoded)
			: this(new MemoryStream(encoded, false))
		{
		}

		internal X509V2AttributeCertificate(
			AttributeCertificate cert)
		{
			this.cert = cert;

			try
			{
				this.notAfter = cert.ACInfo.AttrCertValidityPeriod.NotAfterTime.ToDateTime();
				this.notBefore = cert.ACInfo.AttrCertValidityPeriod.NotBeforeTime.ToDateTime();
			}
			catch (Exception e)
			{
				throw new IOException("invalid data structure in certificate!", e);
			}
		}

		public virtual int Version
		{
			get { return cert.ACInfo.Version.Value.IntValue + 1; }
		}

		public virtual BigInteger SerialNumber
		{
			get { return cert.ACInfo.SerialNumber.Value; }
		}

		public virtual AttributeCertificateHolder Holder
		{
			get
			{
				return new AttributeCertificateHolder((Asn1Sequence)cert.ACInfo.Holder.ToAsn1Object());
			}
		}

		public virtual AttributeCertificateIssuer Issuer
		{
			get
			{
				return new AttributeCertificateIssuer(cert.ACInfo.Issuer);
			}
		}

		public virtual DateTime NotBefore
		{
			get { return notBefore; }
		}

		public virtual DateTime NotAfter
		{
			get { return notAfter; }
		}

		public virtual bool[] GetIssuerUniqueID()
		{
			DerBitString id = cert.ACInfo.IssuerUniqueID;

			if (id != null)
			{
				byte[] bytes = id.GetBytes();
				bool[] boolId = new bool[bytes.Length * 8 - id.PadBits];

				for (int i = 0; i != boolId.Length; i++)
				{
					//boolId[i] = (bytes[i / 8] & (0x80 >>> (i % 8))) != 0;
					boolId[i] = (bytes[i / 8] & (0x80 >> (i % 8))) != 0;
				}

				return boolId;
			}

			return null;
		}

		public virtual bool IsValidNow
		{
			get { return IsValid(DateTime.UtcNow); }
		}

		public virtual bool IsValid(
			DateTime date)
		{
			return date.CompareTo(NotBefore) >= 0 && date.CompareTo(NotAfter) <= 0;
		}

		public virtual void CheckValidity()
		{
			this.CheckValidity(DateTime.UtcNow);
		}

		public virtual void CheckValidity(
			DateTime date)
		{
			if (date.CompareTo(NotAfter) > 0)
				throw new CertificateExpiredException("certificate expired on " + NotAfter);
			if (date.CompareTo(NotBefore) < 0)
				throw new CertificateNotYetValidException("certificate not valid until " + NotBefore);
		}

		public virtual byte[] GetSignature()
		{
			return cert.SignatureValue.GetBytes();
		}

		public virtual void Verify(
			AsymmetricKeyParameter publicKey)
		{
			if (!cert.SignatureAlgorithm.Equals(cert.ACInfo.Signature))
			{
				throw new CertificateException("Signature algorithm in certificate info not same as outer certificate");
			}

			ISigner signature = SignerUtilities.GetSigner(cert.SignatureAlgorithm.ObjectID.Id);

			signature.Init(false, publicKey);

			try
			{
				byte[] b = cert.ACInfo.GetEncoded();
				signature.BlockUpdate(b, 0, b.Length);
			}
			catch (IOException e)
			{
				throw new SignatureException("Exception encoding certificate info object", e);
			}

			if (!signature.VerifySignature(this.GetSignature()))
			{
				throw new InvalidKeyException("Public key presented not for certificate signature");
			}
		}

		public virtual byte[] GetEncoded()
		{
			return cert.GetEncoded();
		}

		protected override X509Extensions GetX509Extensions()
		{
			return cert.ACInfo.Extensions;
		}

		public virtual X509Attribute[] GetAttributes()
		{
			Asn1Sequence seq = cert.ACInfo.Attributes;
			X509Attribute[] attrs = new X509Attribute[seq.Count];

			for (int i = 0; i != seq.Count; i++)
			{
				attrs[i] = new X509Attribute((Asn1Encodable)seq[i]);
			}

			return attrs;
		}

		public virtual X509Attribute[] GetAttributes(
			string oid)
		{
			Asn1Sequence seq = cert.ACInfo.Attributes;
			IList list = Platform.CreateArrayList();

			for (int i = 0; i != seq.Count; i++)
			{
				X509Attribute attr = new X509Attribute((Asn1Encodable)seq[i]);
				if (attr.Oid.Equals(oid))
				{
					list.Add(attr);
				}
			}

			if (list.Count < 1)
			{
				return null;
			}

            X509Attribute[] result = new X509Attribute[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                result[i] = (X509Attribute)list[i];
            }
            return result;
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			X509V2AttributeCertificate other = obj as X509V2AttributeCertificate;

			if (other == null)
				return false;

			return cert.Equals(other.cert);

			// NB: May prefer this implementation of Equals if more than one certificate implementation in play
			//return Arrays.AreEqual(this.GetEncoded(), other.GetEncoded());
		}

		public override int GetHashCode()
		{
			return cert.GetHashCode();
		}
	}
}
