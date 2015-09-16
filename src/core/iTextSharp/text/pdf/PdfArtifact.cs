using System;
using System.Collections.Generic;
using System.util.collections;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.interfaces;
/*
 * $Id: PdfArtifact.java 5830 2013-05-31 09:29:15Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Alexander Chingarev, Bruno Lowagie, et al.
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
    public class PdfArtifact : IAccessibleElement {

        protected PdfName role = PdfName.ARTIFACT;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected AccessibleElementId id = new AccessibleElementId();

        private static readonly HashSet2<String> allowedArtifactTypes = new HashSet2<string>(new string[] { "Pagination", "Layout", "Page", "Background" }); 

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null)
                return accessibleAttributes[key];
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

        virtual public PdfName Role
        {
            get { return role; }
            set { role = value; }
        }

        virtual public AccessibleElementId ID
        {
            get { return id; }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return true; }
        }

        virtual public PdfString Type {
            get {
                if (accessibleAttributes == null)
                    return null;
                PdfObject pdfObject;
                accessibleAttributes.TryGetValue(PdfName.TYPE, out pdfObject);
                return (PdfString) pdfObject;
            }
            set {
                if (!allowedArtifactTypes.Contains(value.ToString()))
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.artifact.type.1.is.invalid", value));
                SetAccessibleAttribute(PdfName.TYPE, value);
            }
        }

        virtual public void SetType(PdfArtifact.ArtifactType type) {
            PdfString artifactType = null;
            switch (type)
            {
                case ArtifactType.BACKGROUND:
                    artifactType = new PdfString("Background");
                    break;
                case ArtifactType.LAYOUT:
                    artifactType = new PdfString("Layout");
                    break;
                case ArtifactType.PAGE:
                    artifactType = new PdfString("Page");
                    break;
                case ArtifactType.PAGINATION:
                    artifactType = new PdfString("Pagination");
                    break;
            }
            SetAccessibleAttribute(PdfName.TYPE, artifactType);
        }

        virtual public PdfArray BBox {
            get {
                if (accessibleAttributes == null)
                    return null;
                PdfObject pdfObject;
                accessibleAttributes.TryGetValue(PdfName.BBOX, out pdfObject);
                return (PdfArray) pdfObject;
            }
            set {
                SetAccessibleAttribute(PdfName.BBOX, value);
            }
        }

        virtual public PdfArray Attached {
            get {
                if(accessibleAttributes == null)
                    return null;
                PdfObject pdfObject;
                accessibleAttributes.TryGetValue(PdfName.ATTACHED, out pdfObject);
                return (PdfArray)pdfObject;
            }
            set {
                SetAccessibleAttribute(PdfName.ATTACHED, value);
            }
        }

        public enum ArtifactType {
            PAGINATION,
            LAYOUT,
            PAGE,
            BACKGROUND
        }
    }
}
