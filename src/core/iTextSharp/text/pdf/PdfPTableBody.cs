using System.Collections.Generic;
using iTextSharp.text.pdf.interfaces;

namespace iTextSharp.text.pdf {

    public class PdfPTableBody : IAccessibleElement
    {

        protected AccessibleElementId id = new AccessibleElementId();
        protected internal List<PdfPRow> rows = null;
        protected PdfName role = PdfName.TBODY;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;

        public PdfPTableBody()
        {
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null) {
                PdfObject value;
                accessibleAttributes.TryGetValue(key, out value);
                return value;
            } else
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

        public virtual PdfName Role {
            get { return role; }
            set { this.role = value; }
        }

        virtual public AccessibleElementId ID {
            get { return id; }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return false; }
        }
    }
}
