using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;

namespace iTextSharp.tool.xml
{

/**
 * Implementation of the <code>ElementHandler</code> interface that helps
 * you build a list of iText <code>Element</code>s.
 */

    public class ElementList : List<IElement>, IElementHandler
    {

        /**
	 * @see com.itextpdf.tool.xml.ElementHandler#add(com.itextpdf.tool.xml.Writable)
	 */

        virtual public void Add(IWritable w)
        {
            if (w is WritableElement) {
                foreach (IElement element in ((WritableElement)w).Elements())
                    this.Add(element);
            }
        }

        /** Serial version UID */
        private const long serialVersionUID = -3943194552607332537L;
    }
}
