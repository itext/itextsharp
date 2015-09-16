using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;
using iTextSharp.text.log;
using iTextSharp.text.error_messages;
/*
 * $Id: CrlClientOnline.java 5302 2012-08-13 07:30:41Z blowagie $
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
namespace iTextSharp.text.pdf.security {

    /**
     * An implementation of the CrlClient that fetches the CRL bytes
     * from an URL.
     * @author Paulo Soares
     */
    public class CrlClientOnline : ICrlClient {

        /** The Logger instance. */
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(CrlClientOnline));
        
        /** The URLs of the CRLs. */
        protected IList<string> urls = new List<string>();

        /**
         * Creates a CrlClientOnline instance that will try to find
         * a single CRL by walking through the certificate chain.
         */
        public CrlClientOnline() {
        }
        
        /**
         * Creates a CrlClientOnline instance using one or more URLs.
         */
        public CrlClientOnline(params String[] crls) {
            foreach (String url in crls) {
                AddUrl(url);
            }
        }
        
        /**
         * Creates a CrlClientOnline instance using a certificate chain.
         */
        public CrlClientOnline(ICollection<X509Certificate> chain) {
            foreach (X509Certificate cert in chain) {
                String url = null;
                try {
                    LOGGER.Info("Checking certificate: " + cert.SubjectDN.ToString());
                    url = CertificateUtil.GetCRLURL(cert);
                    if (url != null) {
                        AddUrl(url);
                    }
                }
                catch (CertificateParsingException)
                {
                    LOGGER.Info("Skipped CRL url: (certificate could not be parsed)");
                }
            }
        }

        /**
         * Adds an URL to the list of CRL URLs
         * @param url	an URL in the form of a String
         */
        virtual protected void AddUrl(String url)
        {
            if (urls.Contains(url))
            {
                LOGGER.Info("Skipped CRL url (duplicate): " + url);
                return;
            }

            LOGGER.Info("Added CRL url: " + url);
            urls.Add(url);
        }

        /**
         * Fetches the CRL bytes from an URL.
         * If no url is passed as parameter, the url will be obtained from the certificate.
         * If you want to load a CRL from a local file, subclass this method and pass an
         * URL with the path to the local file to this method. An other option is to use
         * the CrlClientOffline class.
         * @see com.itextpdf.text.pdf.security.CrlClient#getEncoded(java.security.cert.X509Certificate, java.lang.String)
         */
        virtual public ICollection<byte[]> GetEncoded(X509Certificate checkCert, String url) {
            if (checkCert == null)
                return null;
            List<String> urllist = new List<string>(urls);
            if (urllist.Count == 0) {
                LOGGER.Info("Looking for CRL for certificate " + checkCert.SubjectDN.ToString());
                try {
                    if (url == null)
                        url = CertificateUtil.GetCRLURL(checkCert);
                    if (url == null)
                        throw new ArgumentNullException();
                    urllist.Add(url);
                    LOGGER.Info("Found CRL url: " + url);
                }
                catch (Exception e) {
                    LOGGER.Info("Skipped CRL url: " + e.Message);
                }
            }
            List<byte[]> ar = new List<byte[]>();
            foreach (string urlt in urllist) {
                try {
                    LOGGER.Info("Checking CRL: " + urlt);
                    HttpWebRequest con = (HttpWebRequest)WebRequest.Create(urlt);
                    HttpWebResponse response = (HttpWebResponse)con.GetResponse();
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new IOException(MessageLocalization.GetComposedMessage("invalid.http.response.1", (int)response.StatusCode));
                    //Get Response
                    Stream inp = response.GetResponseStream();
                    byte[] buf = new byte[1024];
                    MemoryStream bout = new MemoryStream();
                    while (true) {
                        int n = inp.Read(buf, 0, buf.Length);
                        if (n <= 0)
                            break;
                        bout.Write(buf, 0, n);
                    }
                    inp.Close();
                    ar.Add(bout.ToArray());
                    LOGGER.Info("Added CRL found at: " + urlt);
                }
                catch (Exception e) {
                    LOGGER.Info("Skipped CRL: " + e.Message + " for " + urlt);
                }
            }
            return ar;
        }
    }
}
