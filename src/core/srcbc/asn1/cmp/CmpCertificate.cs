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

using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cmp
{
    public class CmpCertificate
        : Asn1Encodable, IAsn1Choice
    {
        private readonly X509CertificateStructure x509v3PKCert;
        private readonly AttributeCertificate x509v2AttrCert;

        /**
         * Note: the addition of attribute certificates is a BC extension.
         */
        public CmpCertificate(AttributeCertificate x509v2AttrCert)
        {
            this.x509v2AttrCert = x509v2AttrCert;
        }

        public CmpCertificate(X509CertificateStructure x509v3PKCert)
        {
            if (x509v3PKCert.Version != 3)
                throw new ArgumentException("only version 3 certificates allowed", "x509v3PKCert");

            this.x509v3PKCert = x509v3PKCert;
        }

        public static CmpCertificate GetInstance(object obj)
        {
            if (obj is CmpCertificate)
                return (CmpCertificate)obj;

            if (obj is Asn1Sequence)
                return new CmpCertificate(X509CertificateStructure.GetInstance(obj));

            if (obj is Asn1TaggedObject)
                return new CmpCertificate(AttributeCertificate.GetInstance(((Asn1TaggedObject)obj).GetObject()));

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        public virtual bool IsX509v3PKCert
        {
            get { return x509v3PKCert != null; }
        }

        public virtual X509CertificateStructure X509v3PKCert
        {
            get { return x509v3PKCert; }
        }

        public virtual AttributeCertificate X509v2AttrCert
        {
            get { return x509v2AttrCert; }
        }

        /**
         * <pre>
         * CMPCertificate ::= CHOICE {
         *            x509v3PKCert        Certificate
         *            x509v2AttrCert      [1] AttributeCertificate
         *  }
         * </pre>
         * Note: the addition of attribute certificates is a BC extension.
         *
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            if (x509v2AttrCert != null)
            {
                // explicit following CMP conventions
                return new DerTaggedObject(true, 1, x509v2AttrCert);
            }

            return x509v3PKCert.ToAsn1Object();
        }
    }
}
