using System;
using iTextSharp.tool.xml.css.parser;

namespace iTextSharp.tool.xml.css.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class CommentStart : IState {

        private CssStateController controller;
        /**
         * @param controller  the controller
         *
         */
        public CommentStart(CssStateController controller) {
            this.controller = controller;
        }
        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.parser.State#process(int)
         */
        virtual public void Process(char c) {
            if ('*' == c) {
                this.controller.StateCommentInside();
            } else {
                this.controller.Append('/');
                this.controller.Append(c);
                this.controller.Previous();
            }
        }

    }
}
