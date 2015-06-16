using System;
using iTextSharp.tool.xml.css.parser;

namespace iTextSharp.tool.xml.css.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class Rule : IState {

        private CssStateController controller;
        private bool isCss3AtRule;
        private int openParenthesesCount = 0;

        /**
         * @param cssStateController the controller
         */
        public Rule(CssStateController cssStateController) {
            this.controller= cssStateController;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.css.parser.State#process(char)
         */

        public virtual void Process(char c) {
            if ('}' == c && isCss3AtRule) {
                --openParenthesesCount;
                if (openParenthesesCount == 0) {
                    controller.StateUnknown();
                    isCss3AtRule = false;
                }
            } else if (';' == c && !isCss3AtRule) {
                controller.StateUnknown();
            } else if ('{' == c) {
                ++openParenthesesCount;
                isCss3AtRule = true;
            }
        }
    }
}
