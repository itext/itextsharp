using System;
using iTextSharp.tool.xml.css.parser;

namespace iTextSharp.tool.xml.css.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class Unknown : IState {

        private CssStateController controller;

        /**
         * @param cssStateController the controller
         */
        public Unknown(CssStateController cssStateController) {
            this.controller= cssStateController;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.css.parser.State#process(char)
         */
        public void Process(char c) {
            if ('/' == c) {
                controller.StateCommentStart();
            } else if ('{' == c) {
                controller.StoreSelector();
                controller.StateProperties();
            } else if ('@' == c) {
                controller.StateRule();
            } else {
                controller.Append(c);
            }

        }
    }
}