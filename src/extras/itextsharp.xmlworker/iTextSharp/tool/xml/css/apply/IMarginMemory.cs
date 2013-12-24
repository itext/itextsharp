using System;
using System.Collections.Generic;
/**
 *
 */
namespace iTextSharp.tool.xml.css.apply {

    /**
     * The marginmemory helps remembering the last margin bottom and roottags. These are needed to calculate the right
     * margins for some elements when applying CSS.
     *
     * @author redlab_b
     *
     */
    public interface IMarginMemory {

        /**
         * @return a Float
         * @throws NoDataException if there is no LastMarginBottom set
         */
        float LastMarginBottom {get; set;}

        /**
         * @return a list of roottags
         */
        IList<String> GetRootTags();
    }
}
