using System;
using iTextSharp.tool.xml.css.parser;

namespace iTextSharp.tool.xml.css.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class Properties : IState {

        private CssStateController controller;

        /**
         * @param cssStateController the controller
         */
        public Properties(CssStateController cssStateController) {
            this.controller = cssStateController;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.css.parser.State#process(char)
         */
        public void Process(char c) {
            if ('}' == c) {
                controller.StoreProperties();
                controller.StateUnknown();
            } else if ('/' == c) {
                controller.StateCommentStart();
            } else {
                controller.Append(c);
            }
        }
    }
}