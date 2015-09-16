using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using iTextSharp.text;
/*
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
     * PAdES-LTV Timestamp
     * @author Pulo Soares
     */
    public static class LtvTimestamp {
        /**
         * Signs a document with a PAdES-LTV Timestamp. The document is closed at the end.
         * @param sap the signature appearance
         * @param tsa the timestamp generator
         * @param signatureName the signature name or null to have a name generated
         * automatically
         * @throws Exception
         */
        public static void Timestamp(PdfSignatureAppearance sap, ITSAClient tsa, String signatureName) {
            int contentEstimated = tsa.GetTokenSizeEstimate();
            sap.AddDeveloperExtension(PdfDeveloperExtension.ESIC_1_7_EXTENSIONLEVEL5);
            sap.SetVisibleSignature(new Rectangle(0,0,0,0), 1, signatureName);

            PdfSignature dic = new PdfSignature(PdfName.ADOBE_PPKLITE, PdfName.ETSI_RFC3161);
            dic.Put(PdfName.TYPE, PdfName.DOCTIMESTAMP);
            sap.CryptoDictionary = dic;

            Dictionary<PdfName,int> exc = new Dictionary<PdfName,int>();
            exc[PdfName.CONTENTS] = contentEstimated * 2 + 2;
            sap.PreClose(exc);
            Stream data = sap.GetRangeStream();
            IDigest messageDigest = tsa.GetMessageDigest();
            byte[] buf = new byte[4096];
            int n;
            while ((n = data.Read(buf, 0, buf.Length)) > 0) {
                messageDigest.BlockUpdate(buf, 0, n);
            }
            byte[] tsImprint = new byte[messageDigest.GetDigestSize()];
            messageDigest.DoFinal(tsImprint, 0);
            byte[] tsToken;
            try {
        	    tsToken = tsa.GetTimeStampToken(tsImprint);
            }
            catch(Exception e) {
        	    throw new GeneralSecurityException(e.Message);
            }
            if (contentEstimated + 2 < tsToken.Length)
                throw new IOException("Not enough space");

            byte[] paddedSig = new byte[contentEstimated];
            System.Array.Copy(tsToken, 0, paddedSig, 0, tsToken.Length);

            PdfDictionary dic2 = new PdfDictionary();
            dic2.Put(PdfName.CONTENTS, new PdfString(paddedSig).SetHexWriting(true));
            sap.Close(dic2);
        }
    }
}
