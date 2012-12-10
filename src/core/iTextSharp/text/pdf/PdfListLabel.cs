using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.interfaces;

namespace com.itextpdf.text.pdf
{
    public class PdfListLabel : IAccessibleElement
    {

        protected PdfName role = PdfName.LBL;
        protected Guid id = Guid.NewGuid();

        protected PdfContentByte canvas;
        protected float x;
        protected float y;
        protected Chunk listSymbol;

        public PdfObject GetAccessibleProperty(PdfName key)
        {
            return null;
        }

        public void SetAccessibleProperty(PdfName key, PdfObject value)
        {

        }

        public Dictionary<PdfName, PdfObject> GetAccessibleProperties()
        {
            return null;
        }

        public PdfName Role
        {
            get { return role; }
            set { role = value; }
        }
        
        public Guid ID
        {
            get { return id; }
        }

    }
}
