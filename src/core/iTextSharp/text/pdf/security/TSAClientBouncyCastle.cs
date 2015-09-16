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
using Org.BouncyCastle.Crypto;
using iTextSharp.text.log;
using iTextSharp.text.error_messages;
/*
 * $Id: TSAClientBouncyCastle.cs 106 2009-12-07 12:23:50Z psoares33 $
 *
 * This file is part of the iText project.
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

namespace iTextSharp.text.pdf.security {

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

        /** The Logger instance. */
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(TSAClientBouncyCastle));

        /** URL of the Time Stamp Authority */
	    protected internal String tsaURL;
	    /** TSA Username */
        protected internal String tsaUsername;
        /** TSA password */
        protected internal String tsaPassword;
        /** An interface that allows you to inspect the timestamp info. */
        protected ITSAInfoBouncyCastle tsaInfo;
        /** The default value for the hash algorithm */
        public const int DEFAULTTOKENSIZE = 4096;
        
        /** Estimate of the received time stamp token */
        protected internal int tokenSizeEstimate;
        
        /** The default value for the hash algorithm */
        public const String DEFAULTHASHALGORITHM = "SHA-256";
        
        /** Hash algorithm */
        protected internal String digestAlgorithm;
        /**
        * Creates an instance of a TSAClient that will use BouncyCastle.
        * @param url String - Time Stamp Authority URL (i.e. "http://tsatest1.digistamp.com/TSA")
        */
        public TSAClientBouncyCastle(String url) 
            : this(url, null, null, DEFAULTTOKENSIZE, DEFAULTHASHALGORITHM) {
        }
        
        /**
        * Creates an instance of a TSAClient that will use BouncyCastle.
        * @param url String - Time Stamp Authority URL (i.e. "http://tsatest1.digistamp.com/TSA")
        * @param username String - user(account) name
        * @param password String - password
        */
        public TSAClientBouncyCastle(String url, String username, String password) 
            : this(url, username, password, DEFAULTTOKENSIZE, DEFAULTHASHALGORITHM) {
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
        public TSAClientBouncyCastle(String url, String username, String password, int tokSzEstimate, String digestAlgorithm) {
            this.tsaURL       = url;
            this.tsaUsername  = username;
            this.tsaPassword  = password;
            this.tokenSizeEstimate = tokSzEstimate;
            this.digestAlgorithm = digestAlgorithm;
        }
        
        /**
         * @param tsaInfo the tsaInfo to set
         */
        virtual public void SetTSAInfo(ITSAInfoBouncyCastle tsaInfo) {
            this.tsaInfo = tsaInfo;
        }

        /**
        * Get the token size estimate.
        * Returned value reflects the result of the last succesfull call, padded
        * @return an estimate of the token size
        */
        public virtual int GetTokenSizeEstimate() {
            return tokenSizeEstimate;
        }
        
        /**
         * Gets the MessageDigest to digest the data imprint
         * @return the digest algorithm name
         */
        virtual public IDigest GetMessageDigest() {
            return DigestAlgorithms.GetMessageDigest(digestAlgorithm);
        }

        /**
         * Get RFC 3161 timeStampToken.
         * Method may return null indicating that timestamp should be skipped.
         * @param imprint data imprint to be time-stamped
         * @return encoded, TSA signed data of the timeStampToken
         */
        public virtual byte[] GetTimeStampToken(byte[] imprint) {
            byte[] respBytes = null;
            // Setup the time stamp request
            TimeStampRequestGenerator tsqGenerator = new TimeStampRequestGenerator();
            tsqGenerator.SetCertReq(true);
            // tsqGenerator.setReqPolicy("1.3.6.1.4.1.601.10.3.1");
            BigInteger nonce = BigInteger.ValueOf(DateTime.Now.Ticks + Environment.TickCount);
            TimeStampRequest request = tsqGenerator.Generate(DigestAlgorithms.GetAllowedDigests(digestAlgorithm), imprint, nonce);
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
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.tsa.1.response.code.2", tsaURL, value));
            }
            // @todo: validate the time stap certificate chain (if we want
            //        assure we do not sign using an invalid timestamp).
            
            // extract just the time stamp token (removes communication status info)
            TimeStampToken tsToken = response.TimeStampToken;
            if (tsToken == null) {
                throw new IOException(MessageLocalization.GetComposedMessage("tsa.1.failed.to.return.time.stamp.token.2", tsaURL, response.GetStatusString()));
            }
            TimeStampTokenInfo tsTokenInfo = tsToken.TimeStampInfo; // to view details
            byte[] encoded = tsToken.GetEncoded();
            
            LOGGER.Info("Timestamp generated: " + tsTokenInfo.GenTime);
            if (tsaInfo != null) {
                tsaInfo.InspectTimeStampTokenInfo(tsTokenInfo);
            }
            // Update our token size estimate for the next call (padded to be safe)
            this.tokenSizeEstimate = encoded.Length + 32;
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
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo), Base64FormattingOptions.None);
                con.Headers["Authorization"] = "Basic " + authInfo;
            }
            Stream outp = con.GetRequestStream();
            outp.Write(requestBytes, 0, requestBytes.Length);
            outp.Close();
            HttpWebResponse response = (HttpWebResponse)con.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.http.response.1", (int)response.StatusCode));
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
