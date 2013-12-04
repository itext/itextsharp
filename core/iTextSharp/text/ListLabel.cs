using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace com.itextpdf.text.pdf
{
    public class ListLabel : ListBody
    {

        protected bool tagLabelContent = true;

        protected internal ListLabel(ListItem parentItem) : base(parentItem)
        {
            role = PdfName.LBL;
            indentation = 0;
        }

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

        public override PdfName Role
        {
            get { return role; }
            set { role = value; }
        }

        public float Indentation
        {
            get { return indentation; }
            set { indentation = value; }
        }

        public bool TagLabelContent { 
            /**
            * Gets the value of <code>tagLabelContent</code> property.
            * If the property is <code>true</code> it means that content of the list item lable will be tagged.
            * For example:
            * <code>
            * &lt;LI&gt;
            *     &lt;Lbl&gt;
            *         &lt;Span&gt;1.&lt;/Span&gt;
            *     &lt;/Lbl&gt;
            * &lt;/LI&gt;
            * </code>
            * If the property set to <code>false</code> it will look as follows:
            * <code>
            * &lt;LI&gt;
            *     &lt;Lbl&gt;1.&lt;/Lbl&gt;
            * &lt;/LI&gt;
            * @return
            */
            get { return tagLabelContent; }
            set { tagLabelContent = value; }
        }
    }
}
