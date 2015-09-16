using System;

using iTextSharp.text;

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
     * Allows a class to catch several document events.
     *
     * @author  Paulo Soares
     */

    public interface IPdfPageEvent {
    
        /**
         * Called when the document is opened.
         *
         * @param writer the <CODE>PdfWriter</CODE> for this document
         * @param document the document
         */
        void OnOpenDocument(PdfWriter writer, Document document);

        /**
         * Called when a page is initialized.
         * <P>
         * Note that if even if a page is not written this method is still
         * called. It is preferable to use <CODE>onEndPage</CODE> to avoid
         * infinite loops.
         * </P>
         * <P>
         * Note that this method isn't called for the first page. You should apply modifications for the first
         * page either before opening the document or by using the onOpenDocument() method.
         * </P>
         *
         * @param writer the <CODE>PdfWriter</CODE> for this document
         * @param document the document
         */
        void OnStartPage(PdfWriter writer, Document document);
    
        /**
         * Called when a page is finished, just before being written to the document.
         *
         * @param writer the <CODE>PdfWriter</CODE> for this document
         * @param document the document
         */
        void OnEndPage(PdfWriter writer, Document document);
    
        /**
         * Called when the document is closed.
         * <P>
         * Note that this method is called with the page number equal
         * to the last page plus one.
         *
         * @param writer the <CODE>PdfWriter</CODE> for this document
         * @param document the document
         */
        void OnCloseDocument(PdfWriter writer, Document document);
    
        /**
         * Called when a Paragraph is written.
         * <P>
         * <CODE>paragraphPosition</CODE> will hold the height at which the
         * paragraph will be written to. This is useful to insert bookmarks with
         * more control.
         *
         * @param writer the <CODE>PdfWriter</CODE> for this document
         * @param document the document
         * @param paragraphPosition the position the paragraph will be written to
         */
        void OnParagraph(PdfWriter writer, Document document, float paragraphPosition);
    
        /**
         * Called when a Paragraph is written.
         * <P>
         * <CODE>paragraphPosition</CODE> will hold the height of the end of the paragraph.
         *
         * @param writer the <CODE>PdfWriter</CODE> for this document
         * @param document the document
         * @param paragraphPosition the position of the end of the paragraph
         */
        void OnParagraphEnd(PdfWriter writer,Document document,float paragraphPosition);
    
        /**
         * Called when a Chapter is written.
         * <P>
         * <CODE>position</CODE> will hold the height at which the
         * chapter will be written to.
         *
         * @param writer            the <CODE>PdfWriter</CODE> for this document
         * @param document          the document
         * @param paragraphPosition the position the chapter will be written to
         * @param title             the title of the Chapter
         */
        void OnChapter(PdfWriter writer,Document document,float paragraphPosition, Paragraph title);
    
        /**
         * Called when the end of a Chapter is reached.
         * <P>
         * <CODE>position</CODE> will hold the height of the end of the chapter.
         *
         * @param writer            the <CODE>PdfWriter</CODE> for this document
         * @param document          the document
         * @param paragraphPosition the position the chapter will be written to
         */
        void OnChapterEnd(PdfWriter writer,Document document,float paragraphPosition);
    
        /**
         * Called when a Section is written.
         * <P>
         * <CODE>position</CODE> will hold the height at which the
         * section will be written to.
         *
         * @param writer            the <CODE>PdfWriter</CODE> for this document
         * @param document          the document
         * @param paragraphPosition the position the chapter will be written to
         * @param title             the title of the Chapter
         */
        void OnSection(PdfWriter writer,Document document,float paragraphPosition, int depth, Paragraph title);
    
        /**
         * Called when the end of a Section is reached.
         * <P>
         * <CODE>position</CODE> will hold the height of the section end.
         *
         * @param writer            the <CODE>PdfWriter</CODE> for this document
         * @param document          the document
         * @param paragraphPosition the position the chapter will be written to
         */
        void OnSectionEnd(PdfWriter writer,Document document,float paragraphPosition);
    
        /**
         * Called when a <CODE>Chunk</CODE> with a generic tag is written.
         * <P>
         * It is usefull to pinpoint the <CODE>Chunk</CODE> location to generate
         * bookmarks, for example.
         *
         * @param writer the <CODE>PdfWriter</CODE> for this document
         * @param document the document
         * @param rect the <CODE>Rectangle</CODE> containing the <CODE>Chunk</CODE>
         * @param text the text of the tag
         */
        void OnGenericTag(PdfWriter writer, Document document, Rectangle rect, string text);
    }
}
