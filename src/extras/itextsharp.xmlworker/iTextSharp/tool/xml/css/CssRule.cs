using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iTextSharp.tool.xml.css {
    public class CssRule : IComparable<CssRule> {
        private CssSelector selector;
        private IDictionary<String, String> normalDeclarations;
        private IDictionary<String, String> importantDeclarations;
        private static readonly Regex importantMatcher = new Regex("^.*!\\s*important$");

        public CssRule(IList<ICssSelectorItem> selector, IDictionary<String, String> declarations) {
            this.selector = new CssSelector(selector);
            this.normalDeclarations = declarations;
            this.importantDeclarations = new Dictionary<string, string>();

            foreach (KeyValuePair<String, String> declaration in normalDeclarations) {
                int exclIndex = declaration.Value.IndexOf('!');
                if (exclIndex > 0 && importantMatcher.IsMatch(declaration.Value)) {
                    importantDeclarations[declaration.Key] = declaration.Value.Substring(0, exclIndex).Trim();
                }
            }
            //remove important declarations from normal declarations
            foreach (String key in importantDeclarations.Keys)
                normalDeclarations.Remove(key);
        }

        public virtual CssSelector Selector {
            get { return selector; }
        }

        public virtual IDictionary<String, String> NormalDeclarations {
            get { return normalDeclarations; }
        }

        public virtual IDictionary<String, String> ImportantDeclarations {
            get { return importantDeclarations; }
        }

        public override String ToString() {
            return String.Format("{0} {{ count: {1} }} #spec:{2}", selector.ToString(), normalDeclarations.Count + importantDeclarations.Count, selector.CalculateSpecifity());
        }

        public int CompareTo(CssRule o) {
            return this.selector.CalculateSpecifity() - o.selector.CalculateSpecifity();
        }
    }
}
