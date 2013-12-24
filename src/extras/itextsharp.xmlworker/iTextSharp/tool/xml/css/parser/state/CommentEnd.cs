using System;
using iTextSharp.tool.xml.css.parser;

namespace iTextSharp.tool.xml.css.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class CommentEnd : IState {

        private CssStateController controller;

        /**
         * @param controller the controller
         */
        public CommentEnd(CssStateController controller) {
            this.controller = controller;
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.css.parser.State#process(char)
         */
        virtual public void Process(char c) {
            if ('/' == c) {
                this.controller.Previous();
            } else {
                this.controller.StateCommentInside();
            }

        }
    }
}
