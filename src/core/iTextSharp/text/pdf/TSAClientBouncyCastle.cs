using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Text;
using System.util;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Tsp;
/*
 * $Id: TSAClientBouncyCastle.java 3973 2009-06-16 10:30:31Z psoares33 $
 *
 * Copyright 2009 Martin Brunecky, Aiken Sam
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
 * are Copyright (C) 2009 by Martin Brunecky. All Rights Reserved.
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
    * Time Stamp Authority Client interface implementation using Bouncy Castle
    * org.bouncycastle.tsp package.
    * <p>
    * Created by Aiken Sam, 2006-11-15, refactored by Martin Brunecky, 07/15/2007
    * for ease of subclassing.
    * </p>
    * @since	2.1.6
    */
    public class TSAClientBouncyCastle : ITSAClient {
        /** URL of the Time Stamp Authority */
	    protected String tsaURL;
	    /** TSA Username */
        protected String tsaUsername;
        /** TSA password */
        protected String tsaPassword;
        /** Estimate of the received time stamp token */
        protected int tokSzEstimate;
        
        /**
        * Creates an instance of a TSAClient that will use BouncyCastle.
        * @param url String - Time Stamp Authority URL (i.e. "http://tsatest1.digistamp.com/TSA")
        */
        public TSAClientBouncyCastle(String url) : this(url, null, null, 4096) {
        }
        
        /**
        * Creates an instance of a TSAClient that will use BouncyCastle.
        * @param url String - Time Stamp Authority URL (i.e. "http://tsatest1.digistamp.com/TSA")
        * @param username String - user(account) name
        * @param password String - password
        */
        public TSAClientBouncyCastle(String url, String username, String password) : this(url, username, password, 4096) {
        }
        
        /**
        * Constructor.
        * Note the token size estimate is updated by each call, as the token
        * size is not likely to change (as long as we call the same TSA using
        * the same imprint length).
        * @param url String - Time Stamp Authority URL (i.e. "http://tsatest1.digistamp.com/TSA")
        * @param username String - user(account) name
        * @param password String - password
        * @param tokSzEstimate int - estimated size of received time stamp token (DER encoded)
        */
        public TSAClientBouncyCastle(String url, String username, String password, int tokSzEstimate) {
            this.tsaURL       = url;
            this.tsaUsername  = username;
            this.tsaPassword  = password;
            this.tokSzEstimate = tokSzEstimate;
        }
        
        /**
        * Get the token size estimate.
        * Returned value reflects the result of the last succesfull call, padded
        * @return an estimate of the token size
        */
        public int GetTokenSizeEstimate() {
            return tokSzEstimate;
        }
        
        /**
        * Get RFC 3161 timeStampToken.
        * Method may return null indicating that timestamp should be skipped.
        * @param caller PdfPKCS7 - calling PdfPKCS7 instance (in case caller needs it)
        * @param imprint byte[] - data imprint to be time-stamped
        * @return byte[] - encoded, TSA signed data of the timeStampToken
        * @throws Exception - TSA request failed
        * @see com.lowagie.text.pdf.TSAClient#getTimeStampToken(com.lowagie.text.pdf.PdfPKCS7, byte[])
        */
        public byte[] GetTimeStampToken(PdfPKCS7 caller, byte[] imprint) {
            return GetTimeStampToken(imprint);
        }
        
        /**
        * Get timestamp token - Bouncy Castle request encoding / decoding layer
        */
        protected internal byte[] GetTimeStampToken(byte[] imprint) {
            byte[] respBytes = null;
            // Setup the time stamp request
            TimeStampRequestGenerator tsqGenerator = new TimeStampRequestGenerator();
            tsqGenerator.SetCertReq(true);
            // tsqGenerator.setReqPolicy("1.3.6.1.4.1.601.10.3.1");
            BigInteger nonce = BigInteger.ValueOf(DateTime.Now.Ticks + Environment.TickCount);
            TimeStampRequest request = tsqGenerator.Generate(X509ObjectIdentifiers.IdSha1.Id, imprint, nonce);
            byte[] requestBytes = request.GetEncoded();
            
            // Call the communications layer
            respBytes = GetTSAResponse(requestBytes);
            
            // Handle the TSA response
            TimeStampResponse response = new TimeStampResponse(respBytes);
            
            // validate communication level attributes (RFC 3161 PKIStatus)
            response.Validate(request);
            PkiFailureInfo failure = response.GetFailInfo();
            int value = (failure == null) ? 0 : failure.IntValue;
            if (value != 0) {
                // @todo: Translate value of 15 error codes defined by PKIFailureInfo to string
                throw new Exception("Invalid TSA '" + tsaURL + "' response, code " + value);
            }
            // @todo: validate the time stap certificate chain (if we want
            //        assure we do not sign using an invalid timestamp).
            
            // extract just the time stamp token (removes communication status info)
            TimeStampToken  tsToken = response.TimeStampToken;
            if (tsToken == null) {
                throw new Exception("TSA '" + tsaURL + "' failed to return time stamp token: " + response.GetStatusString());
            }
            TimeStampTokenInfo info = tsToken.TimeStampInfo; // to view details
            byte[] encoded = tsToken.GetEncoded();
            
            // Update our token size estimate for the next call (padded to be safe)
            this.tokSzEstimate = encoded.Length + 32;
            return encoded;
        }
        
        /**
        * Get timestamp token - communications layer
        * @return - byte[] - TSA response, raw bytes (RFC 3161 encoded)
        */
        protected internal virtual byte[] GetTSAResponse(byte[] requestBytes) {
            HttpWebRequest con = (HttpWebRequest)WebRequest.Create(tsaURL);
            con.ContentLength = requestBytes.Length;
            con.ContentType = "application/timestamp-query";
            con.Method = "POST";
            if ((tsaUsername != null) && !tsaUsername.Equals("") ) {
                string authInfo = tsaUsername + ":" + tsaPassword;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                con.Headers["Authorization"] = "Basic " + authInfo;
            }
            Stream outp = con.GetRequestStream();
            outp.Write(requestBytes, 0, requestBytes.Length);
            outp.Close();
            HttpWebResponse response = (HttpWebResponse)con.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new IOException("Invalid HTTP response: " + (int)response.StatusCode);
            Stream inp = response.GetResponseStream();

            MemoryStream baos = new MemoryStream();
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while ((bytesRead = inp.Read(buffer, 0, buffer.Length)) > 0) {
                baos.Write(buffer, 0, bytesRead);
            }
            inp.Close();
            response.Close();
            byte[] respBytes = baos.ToArray();
            
            String encoding = response.ContentEncoding;
            if (encoding != null && Util.EqualsIgnoreCase(encoding, "base64")) {
                respBytes = Convert.FromBase64String(Encoding.ASCII.GetString(respBytes));
            }
            return respBytes;
        }    
    }
}