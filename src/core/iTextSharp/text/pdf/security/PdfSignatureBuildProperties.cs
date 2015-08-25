/*
 * $Id: PdfSignatureBuildProperties.java 6037 2013-10-21 13:44:07Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Kwinten Pisman, et al.
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
     * Dictionary that stores signature build properties.
     * @author Kwinten Pisman
     */
    internal class PdfSignatureBuildProperties : PdfDictionary {

        /** Creates new PdfSignatureBuildProperties */
        public PdfSignatureBuildProperties() : base() {
        }

        /**
         * Sets the signatureCreator property in the underlying
         * {@link PdfSignatureAppDictionary} dictionary.
         * 
         * @param name
         */
        virtual public string SignatureCreator {
            set { GetPdfSignatureAppProperty().SignatureCreator = value; }
        }

        /**
         * Gets the {@link PdfSignatureAppDictionary} from this dictionary. If it
         * does not exist, it adds a new {@link PdfSignatureAppDictionary} and
         * returns this instance.
         * 
         * @return {@link PdfSignatureAppDictionary}
         */
        private PdfSignatureAppDictionary GetPdfSignatureAppProperty() {
            PdfSignatureAppDictionary appPropDic = (PdfSignatureAppDictionary) GetAsDict(PdfName.APP);
            if (appPropDic == null) {
                appPropDic = new PdfSignatureAppDictionary();
                Put(PdfName.APP, appPropDic);
            }
            return appPropDic;
        }
    }
}
