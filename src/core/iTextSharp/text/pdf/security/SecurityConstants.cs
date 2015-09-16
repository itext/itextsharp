/*
 * $Id$
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

using System;

namespace iTextSharp.text.pdf.security
{
    internal sealed class SecurityConstants {

        public const String XMLNS = "xmlns";
        public const String XMLNS_XADES = "xmlns:xades";

        public const String XMLNS_URI = "http://www.w3.org/2000/xmlns/";
        public const String XMLDSIG_URI = "http://www.w3.org/2000/09/xmldsig#";
        public const String XADES_132_URI = "http://uri.etsi.org/01903/v1.3.2#";
        public const String XMLDSIG_URI_C14N = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        public const String XMLDSIG_URI_RSA_SHA1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
        public const String XMLDSIG_URI_DSA_SHA1 = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
        public const String XMLDSIG_URI_ENVELOPED = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
        public const String XMLDSIG_URI_XPATH_FILTER2 = "http://www.w3.org/2002/06/xmldsig-filter2";
        public const String XMLDSIG_URI_SHA1 = "http://www.w3.org/2000/09/xmldsig#sha1";

        public const String SignedProperties_Type = "http://uri.etsi.org/01903#SignedProperties";

        public const String OIDAsURN = "OIDAsURN";
        public const String OID_DSA_SHA1 = "urn:oid:1.2.840.10040.4.3";
        public const String OID_DSA_SHA1_DESC = "ANSI X9.57 DSA signature generated with SHA-1 hash (DSA x9.30)";

        public const String OID_RSA_SHA1 = "urn:oid:1.2.840.113549.1.1.5";
        public const String OID_RSA_SHA1_DESC = "RSA (PKCS #1 v1.5) with SHA-1 signature";

        public const String DSA = "DSA";
        public const String RSA = "RSA";
        public const String SHA1 = "SHA1";

        public const String DigestMethod = "DigestMethod";
        public const String DigestValue = "DigestValue";
        public const String Signature = "Signature";
        public const String SignatureValue = "SignatureValue";
        public const String X509SerialNumber = "X509SerialNumber";
        public const String X509IssuerName = "X509IssuerName";

        public const String Algorithm = "Algorithm";
        public const String Id = "Id";
        public const String ObjectReference = "ObjectReference";
        public const String Target = "Target";
        public const String Qualifier = "Qualifier";

        public const String XADES_Encoding = "xades:Encoding";
        public const String XADES_MimeType = "xades:MimeType";
        public const String XADES_Description = "xades:Description";
        public const String XADES_DataObjectFormat = "xades:DataObjectFormat";
        public const String XADES_SignedDataObjectProperties = "xades:SignedDataObjectProperties";
        public const String XADES_IssuerSerial = "xades:IssuerSerial";
        public const String XADES_CertDigest = "xades:CertDigest";
        public const String XADES_Cert = "xades:Cert";
        public const String XADES_SigningCertificate = "xades:SigningCertificate";
        public const String XADES_SigningTime = "xades:SigningTime";
        public const String XADES_SignedSignatureProperties = "xades:SignedSignatureProperties";
        public const String XADES_SignedProperties = "xades:SignedProperties";
        public const String XADES_QualifyingProperties = "xades:QualifyingProperties";
        public const String XADES_SignaturePolicyIdentifier = "xades:SignaturePolicyIdentifier";
        public const String XADES_SignaturePolicyId = "xades:SignaturePolicyId";
        public const String XADES_SigPolicyId = "xades:SigPolicyId";
        public const String XADES_Identifier = "xades:Identifier";
        public const String XADES_SigPolicyHash = "xades:SigPolicyHash";


        public const String Reference_ = "Reference-";
        public const String SignedProperties_ = "SignedProperties-";
        public const String Signature_ = "Signature-";

        public const String SigningTimeFormat = "yyyy-MM-dd'T'HH:mm:sszzz";
    }
}
