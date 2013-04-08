using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.css.parser.state;

namespace iTextSharp.tool.xml.css.parser {

    /**
     * IState controller for CSS Processing
     * @author redlab_b
     *
     */
    public class CssStateController {

        private IState current;
        private IState previous;
        private IState commentEnd;
        private IState commentStart;
        private IState commentInside;
        private StringBuilder buffer;
        private IState properties;
        private IState unknown;
        private String currentSelector;
        private IState rule;
        private CssUtils utils;
        private ICssFile css;

        /**
         * @param file the CssFile
         *
         */
        public CssStateController(ICssFile file) {
            this.css = file;
            utils = CssUtils.GetInstance();
            buffer = new StringBuilder();
            commentStart = new CommentStart(this);
            commentEnd = new CommentEnd(this);
            commentInside = new CommentInside(this);
            unknown = new Unknown(this);
            properties = new Properties(this);
            rule = new Rule(this);
            current = unknown;
        }

        /**
         * Appends a character to the buffer.
         * @param c the char to append to the buffer.
         */
        public void Append(char c) {
            this.buffer.Append(c);

        }

        /**
         * Sets the state to the previous state
         */
        public void Previous() {
            this.current = previous;
        }

        /**
         * Processes the character, delegates to states.
         *
         * @param c a character that needs to be processed
         */
        public void Process(char c) {
            current.Process(c);
        }

        private void ProcessProps(String props) {
            String[] split = props.Split(';');
            IDictionary<String, String> map = new Dictionary<String, String>();
            foreach (String prop in split) {
                String[] propSplit = prop.Split(':');
                if (propSplit.Length == 2) {
                    map[utils.StripDoubleSpacesTrimAndToLowerCase(propSplit[0])] = utils.StripDoubleSpacesAndTrim(propSplit[1]);
                }
            }
            if (currentSelector.Contains(",")) {
                String[] selectors = currentSelector.Split(',');
                foreach (String selector in selectors) {
                    css.Add(utils.StripDoubleSpacesAndTrim(selector), new Dictionary<string, string>(map));
                }
            } else {
                css.Add(utils.StripDoubleSpacesAndTrim(currentSelector), map);
            }
        }

        private void SetState(IState state) {
            this.current = state;
        }

        private void SetPrevious() {
            this.previous = current;
        }

        /**
         *
         */
        public void StateCommentEnd() {
            SetState(commentEnd);
        }

        /**
         *
         */
        public void StateCommentInside() {
            SetState(commentInside);
        }

        /**
         *
         */
        public void StateCommentStart() {
            SetPrevious();
            SetState(commentStart);
        }

        /**
         *
         */
        public void StateProperties() {
            previous = current;
            SetState(properties);
        }

        /**
         *
         */
        public void StateUnknown() {
            SetState(unknown);
        }

        /**
         *
         */
        public void StateRule() {
            SetState(rule);
        }

        /**
         *
         */
        public void StoreSelector() {
            this.currentSelector = buffer.ToString();
            buffer.Length = 0;
        }

        /**
         *
         */
        public void StoreProperties() {
            ProcessProps(buffer.ToString());
            buffer.Length = 0;
        }
    }
}