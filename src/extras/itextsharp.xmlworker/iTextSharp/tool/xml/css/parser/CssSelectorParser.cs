using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iTextSharp.tool.xml.css.parser {
    public class CssSelectorParser {
        private const String selectorPattern =
            "(\\*)|([_a-z][_a-z0-9-]*)|(\\.[_a-zA-Z][_a-zA-Z0-9-]*)|(#[_a-z][_a-zA-Z0-9-]*)|( )|(\\+)|(>)|(~)";

        private const String selectorMatcher =
            "^((\\*)|([_a-z][_a-z0-9-]*)|(\\.[_a-zA-Z][_a-zA-Z0-9-]*)|(#[_a-z][_a-zA-Z0-9-]*)|( )|(\\+)|(>)|(~))*$";

        public static IList<ICssSelectorItem> CreateCssSelector(String selector) {
            if (!Regex.IsMatch(selector, selectorMatcher))
                return null;
            IList<ICssSelectorItem> cssSelectorItems = new List<ICssSelectorItem>();
            Match matcher = new Regex(selectorPattern).Match(selector);
            bool isTagSelector = false;
            while (matcher.Success) {
                String selectorItem = matcher.Groups[0].Value;
                switch (selectorItem[0]) {
                    case '#':
                        cssSelectorItems.Add(new CssIdSelector(selectorItem.Substring(1)));
                        break;
                    case '.':
                        cssSelectorItems.Add(new CssClassSelector(selectorItem.Substring(1)));
                        break;
                    case ' ':
                    case '+':
                    case '>':
                    case '~':
                        ICssSelectorItem lastItem = cssSelectorItems[cssSelectorItems.Count - 1];
                        ICssSelectorItem currItem = new CssSeparatorSelector(selectorItem[0]);
                        if (lastItem is CssSeparatorSelector) {
                            if (selectorItem[0] == ' ')
                                break;
                            else if (lastItem.Separator == ' ')
                                cssSelectorItems[cssSelectorItems.Count - 1] = currItem;
                            else
                                return null;
                        } else {
                            cssSelectorItems.Add(currItem);
                            isTagSelector = false;
                        }
                        break;
                    default: //and case '*':
                        if (isTagSelector)
                            return null;
                        isTagSelector = true;
                        cssSelectorItems.Add(new CssTagSelector(selectorItem));
                        break;
                }
                matcher = matcher.NextMatch();
            }

            return cssSelectorItems;
        }

        internal class CssTagSelector : ICssSelectorItem {
            private String t;

            internal CssTagSelector(String t) {
                this.t = t;
            }

            public virtual bool Matches(Tag t) {
                return this.t.Equals("*") || this.t.Equals(t.Name);
            }

            public virtual char Separator {
                get { return (char) 0; }
            }

            public override String ToString() {
                return t;
            }
        }

        internal class CssClassSelector : ICssSelectorItem {
            private String className;

            internal CssClassSelector(String className) {
                this.className = className;
            }

            public virtual bool Matches(Tag t) {
                String classAttr = null;
                t.Attributes.TryGetValue("class", out classAttr);
                if (classAttr == null || classAttr.Length == 0)
                    return false;
                String[] classNames = classAttr.Split(' ');
                foreach (String currClassName in classNames)
                    if (this.className.Equals(currClassName.Trim()))
                        return true;
                return false;
            }

            public virtual char Separator {
                get { return (char) 0; }
            }

            public override String ToString() {
                return "." + className;
            }
        }

        internal class CssIdSelector : ICssSelectorItem {
            private String id;

            internal CssIdSelector(String id) {
                this.id = id;
            }

            public virtual bool Matches(Tag t) {
                String id = null;
                t.Attributes.TryGetValue("id", out id);
                return id != null && this.id.Equals(id.Trim());
            }

            public virtual char Separator {
                get { return (char)0; }
            }

            public override String ToString() {
                return "#" + id;
            }
        }

        internal class CssSeparatorSelector : ICssSelectorItem {
            private char separator;

            internal CssSeparatorSelector(char separator) {
                this.separator = separator;
            }

            public virtual char Separator {
                get { return separator; }
            }

            public virtual bool Matches(Tag t) {
                return false;
            }

            public override String ToString() {
                return separator.ToString();
            }
        }
    }
}
