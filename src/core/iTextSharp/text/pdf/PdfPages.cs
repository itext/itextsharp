using System;
using System.Collections.Generic;
using System.IO;
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
    * <CODE>PdfPages</CODE> is the PDF Pages-object.
    * <P>
    * The Pages of a document are accessible through a tree of nodes known as the Pages tree.
    * This tree defines the ordering of the pages in the document.<BR>
    * This object is described in the 'Portable Document Format Reference Manual version 1.3'
    * section 6.3 (page 71-73)
    *
    * @see        PdfPageElement
    * @see        PdfPage
    */

    public class PdfPages {
        
        private List<PdfIndirectReference> pages = new List<PdfIndirectReference>();
        private List<PdfIndirectReference> parents = new List<PdfIndirectReference>();
        private int leafSize = 10;
        private PdfWriter writer;
        private PdfIndirectReference topParent;
        
        // constructors
        
    /**
    * Constructs a <CODE>PdfPages</CODE>-object.
    */
        
        internal PdfPages(PdfWriter writer) {
            this.writer = writer;
        }
        
        internal void AddPage(PdfDictionary page) {
            if ((pages.Count % leafSize) == 0)
                parents.Add(writer.PdfIndirectReference);
            PdfIndirectReference parent = parents[parents.Count - 1];
            page.Put(PdfName.PARENT, parent);
            PdfIndirectReference current = writer.CurrentPage;
            writer.AddToBody(page, current);
            pages.Add(current);
        }
        
        internal PdfIndirectReference AddPageRef(PdfIndirectReference pageRef) {
            if ((pages.Count % leafSize) == 0)
                parents.Add(writer.PdfIndirectReference);
            pages.Add(pageRef);
            return parents[parents.Count - 1];
        }
        
        // returns the top parent to include in the catalog
        internal PdfIndirectReference WritePageTree() {
            if (pages.Count == 0)
                throw new IOException(MessageLocalization.GetComposedMessage("the.document.has.no.pages"));
            int leaf = 1;
            List<PdfIndirectReference> tParents = parents;
            List<PdfIndirectReference> tPages = pages;
            List<PdfIndirectReference> nextParents = new List<PdfIndirectReference>();
            while (true) {
                leaf *= leafSize;
                int stdCount = leafSize;
                int rightCount = tPages.Count % leafSize;
                if (rightCount == 0)
                    rightCount = leafSize;
                for (int p = 0; p < tParents.Count; ++p) {
                    int count;
                    int thisLeaf = leaf;
                    if (p == tParents.Count - 1) {
                        count = rightCount;
                        thisLeaf = pages.Count % leaf;
                        if (thisLeaf == 0)
                            thisLeaf = leaf;
                    }
                    else
                        count = stdCount;
                    PdfDictionary top = new PdfDictionary(PdfName.PAGES);
                    top.Put(PdfName.COUNT, new PdfNumber(thisLeaf));
                    PdfArray kids = new PdfArray();
                    List<PdfObject> intern = kids.ArrayList;
                    foreach (PdfObject obb in tPages.GetRange(p * stdCount, count))
                        intern.Add(obb);
                    top.Put(PdfName.KIDS, kids);
                    if (tParents.Count > 1) {
                        if ((p % leafSize) == 0)
                            nextParents.Add(writer.PdfIndirectReference);
                        top.Put(PdfName.PARENT, nextParents[p / leafSize]);
                    }
                    writer.AddToBody(top, tParents[p]);
                }
                if (tParents.Count == 1) {
                    topParent = tParents[0];
                    return topParent;
                }
                tPages = tParents;
                tParents = nextParents;
                nextParents = new List<PdfIndirectReference>();
            }
        }
        
        internal PdfIndirectReference TopParent {
            get {
                return topParent;
            }
        }
        
        internal void SetLinearMode(PdfIndirectReference topParent) {
            if (parents.Count > 1)
                throw new Exception(MessageLocalization.GetComposedMessage("linear.page.mode.can.only.be.called.with.a.single.parent"));
            if (topParent != null) {
                this.topParent = topParent;
                parents.Clear();
                parents.Add(topParent);
            }
            leafSize = 10000000;
        }

        internal void AddPage(PdfIndirectReference page) {
            pages.Add(page);
        }

        internal int ReorderPages(int[] order) {
            if (order == null)
                return pages.Count;
            if (parents.Count > 1)
                throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.a.single.parent.in.the.page.tree.call.pdfwriter.setlinearmode.after.open"));
            if (order.Length != pages.Count)
                throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.an.array.with.the.same.size.as.the.number.of.pages"));
            int max = pages.Count;
            bool[] temp = new bool[max];
            for (int k = 0; k < max; ++k) {
                int p = order[k];
                if (p < 1 || p > max)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.pages.between.1.and.1.found.2", max, p));
                if (temp[p - 1])
                    throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.no.page.repetition.page.1.is.repeated", p));
                temp[p - 1] = true;
            }
            PdfIndirectReference[] copy = pages.ToArray();
            for (int k = 0; k < max; ++k) {
                pages[k] = copy[order[k] - 1];
            }
            return max;
        }
    }
}
