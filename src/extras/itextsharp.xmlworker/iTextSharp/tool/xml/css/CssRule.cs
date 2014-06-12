using System;
using System.Collections.Generic;

namespace iTextSharp.tool.xml.css {
    public class CssRule {
        private CssSelector selector;
        private IDictionary<String, String> declarations;

        public CssRule(IList<ICssSelectorItem> selector, IDictionary<String, String> declarations) {
            this.selector = new CssSelector(selector);
            this.declarations = declarations;
        }

        public virtual CssSelector Selector {
            get { return selector; }
        }

        public virtual IDictionary<String, String> Declarations {
            get { return declarations; }
        }

        public override String ToString() {
            return String.Format("{0} {{ count: {1} }}", selector.ToString(), declarations.Count);
        }
    }
}
