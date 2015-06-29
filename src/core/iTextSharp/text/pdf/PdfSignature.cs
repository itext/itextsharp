/*
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
using iTextSharp.text.pdf.security;

namespace iTextSharp.text.pdf {

    /** Implements the signature dictionary.
     *
     * @author Paulo Soares
     */
    public class PdfSignature : PdfDictionary {

        /** Creates new PdfSignature */
        public PdfSignature(PdfName filter, PdfName subFilter) : base(PdfName.SIG) {
            Put(PdfName.FILTER, filter);
            Put(PdfName.SUBFILTER, subFilter);
        }
        
        virtual public int[] ByteRange {
            set {
                PdfArray array = new PdfArray();
                for (int k = 0; k < value.Length; ++k)
                    array.Add(new PdfNumber(value[k]));
                Put(PdfName.BYTERANGE, array);
            }
        }
        
        virtual public byte[] Contents {
            set {
                Put(PdfName.CONTENTS, new PdfString(value).SetHexWriting(true));
            }
        }
        
        virtual public byte[] Cert {
            set {
                Put(PdfName.CERT, new PdfString(value));
            }
        }
        
        virtual public string Name {
            set {
                Put(PdfName.NAME, new PdfString(value, PdfObject.TEXT_UNICODE));
            }
        }

        virtual public PdfDate Date {
            set {
                Put(PdfName.M, value);
            }
        }

        virtual public string Location {
            set {
                Put(PdfName.LOCATION, new PdfString(value, PdfObject.TEXT_UNICODE));
            }
        }

        virtual public string Reason {
            set {
                Put(PdfName.REASON, new PdfString(value, PdfObject.TEXT_UNICODE));
            }
        }

        /**
         * Sets the signature creator name in the
         * {@link PdfSignatureBuildProperties} dictionary.
         * 
         * @param name
         */
        virtual public string SignatureCreator {
            set {
                if (value != null) {
                    PdfSignatureBuildProperties.SignatureCreator = value;
                }
            }
        }

        /**
         * Gets the {@link PdfSignatureBuildProperties} instance if it exists, if
         * not it adds a new one and returns this.
         * 
         * @return {@link PdfSignatureBuildProperties}
         */
        private PdfSignatureBuildProperties PdfSignatureBuildProperties {
            get {
                PdfSignatureBuildProperties buildPropDic = (PdfSignatureBuildProperties) GetAsDict(PdfName.PROP_BUILD);
                if (buildPropDic == null) {
                    buildPropDic = new PdfSignatureBuildProperties();
                    Put(PdfName.PROP_BUILD, buildPropDic);
                }
                return buildPropDic;
            }
        }
        
        virtual public string Contact {
            set {
                Put(PdfName.CONTACTINFO, new PdfString(value, PdfObject.TEXT_UNICODE));
            }
        }
    }
}
