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
using System.Collections.Generic;
using iTextSharp.text.pdf.interfaces;

namespace iTextSharp.text.pdf {

    /**
    * Implements the form XObject.
    */
    public class PdfTemplate : PdfContentByte, IAccessibleElement {
        public const int TYPE_TEMPLATE = 1;
        public const int TYPE_IMPORTED = 2;
        public const int TYPE_PATTERN = 3;
        protected int type;
        /** The indirect reference to this template */
        protected PdfIndirectReference thisReference;
        
        /** The resources used by this template */
        protected PageResources pageResources;
        
        /** The bounding box of this template */
        protected Rectangle bBox = new Rectangle(0, 0);
        
        protected PdfArray matrix;
        
        protected PdfTransparencyGroup group;
        
        protected IPdfOCG layer;

        protected PdfIndirectReference pageReference;

        protected bool contentTagged = false;

        /**
         * A dictionary with additional information
         * @since 5.1.0
         */
        private PdfDictionary additional = null;

        protected PdfName role = PdfName.FIGURE;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        private AccessibleElementId id;
        
        /**
        *Creates a <CODE>PdfTemplate</CODE>.
        */
        protected PdfTemplate() : base(null) {
            type = TYPE_TEMPLATE;
        }
        
        /**
        * Creates new PdfTemplate
        *
        * @param wr the <CODE>PdfWriter</CODE>
        */
        internal PdfTemplate(PdfWriter wr) : base(wr) {
            type = TYPE_TEMPLATE;
            pageResources = new PageResources();
            pageResources.AddDefaultColor(wr.DefaultColorspace);
            thisReference = writer.PdfIndirectReference;
        }
        
        /**
         * Creates a new template.
         * <P>
         * Creates a new template that is nothing more than a form XObject. This template can be included
         * in this <CODE>PdfContentByte</CODE> or in another template. Templates are only written
         * to the output when the document is closed permitting things like showing text in the first page
         * that is only defined in the last page.
         *
         * @param width the bounding box width
         * @param height the bounding box height
         * @return the templated created
         */
        public static PdfTemplate CreateTemplate(PdfWriter writer, float width, float height) {
            return CreateTemplate(writer, width, height, null);
        }
        
        internal static PdfTemplate CreateTemplate(PdfWriter writer, float width, float height, PdfName forcedName) {
            PdfTemplate template = new PdfTemplate(writer);
            template.Width = width;
            template.Height = height;
            writer.AddDirectTemplateSimple(template, forcedName);
            return template;
        }

        public override bool IsTagged() {
            return base.IsTagged() && contentTagged;
        }

        /**
        * Gets the bounding width of this template.
        *
        * @return width the bounding width
        */
        virtual public float Width {
            get {
                return bBox.Width;
            }

            set {
                bBox.Left = 0;
                bBox.Right = value;
            }
        }
        
        /**
        * Gets the bounding heigth of this template.
        *
        * @return heigth the bounding height
        */
        
        virtual public float Height {
            get {
                return bBox.Height;
            }

            set {
                bBox.Bottom = 0;
                bBox.Top = value;
            }
        }
        
        virtual public Rectangle BoundingBox {
            get {
                return bBox;
            }
            set {
                this.bBox = value;
            }
        }
        
        /**
        * Gets the layer this template belongs to.
        * @return the layer this template belongs to or <code>null</code> for no layer defined
        */
        virtual public IPdfOCG Layer {
            get {
                return layer;
            }
            set {
                layer = value;
            }
        }

        virtual public void SetMatrix(float a, float b, float c, float d, float e, float f) {
            matrix = new PdfArray();
            matrix.Add(new PdfNumber(a));
            matrix.Add(new PdfNumber(b));
            matrix.Add(new PdfNumber(c));
            matrix.Add(new PdfNumber(d));
            matrix.Add(new PdfNumber(e));
            matrix.Add(new PdfNumber(f));
        }

        internal PdfArray Matrix {
            get {
                return matrix;
            }
        }
        
        /**
        * Gets the indirect reference to this template.
        *
        * @return the indirect reference to this template
        */
        
        virtual public PdfIndirectReference IndirectReference {
            get {
    	        // uncomment the null check as soon as we're sure all examples still work
    	        if (thisReference == null /* && writer != null */) {
    		        thisReference = writer.PdfIndirectReference;
    	        }
                return thisReference;
            }
        }
        
        virtual public void BeginVariableText() {
            content.Append("/Tx BMC ");
        }
        
        virtual public void EndVariableText() {
            content.Append("EMC ");
        }
        
        /**
        * Constructs the resources used by this template.
        *
        * @return the resources used by this template
        */
        
        internal virtual PdfObject Resources {
            get {
                return PageResources.Resources;
            }
        }
        
        /**
        * Gets the stream representing this template.
        *
        * @param   compressionLevel    the compressionLevel
        * @return the stream representing this template
        * @since   2.1.3   (replacing the method without param compressionLevel)
        */
        virtual public PdfStream GetFormXObject(int compressionLevel) {
            return new PdfFormXObject(this, compressionLevel);
        }
        
        /**
        * Gets a duplicate of this <CODE>PdfTemplate</CODE>. All
        * the members are copied by reference but the buffer stays different.
        * @return a copy of this <CODE>PdfTemplate</CODE>
        */
        
        public override PdfContentByte Duplicate {
            get {
                PdfTemplate tpl = new PdfTemplate();
                tpl.writer = writer;
                tpl.pdf = pdf;
                tpl.thisReference = thisReference;
                tpl.pageResources = pageResources;
                tpl.bBox = new Rectangle(bBox);
                tpl.group = group;
                tpl.layer = layer;
                if (matrix != null) {
                    tpl.matrix = new PdfArray(matrix);
                }
                tpl.separator = separator;
                tpl.additional = additional;
                return tpl;
            }
        }
        
        virtual public int Type {
            get {
                return type;
            }
        }

        internal override PageResources PageResources {
            get {
                return pageResources;
            }
        }
        
        public virtual PdfTransparencyGroup Group {
            get {
                return this.group;
            }
            set {
                group = value;
            }
        }

        /**
         * Sets/gets a dictionary with extra entries, for instance /Measure.
         *
         * @param additional
         *            a PdfDictionary with additional information.
         * @since 5.1.0
         */
        virtual public PdfDictionary Additional {
            set {
                additional = value;
            }
            get {
                return additional;
            }
        }

        protected override PdfIndirectReference CurrentPage {
            get { return pageReference ?? writer.CurrentPage; }
        }

        virtual public PdfIndirectReference PageReference
        {
            get { return pageReference; }
            set { pageReference = value; }
        }

        virtual public bool ContentTagged
        {
            get { return contentTagged; }
            set { contentTagged = value; }
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null)
            {
                PdfObject obj;
                accessibleAttributes.TryGetValue(key, out obj);
                return obj;
            }
            else
                return null;
        }

        virtual public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }

        virtual public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return accessibleAttributes;
        }

        virtual public PdfName Role {
            get { return role; }
            set { role = value; }
        }

        virtual public AccessibleElementId ID {
            get {
                if (id == null)
                    id = new AccessibleElementId();
                return id;
            }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return true; }
        }
    }
}
