using System;
using Org.BouncyCastle.X509;
using iTextSharp.text.pdf.security;
/*
 * $Id: XmlSignatureAppearance.java 5830 2013-05-31 09:29:15Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Pavel Alay, Bruno Lowagie, et al.
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
namespace iTextSharp.text.pdf
{
    public class XmlSignatureAppearance {
         
        /**
         * Constructs XmlSignatureAppearance object.
         * @param writer the writer to which the signature will be written.
         */
        internal XmlSignatureAppearance(PdfStamperImp writer) {
            this.writer = writer;
        }

        private PdfStamperImp writer;
        private PdfStamper stamper;
        private X509Certificate signCertificate;
        private IXmlLocator xmlLocator;
        private IXpathConstructor xpathConstructor;
        
        /** Holds value of property xades:SigningTime. */
        private DateTime signDate = DateTime.MinValue;

        /** Holds value of property xades:Description. */
        private String description;

        /** Holds value of property xades:MimeType. */
        private String mimeType = "text/xml";



        virtual public PdfStamperImp GetWriter() {
            return writer;
        }

        virtual public PdfStamper GetStamper() {
            return stamper;
        }

        virtual public void SetStamper(PdfStamper stamper) {
            this.stamper = stamper;
        }

        /**
         * Sets the certificate used to provide the text in the appearance.
         * This certificate doesn't take part in the actual signing process.
         * @param signCertificate the certificate
         */
        virtual public void SetCertificate(X509Certificate signCertificate) {
            this.signCertificate = signCertificate;
        }

        virtual public X509Certificate GetCertificate() {
            return signCertificate;
        }


        virtual public void SetDescription(String description) {
            this.description = description;
        }

        virtual public String GetDescription() {
            return description;
        }

        virtual public String GetMimeType() {
            return mimeType;
        }

        virtual public void SetMimeType(String mimeType) {
            this.mimeType = mimeType;
        }

        /**
         * Gets the signature date.
         * @return the signature date
         */
        virtual public DateTime GetSignDate() {
            if(signDate == DateTime.MinValue)
                signDate = DateTime.Now;
            return signDate;
        }

        /**
         * Sets the signature date.
         * @param signDate the signature date
         */
        virtual public void SetSignDate(DateTime signDate) {
            this.signDate = signDate;
        }

        /**
         * Helps to locate xml stream
         * @return XmlLocator, cannot be null.
         */
        virtual public IXmlLocator GetXmlLocator() {
            return xmlLocator;
        }


        virtual public void SetXmlLocator(IXmlLocator xmlLocator) {
            this.xmlLocator = xmlLocator;
        }

        /**
         * Constructor for xpath expression in case signing only part of XML document.
         * @return XpathConstructor, can be null
         */
        virtual public IXpathConstructor GetXpathConstructor() {
            return xpathConstructor;
        }

        virtual public void SetXpathConstructor(IXpathConstructor xpathConstructor) {
            this.xpathConstructor = xpathConstructor;
        }

        /**
         * Close PdfStamper
         * @throws IOException
         * @throws DocumentException
         */
        virtual public void Close() {
            writer.Close(stamper.MoreInfo);
        }
    }
}
