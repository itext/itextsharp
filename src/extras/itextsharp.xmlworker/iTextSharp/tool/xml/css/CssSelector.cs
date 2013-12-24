using System;
using System.Text;
using System.Collections.Generic;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
/*
 * $Id: CssSelector.java 141 2011-05-31 12:58:38Z redlab_b $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Balder Van Camp, Emiel Ackermann, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */
namespace iTextSharp.tool.xml.css {

    /**
     * @author redlab_b
     *
     */
    public class CssSelector {

        private CssUtils utils;

        /**
         *
         */
        public CssSelector() {
            this.utils = CssUtils.GetInstance();
        }
        /**
         * Calls each method in this class and returns an aggregated list of selectors.
         * @param t the tag
         * @return set of selectors
         */
        virtual public IDictionary<String,object> CreateAllSelectors(Tag t) {
            IDictionary<String,object> set = new Dictionary<String,object>();
            CssUtils.MapPutAll(set, CreateTagSelectors(t));
            CssUtils.MapPutAll(set, CreateClassSelectors(t));
            CssUtils.MapPutAll(set, CreateIdSelector(t));
            return set;
        }

        virtual public void CreateSelectors(Tag t, IDictionary<String, object> selectors) {
            IDictionary<String, object> tagNameSet = new Dictionary<String, object>();
            foreach (String selector in selectors.Keys) {
                tagNameSet[t.Name + (selector.StartsWith(" ") ? "" : " ") + selector] = null;
            }
            IDictionary<String, object> tagClassSet = new Dictionary<String, object>();
            String tagClasses;
            String[] classSplit = null;
            if (t.Attributes.TryGetValue(HTML.Attribute.CLASS, out tagClasses)) {
                classSplit = utils.StripDoubleSpacesAndTrim(tagClasses).Split(' ');
                foreach (String selector in selectors.Keys) {
                    foreach (String tagClass in classSplit) {
                        StringBuilder builder = new StringBuilder();
                        builder.Append('.').Append(tagClass);
                        if (selector.StartsWith("#") || selector.StartsWith(".") || selector.StartsWith(" ")) {
                            tagClassSet[builder + selector] = null;
                            if (!selector.StartsWith(" ")) {
                                tagClassSet[builder + " " + selector] = null;
                            }
                            foreach (String tagSelector in tagNameSet.Keys) {
                                builder = new StringBuilder(tagSelector);
                                builder.Append('.').Append(tagClass);
                                tagClassSet[builder.ToString()] = null;
                            }
                        } else {
                            builder.Append(" " + selector);
                            tagClassSet[builder.ToString()] = null;
                        }
                    }
                }
            }

            String id = null;
            IDictionary<String, Object> tagIdSet = new Dictionary<string, object>();
            if (t.Attributes.TryGetValue(HTML.Attribute.ID, out id)) {
                foreach (String selector in selectors.Keys) {
                    StringBuilder builder = new StringBuilder();
                    builder.Append('#').Append(id);
                    if (selector.StartsWith("#") || selector.StartsWith(".") || selector.StartsWith(" ")) {
                        tagClassSet[builder + selector] = null;
                        if (!selector.StartsWith(" ")) {
                            tagClassSet[builder.ToString() + " " + selector] = null;
                        }
                    } else {
                        builder.Append(" " + selector);
                        tagClassSet[builder.ToString()] = null;
                    }
                }
            }

            if (selectors.Count == 0) {
                tagNameSet[t.Name] = null;
                if (tagClasses != null) {
                    foreach (String tagClass in classSplit) {
                        StringBuilder builder = new StringBuilder();
                        builder.Append('.').Append(tagClass);
                        tagClassSet[builder.ToString()] = null;

                        builder = new StringBuilder();
                        builder.Append(" .").Append(tagClass);
                        tagClassSet[builder.ToString()] = null;

                        foreach (String tagSelector in tagNameSet.Keys) {
                            builder = new StringBuilder(tagSelector);
                            builder.Append('.').Append(tagClass);
                            tagClassSet[builder.ToString()] = null;
                        }
                    }
                }

                if (id != null) {
                    StringBuilder builder = new StringBuilder();
                    builder.Append('#').Append(id);
                    tagIdSet[builder.ToString()] = null;

                    builder = new StringBuilder();
                    builder.Append(" #").Append(id);
                    tagIdSet[builder.ToString()] = null;
                }
            }

            CssUtils.MapPutAll(selectors, tagNameSet);
            CssUtils.MapPutAll(selectors, tagClassSet);
            CssUtils.MapPutAll(selectors, tagIdSet);

            if (t.Parent != null && selectors.Count < 30) {
                CreateSelectors(t.Parent, selectors);
            }
        }

