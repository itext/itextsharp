using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.interfaces;

namespace com.itextpdf.text.pdf{

    public class ListBody : IAccessibleElement {

        protected PdfName role = PdfName.LBODY;
        protected AccessibleElementId id = new AccessibleElementId();
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected internal ListItem parentItem = null;
        protected internal float indentation = 0;

        protected internal ListBody(ListItem parentItem) {
            this.parentItem = parentItem;
        }

        public ListBody(ListItem parentItem, float indentation) : this(parentItem) {
            this.indentation = indentation;
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

        public virtual PdfName Role {
            get { return role; }
            set { this.role = value; }
        }

        public AccessibleElementId ID {
            get
            {
                if (id == null)
                    id = new AccessibleElementId();
                return id;
            }
            set { id = value; }
        }

    }
}
