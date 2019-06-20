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

using Org.BouncyCastle.Asn1.Pkcs;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * an X509Certificate structure.
     * <pre>
     *  Certificate ::= Sequence {
     *      tbsCertificate          TbsCertificate,
     *      signatureAlgorithm      AlgorithmIdentifier,
     *      signature               BIT STRING
     *  }
     * </pre>
     */
    public class X509CertificateStructure
        : Asn1Encodable
    {
        private readonly TbsCertificateStructure	tbsCert;
        private readonly AlgorithmIdentifier		sigAlgID;
        private readonly DerBitString				sig;

		public static X509CertificateStructure GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static X509CertificateStructure GetInstance(
            object obj)
        {
            if (obj is X509CertificateStructure)
                return (X509CertificateStructure)obj;

			if (obj != null)
				return new X509CertificateStructure(Asn1Sequence.GetInstance(obj));

			return null;
        }

		public X509CertificateStructure(
			TbsCertificateStructure	tbsCert,
			AlgorithmIdentifier		sigAlgID,
			DerBitString			sig)
		{
			if (tbsCert == null)
				throw new ArgumentNullException("tbsCert");
			if (sigAlgID == null)
				throw new ArgumentNullException("sigAlgID");
			if (sig == null)
				throw new ArgumentNullException("sig");

			this.tbsCert = tbsCert;
			this.sigAlgID = sigAlgID;
			this.sig = sig;
		}

		private X509CertificateStructure(
            Asn1Sequence seq)
        {
			if (seq.Count != 3)
				throw new ArgumentException("sequence wrong size for a certificate", "seq");

			//
            // correct x509 certficate
            //
			tbsCert = TbsCertificateStructure.GetInstance(seq[0]);
			sigAlgID = AlgorithmIdentifier.GetInstance(seq[1]);
			sig = DerBitString.GetInstance(seq[2]);
        }

		public TbsCertificateStructure TbsCertificate
        {
			get { return tbsCert; }
        }

		public int Version
        {
            get { return tbsCert.Version; }
        }

		public DerInteger SerialNumber
        {
            get { return tbsCert.SerialNumber; }
        }

		public X509Name Issuer
        {
            get { return tbsCert.Issuer; }
        }

		public Time StartDate
        {
            get { return tbsCert.StartDate; }
        }

		public Time EndDate
        {
            get { return tbsCert.EndDate; }
        }

		public X509Name Subject
        {
            get { return tbsCert.Subject; }
        }

		public SubjectPublicKeyInfo SubjectPublicKeyInfo
        {
            get { return tbsCert.SubjectPublicKeyInfo; }
        }

		public AlgorithmIdentifier SignatureAlgorithm
        {
            get { return sigAlgID; }
        }

		public DerBitString Signature
        {
            get { return sig; }
        }

		public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(tbsCert, sigAlgID, sig);
        }
	}
}
