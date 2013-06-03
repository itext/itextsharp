using System;
using System.Collections.Generic;
using iTextSharp.text.pdf.interfaces;

namespace iTextSharp.text.pdf
{
    public class PdfArtifact : IAccessibleElement {

        protected PdfName role = PdfName.ARTIFACT;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected Guid id = Guid.NewGuid();

        public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null)
                return accessibleAttributes[key];
            else
                return null;
        }

        public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }

        public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return accessibleAttributes;
        }

        public PdfName Role
        {
            get { return role; }
            set { role = value; }
        }

        public Guid ID
        {
            get { return id; }
            set { id = value; }
        }

        public PdfString Type {
            get {
                if (accessibleAttributes == null)
                    return null;
                PdfObject pdfObject;
                accessibleAttributes.TryGetValue(PdfName.TYPE, out pdfObject);
                return (PdfString) pdfObject;
            }
            set {
                SetAccessibleAttribute(PdfName.TYPE, value);
            }
        }

        public PdfArray BBox {
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

        public PdfArray Attached {
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
    }
}