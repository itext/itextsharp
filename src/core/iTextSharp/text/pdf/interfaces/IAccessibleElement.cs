using System;
using System.Collections.Generic;

namespace iTextSharp.text.pdf.interfaces
{
	public interface IAccessibleElement {
        /**
         * Get the attribute of accessible element (everything in <code>A</code> dictionary + <code>Lang</code>, <code>Alt</code>, <code>ActualText</code>, <code>E</code>).
         * @param key
         * @return
         */
        PdfObject GetAccessibleAttribute(PdfName key);

        /**
         * Set the attribute of accessible element (everything in <code>A</code> dictionary + <code>Lang</code>, <code>Alt</code>, <code>ActualText</code>, <code>E</code>).
         * @param key
         * @param value
         */
        void SetAccessibleAttribute(PdfName key, PdfObject value);

        /**
         * Gets all the properties of accessible element.
         * @return
         */
        Dictionary<PdfName, PdfObject> GetAccessibleAttributes();

        /**
         * Role propherty of the accessible element.
         * Note that all child elements won't also be tagged.
         * @return
         */
        PdfName Role { get; set; }

        Guid ID { get; set; }
    }
}
