/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace iTextSharp.tool.xml.css {
    public class CssSelector {
        private IList<ICssSelectorItem> selectorItems;

        public CssSelector(IList<ICssSelectorItem> selector) {
            this.selectorItems = selector;
        }

        public virtual bool Matches(Tag t) {
            return Matches(t, selectorItems.Count - 1);
        }

        private bool Matches(Tag t, int index) {
            if (t == null)
                return false;
            Stack<ICssSelectorItem> currentSelector = new Stack<ICssSelectorItem>();
            for (; index >= 0; index--) {
                if (selectorItems[index].Separator != (char) 0)
                    break;
                else
                    currentSelector.Push(selectorItems[index]);
            }
            while (currentSelector.Count != 0)
                if (!currentSelector.Pop().Matches(t))
                    return false;
            if (index == -1)
                return true;
            else {
                char separator = selectorItems[index].Separator;
                if (separator == 0)
                    return false;
                int precededIndex;
                index--;
                switch (separator) {
                    case '>':
                        return Matches(t.Parent, index);
                    case ' ':
                        while (t != null) {
                            if (Matches(t.Parent, index))
                                return true;
                            t = t.Parent;
                        }
                        return false;
                    case '~':
                        if (!t.HasParent())
                            return false;
                        precededIndex = t.Parent.Children.IndexOf(t) - 1;
                        while (precededIndex >= 0) {
                            if (Matches(t.Parent.Children[precededIndex], index))
                                return true;
                            precededIndex--;
                        }
                        return false;
                    case '+':
                        if (!t.HasParent())
                            return false;
                        precededIndex = t.Parent.Children.IndexOf(t) - 1;
                        return precededIndex >= 0 && Matches(t.Parent.Children[precededIndex], index);
                    default:
                        return false;
                }
            }
        }

        public virtual int CalculateSpecifity() {
            int specifity = 0;
            foreach (ICssSelectorItem item in this.selectorItems)
                specifity += item.Specificity;
            return specifity;
        }

        public override String ToString() {
            StringBuilder buf = new StringBuilder();
            foreach (ICssSelectorItem item in selectorItems)
                buf.Append(item.ToString());
            return buf.ToString();
        }

    }
}
