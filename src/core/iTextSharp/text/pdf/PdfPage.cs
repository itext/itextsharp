using System;
using System.Collections.Generic;

using iTextSharp.text;
using iTextSharp.text.error_messages;

/*
 * $Id$
 * 
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

namespace iTextSharp.text.pdf {
    /**
     * <CODE>PdfPage</CODE> is the PDF Page-object.
     * <P>
     * A Page object is a dictionary whose keys describe a single page containing text,
     * graphics, and images. A Page onjects is a leaf of the Pages tree.<BR>
     * This object is described in the 'Portable Document Format Reference Manual version 1.3'
     * section 6.4 (page 73-81)
     *
     * @see     PdfPageElement
     * @see     PdfPages
     */

    public class PdfPage : PdfDictionary {
    
        // membervariables
        private static String[] boxStrings = {"crop", "trim", "art", "bleed"};
        private static PdfName[] boxNames = {PdfName.CROPBOX, PdfName.TRIMBOX, PdfName.ARTBOX, PdfName.BLEEDBOX};
    
        /** value of the <B>Rotate</B> key for a page in PORTRAIT */
        public static PdfNumber PORTRAIT = new PdfNumber(0);
    
        /** value of the <B>Rotate</B> key for a page in LANDSCAPE */
        public static PdfNumber LANDSCAPE = new PdfNumber(90);
    
        /** value of the <B>Rotate</B> key for a page in INVERTEDPORTRAIT */
        public static PdfNumber INVERTEDPORTRAIT = new PdfNumber(180);
    
        /** value of the <B>Rotate</B> key for a page in SEASCAPE */
        public static PdfNumber SEASCAPE = new PdfNumber(270);
    
        /** value of the <B>MediaBox</B> key */
        PdfRectangle mediaBox;
    
        // constructors

        /**
         * Constructs a <CODE>PdfPage</CODE>.
         *
         * @param		mediaBox		a value for the <B>MediaBox</B> key
         * @param		resources		an indirect reference to a <CODE>PdfResources</CODE>-object
         * @param		rotate			a value for the <B>Rotate</B> key
         * @throws DocumentException 
         */
    
        internal PdfPage(PdfRectangle mediaBox, Dictionary<string, PdfRectangle> boxSize, PdfDictionary resources, int rotate) : base(PAGE) {
            this.mediaBox = mediaBox;
            if (mediaBox != null && (mediaBox.Width > 14400 || mediaBox.Height > 14400)) {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.page.size.must.be.smaller.than.14400.by.14400.its.1.by.2", mediaBox.Width, mediaBox.Height));
            }
            Put(PdfName.MEDIABOX, mediaBox);
            Put(PdfName.RESOURCES, resources);
            if (rotate != 0) {
                Put(PdfName.ROTATE, new PdfNumber(rotate));
            }
            for (int k = 0; k < boxStrings.Length; ++k) {
                if (!boxSize.ContainsKey(boxStrings[k]))
                    continue;
                Put(boxNames[k], boxSize[boxStrings[k]]);
            }
        }

        /**
         * Constructs a <CODE>PdfPage</CODE>.
         *
         * @param		mediaBox		a value for the <B>MediaBox</B> key
         * @param		resources		an indirect reference to a <CODE>PdfResources</CODE>-object
         * @throws DocumentException 
         */
    
        internal PdfPage(PdfRectangle mediaBox, Dictionary<string, PdfRectangle> boxSize, PdfDictionary resources) : this(mediaBox, boxSize, resources, 0) {
        }
    
        /**
         * Checks if this page element is a tree of pages.
         * <P>
         * This method allways returns <CODE>false</CODE>.
         *
         * @return  <CODE>false</CODE> because this is a single page
         */
    
        virtual public bool IsParent() {
            return false;
        }
    
        // methods
    
        /**
         * Adds an indirect reference pointing to a <CODE>PdfContents</CODE>-object.
         *
         * @param       contents        an indirect reference to a <CODE>PdfContents</CODE>-object
         */
    
        internal void Add(PdfIndirectReference contents) {
            Put(PdfName.CONTENTS, contents);
        }
    
        /**
         * Rotates the mediabox, but not the text in it.
         *
         * @return      a <CODE>PdfRectangle</CODE>
         */
    
        internal PdfRectangle RotateMediaBox() {
            this.mediaBox =  mediaBox.Rotate;
            Put(PdfName.MEDIABOX, this.mediaBox);
            return this.mediaBox;
        }
    
        /**
         * Returns the MediaBox of this Page.
         *
         * @return      a <CODE>PdfRectangle</CODE>
         */
    
        internal PdfRectangle MediaBox {
            get {
                return mediaBox;
            }
        }
    }
}
