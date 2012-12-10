using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;

namespace iTextSharp.text.pdf.interfaces
{
	public interface IAccessibleElement {
        /**
         * Get the proeprty of accessible element (i.e. alternate text).
         * @param key
         * @return
         */
        PdfObject GetAccessibleProperty(PdfName key);

        /**
         * Sets the property of accessible element (i.e. alternate text).
         * @param key
         * @param value
         */
        void SetAccessibleProperty(PdfName key, PdfObject value);

        /**
         * Gets all the properties of accessible element.
         * @return
         */
        Dictionary<PdfName, PdfObject> GetAccessibleProperties();

        /**
         * Role propherty of the accessible element.
         * @return
         */
	    PdfName Role { get; set; }

	    Guid ID { get; }
    }
}
