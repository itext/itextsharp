using System;
using System.IO;
using System.Collections;
using System.Net;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Ocsp;
using iTextSharp.text.error_messages;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

namespace iTextSharp.text.pdf {

    /**
    * OcspClient implementation using BouncyCastle.
    * @author psoares
    * @since	2.1.6
    */
    public class OcspClientBouncyCastle : IOcspClient {
        /** root certificate */
        private X509Certificate rootCert;
        /** check certificate */
        private X509Certificate checkCert;
        /** OCSP URL */
        private String url;
        
        /**
        * Creates an instance of an OcspClient that will be using BouncyCastle.
        * @param checkCert	the check certificate
        * @param rootCert	the root certificate
        * @param url	the OCSP URL
        */
        public OcspClientBouncyCastle(X509Certificate checkCert, X509Certificate rootCert, String url) {
            this.checkCert = checkCert;
            this.rootCert = rootCert;
            this.url = url;
        }
        
        /**
        * Generates an OCSP request using BouncyCastle.
        * @param issuerCert	certificate of the issues
        * @param serialNumber	serial number
        * @return	an OCSP request
        * @throws OCSPException
        * @throws IOException
        */
        private static OcspReq GenerateOCSPRequest(X509Certificate issuerCert, BigInteger serialNumber) {
            // Generate the id for the certificate we are looking for
            CertificateID id = new CertificateID(CertificateID.HashSha1, issuerCert, serialNumber);
            
            // basic request generation with nonce
            OcspReqGenerator gen = new OcspReqGenerator();
            
            gen.AddRequest(id);
            
            // create details for nonce extension
            IDictionary extensions = new Hashtable();
            
            extensions[OcspObjectIdentifiers.PkixOcspNonce] = new X509Extension(false, new DerOctetString(new DerOctetString(PdfEncryption.CreateDocumentId()).GetEncoded()));
            
            gen.SetRequestExtensions(new X509Extensions(extensions));
            
            return gen.Generate();
        }
        
        /**
        * @return 	a byte array
        * @see com.lowagie.text.pdf.OcspClient#getEncoded()
        */
        public byte[] GetEncoded() {
            OcspReq request = GenerateOCSPRequest(rootCert, checkCert.SerialNumber);
            byte[] array = request.GetEncoded();
            HttpWebRequest con = (HttpWebRequest)WebRequest.Create(url);
            con.ContentLength = array.Length;
            con.ContentType = "application/ocsp-request";
            con.Accept = "application/ocsp-response";
            con.Method = "POST";
            Stream outp = con.GetRequestStream();
            outp.Write(array, 0, array.Length);
            outp.Close();
            HttpWebResponse response = (HttpWebResponse)con.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.http.response.1", (int)response.StatusCode));
            Stream inp = response.GetResponseStream();
            OcspResp ocspResponse = new OcspResp(inp);
            inp.Close();
            response.Close();

            if (ocspResponse.Status != 0)
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.status.1", ocspResponse.Status));
            BasicOcspResp basicResponse = (BasicOcspResp) ocspResponse.GetResponseObject();
            if (basicResponse != null) {
                SingleResp[] responses = basicResponse.Responses;
                if (responses.Length == 1) {
                    SingleResp resp = responses[0];
                    Object status = resp.GetCertStatus();
                    if (status == CertificateStatus.Good) {
                        return basicResponse.GetEncoded();
                    }
                    else if (status is Org.BouncyCastle.Ocsp.RevokedStatus) {
                        throw new IOException(MessageLocalization.GetComposedMessage("ocsp.status.is.revoked"));
                    }
                    else {
                        throw new IOException(MessageLocalization.GetComposedMessage("ocsp.status.is.unknown"));
                    }
                }
            }
            return null;
        }
    }
}
