using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.error_messages;

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
namespace iTextSharp.text.pdf.events {

    /**
    * Class for an index.
    * 
    * @author Michael Niedermair
    */
    public class FieldPositioningEvents : PdfPageEventHelper, IPdfPCellEvent {

        /**
        * Keeps a map with fields that are to be positioned in inGenericTag.
        */
        protected Dictionary<String, PdfFormField> genericChunkFields = new Dictionary<string,PdfFormField>();

        /**
        * Keeps the form field that is to be positioned in a cellLayout event.
        */
        protected PdfFormField cellField = null;
        
        /**
        * The PdfWriter to use when a field has to added in a cell event. 
        */
        protected PdfWriter fieldWriter = null;
        /**
        * The PdfFormField that is the parent of the field added in a cell event. 
        */
        protected PdfFormField parent = null;
        
        /** Creates a new event. This constructor will be used if you need to position fields with Chunk objects. */
        public FieldPositioningEvents() {}
        
        /** Some extra padding that will be taken into account when defining the widget. */
        public float padding;
        
        /**
        * Add a PdfFormField that has to be tied to a generic Chunk.
        */
        virtual public void AddField(String text, PdfFormField field) {
            genericChunkFields[text] = field;
        }
        
        /** Creates a new event. This constructor will be used if you need to position fields with a Cell Event. */
        public FieldPositioningEvents(PdfWriter writer, PdfFormField field) {
            this.cellField = field;
            this.fieldWriter = writer;
        }  
        
        /** Creates a new event. This constructor will be used if you need to position fields with a Cell Event. */
        public FieldPositioningEvents(PdfFormField parent, PdfFormField field) {
            this.cellField = field;
            this.parent = parent;
        }
        
        /** Creates a new event. This constructor will be used if you need to position fields with a Cell Event. 
        * @throws DocumentException
        * @throws IOException*/
        public FieldPositioningEvents(PdfWriter writer, String text) {
            this.fieldWriter = writer;
            TextField tf = new TextField(writer, new Rectangle(0, 0), text);
            tf.FontSize = 14;
            cellField = tf.GetTextField();
        }   
            
        /** Creates a new event. This constructor will be used if you need to position fields with a Cell Event. 
        * @throws DocumentException
        * @throws IOException*/
        public FieldPositioningEvents(PdfWriter writer, PdfFormField parent, String text) {
            this.parent = parent;
            TextField tf = new TextField(writer, new Rectangle(0, 0), text);
            tf.FontSize = 14;
            cellField = tf.GetTextField();
        }  

        /**
        * @param padding The padding to set.
        */
        virtual public float Padding {
            set {
                padding = value;
            }
            get {
                return padding;
            }
        }
        
        /**
        * @param parent The parent to set.
        */
        virtual public PdfFormField Parent {
            set {
                parent = value;
            }
            get {
                return parent;
            }
        }

        /**
        * @see com.lowagie.text.pdf.PdfPageEvent#onGenericTag(com.lowagie.text.pdf.PdfWriter, com.lowagie.text.Document, com.lowagie.text.Rectangle, java.lang.String)
        */
        public override void OnGenericTag(PdfWriter writer, Document document,
                Rectangle rect, String text) {
            rect.Bottom = rect.Bottom - 3;
            PdfFormField field;
            genericChunkFields.TryGetValue(text, out field);
            if (field == null) {
                TextField tf = new TextField(writer, new Rectangle(rect.GetLeft(padding), rect.GetBottom(padding), rect.GetRight(padding), rect.GetTop(padding)), text);
                tf.FontSize = 14;
                field = tf.GetTextField();
            }
            else {
                field.Put(PdfName.RECT,  new PdfRectangle(rect.GetLeft(padding), rect.GetBottom(padding), rect.GetRight(padding), rect.GetTop(padding)));
            }
            if (parent == null)
                writer.AddAnnotation(field);
            else
                parent.AddKid(field);
        }

        /**
        * @see com.lowagie.text.pdf.PdfPCellEvent#cellLayout(com.lowagie.text.pdf.PdfPCell, com.lowagie.text.Rectangle, com.lowagie.text.pdf.PdfContentByte[])
        */
        virtual public void CellLayout(PdfPCell cell, Rectangle rect, PdfContentByte[] canvases) {
            if (cellField == null || (fieldWriter == null && parent == null)) throw new ArgumentException(MessageLocalization.GetComposedMessage("you.have.used.the.wrong.constructor.for.this.fieldpositioningevents.class"));
            cellField.Put(PdfName.RECT, new PdfRectangle(rect.GetLeft(padding), rect.GetBottom(padding), rect.GetRight(padding), rect.GetTop(padding)));
            if (parent == null)
                fieldWriter.AddAnnotation(cellField);
            else
                parent.AddKid(cellField);
        }
    }
}