        /**
         * Creates selectors for a given tag.
         * @param t the tag to create selectors for.
         * @return all selectors for the given tag.
         */
        virtual public IDictionary<String,object> CreateTagSelectors(Tag t) {
            IDictionary<String,object> selectors = new Dictionary<String,object>();
            selectors[t.Name] = null;;
            if (null != t.Parent) {
                Tag parent = t.Parent;
                StringBuilder b = new StringBuilder(t.Name);
                StringBuilder bStripped = new StringBuilder(t.Name);
                StringBuilder bElem = new StringBuilder(t.Name);
                StringBuilder bChild = new StringBuilder(t.Name);
                StringBuilder bChildSpaced = new StringBuilder(t.Name);
                Tag child = t;
                while (null != parent) {
                    if (parent.Children.IndexOf(child) == 0) {
                        bChild.Insert(0, '+').Insert(0, parent.Name);
                        bChildSpaced.Insert(0, " + ").Insert(0, parent.Name);
                        selectors[bChild.ToString()] = null;
                        selectors[bChildSpaced.ToString()] = null;
                    }
                    b.Insert(0, " > ").Insert(0, parent.Name);
                    selectors[b.ToString()] = null;;
                    bStripped.Insert(0, ">").Insert(0, parent.Name);
                    selectors[bStripped.ToString()] = null;;
                    bElem.Insert(0, ' ').Insert(0, parent.Name);
                    selectors[bElem.ToString()] = null;
                    child = parent;
                    parent = parent.Parent;
                }
            }
            return selectors;
        }

        /**
         * Creates the class selectors, each class is prepended with a ".".
         * @param t the tag
         * @return set of Strings
         */
        virtual public IDictionary<String,object> CreateClassSelectors(Tag t) {
            String classes;
            t.Attributes.TryGetValue(HTML.Attribute.CLASS, out classes);
            IDictionary<String,object> set = new Dictionary<String,object>();
            if (null != classes) {
                String[] classSplit = this.utils.StripDoubleSpacesAndTrim(classes).Split(CssUtils.whitespace);
                foreach (String klass in classSplit) {
                    StringBuilder builder = new StringBuilder();
                    builder.Append('.').Append(klass);
                    set[builder.ToString()] = null;
                    builder = new StringBuilder();
                    builder.Append(t.Name).Append('.').Append(klass);
                    set[builder.ToString()] = null;
                }
            }
            return set;
        }

        /**
         * Creates the selector for id, the id is prepended with '#'.
         * @param t the tag
         * @return set of Strings
         */
        virtual public IDictionary<String,object> CreateIdSelector(Tag t) {
            String id;
            t.Attributes.TryGetValue(HTML.Attribute.ID, out id);
            IDictionary<String,object> set = new Dictionary<String,object>();
            if (null != id) {
                StringBuilder builder = new StringBuilder();
                builder.Append('#').Append(id);
                set[builder.ToString()] = null;
                builder = new StringBuilder();
                builder.Append(t.Name).Append('#').Append(id);
                set[builder.ToString()] = null;
            }
            return set;
        }

    }
}
