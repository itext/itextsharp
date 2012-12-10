using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.interfaces;

namespace com.itextpdf.text.pdf{

public class PdfListBody : IAccessibleElement {

    protected PdfName role = PdfName.LBODY;
    protected Guid id = Guid.NewGuid();
    protected internal ListItem parentItem = null;
    protected internal float indentation = 0;

    public PdfListBody() {

    }

    public PdfListBody( ListItem parentItem) : this() {
        this.parentItem = parentItem;
    }

    public PdfListBody(ListItem parentItem, float indentation) : this(parentItem) {
        this.indentation = indentation;
    }

    public PdfObject GetAccessibleProperty(PdfName key) {
        return null;
    }

    public void SetAccessibleProperty( PdfName key,  PdfObject value) {

    }

    public Dictionary<PdfName, PdfObject> GetAccessibleProperties() {
        return null;
    }

    public PdfName Role {
        get { return role; }
        set { role = value; }
    }


    public Guid ID {
        get { return id; }
    }

}
}
