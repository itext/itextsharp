using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace iTextSharp.tool.xml.css.parser {
    public class CssSelectorParser {
        private const String selectorPatternString =
            "(\\*)|([_a-zA-Z][\\w-]*)|(\\.[_a-zA-Z][\\w-]*)|(#[_a-z][\\w-]*)|(\\[[_a-zA-Z][\\w-]*(([~^$*|])?=((\"[\\w-]+\")|([\\w-]+)))?\\])|(:[\\w()-]*)|( )|(\\+)|(>)|(~)";

        private const String selectorMatcherString = "^(" + selectorPatternString + ")*$";
        private static readonly Regex selectorPattern = new Regex(selectorPatternString);
        private static readonly Regex selectorMatcher = new Regex(selectorMatcherString);

        private static readonly int a = 1 << 16;
        private static readonly int b = 1 << 8;
        private static readonly int c = 1;

        public static IList<ICssSelectorItem> CreateCssSelector(String selector) {
            if (!selectorMatcher.IsMatch(selector))
                return null;
            IList<ICssSelectorItem> cssSelectorItems = new List<ICssSelectorItem>();
            Match itemMatcher = selectorPattern.Match(selector);
            bool isTagSelector = false;
            while (itemMatcher.Success) {
                String selectorItem = itemMatcher.Groups[0].Value;
                switch (selectorItem[0]) {
                    case '#':
                        cssSelectorItems.Add(new CssIdSelector(selectorItem.Substring(1)));
                        break;
                    case '.':
                        cssSelectorItems.Add(new CssClassSelector(selectorItem.Substring(1)));
                        break;
                    case '[':
                        cssSelectorItems.Add(new CssAttributeSelector(selectorItem));
                        break;
                    case ':':
                        cssSelectorItems.Add(new CssPseudoSelector(selectorItem));
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
                itemMatcher = itemMatcher.NextMatch();
            }

            return cssSelectorItems;
        }

        internal class CssTagSelector : ICssSelectorItem {
            private String t;
            private bool isUniversal;

            internal CssTagSelector(String t) {
                this.t = t;
                isUniversal = this.t.Equals("*") ? true : false;
            }

            public virtual bool Matches(Tag t) {
                return isUniversal || this.t.Equals(t.Name);
            }

            public virtual char Separator {
                get { return (char) 0; }
            }

            public virtual int Specificity {
                get {
                    if (isUniversal) return 0;
                    return CssSelectorParser.c;
                }
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

            public virtual int Specificity {
                get { return CssSelectorParser.b; }
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

            public virtual int Specificity {
                get { return CssSelectorParser.a; }
            }

            public override String ToString() {
                return "#" + id;
            }
        }

        internal class CssAttributeSelector : ICssSelectorItem {
            private String property;
            private char matchSymbol = (char) 0;
            private String value = null;

            internal CssAttributeSelector(String attrSelector) {
                int indexOfEqual = attrSelector.IndexOf('=');
                if (indexOfEqual == -1) {
                    property = attrSelector.Substring(1, attrSelector.Length - 1 - 1);
                } else {
                    if (attrSelector[indexOfEqual + 1] == '"')
                        value = attrSelector.Substring(indexOfEqual + 2, attrSelector.Length - 2 - (indexOfEqual + 2));
                    else
                        value = attrSelector.Substring(indexOfEqual + 1, attrSelector.Length - 1 - (indexOfEqual + 1));
                    matchSymbol = attrSelector[indexOfEqual - 1];
                    if ("~^$*|".IndexOf(matchSymbol) == -1) {
                        matchSymbol = (char) 0;
                        property = attrSelector.Substring(1, indexOfEqual - 1);
                    } else {
                        property = attrSelector.Substring(1, indexOfEqual - 1 - 1);
                    }
                }
            }

            public virtual char Separator {
                get { return (char)0; }
            }

            public virtual bool Matches(Tag t) {
                if (t == null)
                    return false;
                String attrValue = null;
                t.Attributes.TryGetValue(property, out attrValue);
                if (attrValue == null) return false;
                if (value == null) return true;

                switch (matchSymbol) {
                    case '|':
                        String pattern = String.Format("^{0}-?", value);
                        if (new Regex(pattern).Match(attrValue).Success)
                            return true;
                        break;
                    case '^':
                        if (attrValue.StartsWith(value))
                            return true;
                        break;
                    case '$':
                        if (attrValue.EndsWith(value))
                            return true;
                        break;
                    case '~':
                        pattern = String.Format("(^{0}\\s+)|(\\s+{1}\\s+)|(\\s+{2}$)", value, value, value);
                        if (new Regex(pattern).Match(attrValue).Success)
                            return true;
                        break;
                    case (char) 0:
                        if (attrValue.Equals(value))
                            return true;
                        break;
                    case '*':
                        if (attrValue.Contains(value))
                            return true;
                        break;
                }
                return false;
            }

            public virtual int Specificity {
                get { return CssSelectorParser.b; }
            }

            public override String ToString() {
                StringBuilder buf = new StringBuilder();
                buf.Append('[').Append(property);
                if (matchSymbol != 0)
                    buf.Append(matchSymbol);
                if (value != null)
                    buf.Append('=').Append('"').Append(value).Append('"');
                buf.Append(']');
                return buf.ToString();
            }
        }

        internal class CssPseudoSelector : ICssSelectorItem {
            private String selector;

            internal CssPseudoSelector(String selector) {
                this.selector = selector;
            }

            public virtual bool Matches(Tag t) {
                return false;
            }

            public virtual char Separator {
                get { return (char)0; }
            }

            public virtual int Specificity {
                get { return 0; }
            }

            public override string ToString() {
                return selector;
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

            public virtual int Specificity {
                get { return 0; }
            }

            public override String ToString() {
                return separator.ToString();
            }
        }
    }
}
