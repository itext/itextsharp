using System;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.interfaces;

namespace iTextSharp.text.pdf {

    public class PdfPTableBody : IAccessibleElement
    {

        protected Guid id = Guid.NewGuid();
        protected internal List<PdfPRow> rows = null;
        protected PdfName role = PdfName.TBODY;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;

        public PdfPTableBody()
        {

        }

        public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null) {
                PdfObject value;
                accessibleAttributes.TryGetValue(key, out value);
                return value;
            } else
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

        public PdfName Role {
            get { return role; }
            set { this.role = value; }
        }

        public Guid ID {
            get { return id; }
            set { id = value; }
        }

    }
}