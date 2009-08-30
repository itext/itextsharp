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
 * $Id: OcspClientBouncyCastle.java 3959 2009-06-09 08:31:05Z blowagie $
 *
 * Copyright 2009 Paulo Soares
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999-2005 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2009 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the "GNU LIBRARY GENERAL PUBLIC LICENSE"), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
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
            ArrayList oids = new ArrayList();
            ArrayList values = new ArrayList();
            
            oids.Add(OcspObjectIdentifiers.PkixOcspNonce);
            values.Add(new X509Extension(false, new DerOctetString(new DerOctetString(PdfEncryption.CreateDocumentId()).GetEncoded())));
            
            gen.SetRequestExtensions(new X509Extensions(oids, values));
            
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
