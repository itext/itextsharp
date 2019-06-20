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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * The AuthorityKeyIdentifier object.
     * <pre>
     * id-ce-authorityKeyIdentifier OBJECT IDENTIFIER ::=  { id-ce 35 }
     *
     *   AuthorityKeyIdentifier ::= Sequence {
     *      keyIdentifier             [0] IMPLICIT KeyIdentifier           OPTIONAL,
     *      authorityCertIssuer       [1] IMPLICIT GeneralNames            OPTIONAL,
     *      authorityCertSerialNumber [2] IMPLICIT CertificateSerialNumber OPTIONAL  }
     *
     *   KeyIdentifier ::= OCTET STRING
     * </pre>
     *
     */
    public class AuthorityKeyIdentifier
        : Asn1Encodable
    {
        internal readonly Asn1OctetString	keyidentifier;
        internal readonly GeneralNames		certissuer;
        internal readonly DerInteger		certserno;

		public static AuthorityKeyIdentifier GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static AuthorityKeyIdentifier GetInstance(
            object obj)
        {
            if (obj is AuthorityKeyIdentifier)
            {
                return (AuthorityKeyIdentifier) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new AuthorityKeyIdentifier((Asn1Sequence) obj);
            }

	        if (obj is X509Extension)
			{
				return GetInstance(X509Extension.ConvertValueToObject((X509Extension) obj));
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		protected internal AuthorityKeyIdentifier(
            Asn1Sequence seq)
        {
			foreach (Asn1TaggedObject o in seq)
			{
				switch (o.TagNo)
                {
					case 0:
						this.keyidentifier = Asn1OctetString.GetInstance(o, false);
						break;
					case 1:
						this.certissuer = GeneralNames.GetInstance(o, false);
						break;
					case 2:
						this.certserno = DerInteger.GetInstance(o, false);
						break;
					default:
						throw new ArgumentException("illegal tag");
                }
            }
        }

		/**
         *
         * Calulates the keyidentifier using a SHA1 hash over the BIT STRING
         * from SubjectPublicKeyInfo as defined in RFC2459.
         *
         * Example of making a AuthorityKeyIdentifier:
         * <pre>
	     *   SubjectPublicKeyInfo apki = new SubjectPublicKeyInfo((ASN1Sequence)new ASN1InputStream(
		 *       publicKey.getEncoded()).readObject());
         *   AuthorityKeyIdentifier aki = new AuthorityKeyIdentifier(apki);
         * </pre>
         *
         **/
        public AuthorityKeyIdentifier(
            SubjectPublicKeyInfo spki)
        {
            IDigest digest = new Sha1Digest();
            byte[] resBuf = new byte[digest.GetDigestSize()];

			byte[] bytes = spki.PublicKeyData.GetBytes();
            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(resBuf, 0);
            this.keyidentifier = new DerOctetString(resBuf);
        }

        /**
         * create an AuthorityKeyIdentifier with the GeneralNames tag and
         * the serial number provided as well.
         */
        public AuthorityKeyIdentifier(
            SubjectPublicKeyInfo	spki,
            GeneralNames			name,
            BigInteger				serialNumber)
        {
            IDigest digest = new Sha1Digest();
            byte[] resBuf = new byte[digest.GetDigestSize()];

			byte[] bytes = spki.PublicKeyData.GetBytes();
            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(resBuf, 0);

			this.keyidentifier = new DerOctetString(resBuf);
            this.certissuer = name;
            this.certserno = new DerInteger(serialNumber);
        }

		/**
		 * create an AuthorityKeyIdentifier with the GeneralNames tag and
		 * the serial number provided.
		 */
		public AuthorityKeyIdentifier(
			GeneralNames	name,
			BigInteger		serialNumber)
		{
			this.keyidentifier = null;
			this.certissuer = GeneralNames.GetInstance(name.ToAsn1Object());
			this.certserno = new DerInteger(serialNumber);
		}

		/**
		 * create an AuthorityKeyIdentifier with a precomputed key identifier
		 */
		public AuthorityKeyIdentifier(
			byte[] keyIdentifier)
		{
			this.keyidentifier = new DerOctetString(keyIdentifier);
			this.certissuer = null;
			this.certserno = null;
		}

		/**
		 * create an AuthorityKeyIdentifier with a precomupted key identifier
		 * and the GeneralNames tag and the serial number provided as well.
		 */
		public AuthorityKeyIdentifier(
			byte[]			keyIdentifier,
			GeneralNames	name,
			BigInteger		serialNumber)
		{
			this.keyidentifier = new DerOctetString(keyIdentifier);
			this.certissuer = GeneralNames.GetInstance(name.ToAsn1Object());
			this.certserno = new DerInteger(serialNumber);
		}

		public byte[] GetKeyIdentifier()
        {
			return keyidentifier == null ? null : keyidentifier.GetOctets();
        }

		public GeneralNames AuthorityCertIssuer
		{
			get { return certissuer; }
		}

		public BigInteger AuthorityCertSerialNumber
        {
            get { return certserno == null ? null : certserno.Value; }
        }

		/**
         * Produce an object suitable for an Asn1OutputStream.
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			if (keyidentifier != null)
            {
                v.Add(new DerTaggedObject(false, 0, keyidentifier));
            }

			if (certissuer != null)
            {
                v.Add(new DerTaggedObject(false, 1, certissuer));
            }

			if (certserno != null)
            {
                v.Add(new DerTaggedObject(false, 2, certserno));
            }

			return new DerSequence(v);
        }

		public override string ToString()
        {
            return ("AuthorityKeyIdentifier: KeyID(" + this.keyidentifier.GetOctets() + ")");
        }
    }
}
