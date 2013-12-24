using System;
using iTextSharp.tool.xml.css.parser;

namespace iTextSharp.tool.xml.css.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class CommentInside : IState {

        private CssStateController controller;

        /**
         * @param cssStateController the controller
         */
        public CommentInside(CssStateController cssStateController) {
            this.controller = cssStateController;
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.css.parser.State#process(char)
         */
        virtual public void Process(char c) {
            if ('*' == c) {
                controller.StateCommentEnd();
            }

        }
    }
}
