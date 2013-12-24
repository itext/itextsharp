using System;
using iTextSharp.text;
/**
 *
 */
namespace iTextSharp.tool.xml.css.apply {

    /**
     * Classes implementing PageSizeContainable have a {@link Rectangle} in possession that defines a PageSize.
     * @author redlab_b
     *
     */
    public interface IPageSizeContainable {

	    /**
	     * returns the Rectangle that indicates a pagesize.
	     * @return the contained <code>Rectangle</code>
	     */
	    Rectangle PageSize {get;}
    }
}
