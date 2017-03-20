/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using System.util;
using iTextSharp.tool.xml.util;

namespace iTextSharp.tool.xml.css {
/**
 * @author itextpdf.com
 *
 */

    public class CssFilesImpl : ICssFiles {

        private IList<ICssFile> files;
        private CssUtils utils;

        /**
         * Constructs a new CssFilesImpl.
         */

        public CssFilesImpl() {
            this.files = new List<ICssFile>();
            this.utils = CssUtils.GetInstance();
        }

        /**
         * Construct a new CssFilesImpl with the given css file.
         * @param css the css file
         */

        public CssFilesImpl(ICssFile css): this() {
            this.Add(css);
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.css.CssFiles#hasFiles()
         */

        virtual public bool HasFiles() {
            return this.files.Count > 0;
        }

        /**
         * Processes a tag and retrieves CSS. Selectors created:
         * <ul>
         * <li>element</li>
         * <li>element&gt;element (and a spaced version element &gt; element)</li>
         * <li>#id</li>
         * <li>.class</li>
         * <li>element+element ( and a spaced version element + element)</li>
         * </ul>
         */
        virtual public IDictionary<String, String> GetCSS(Tag t) {
            IDictionary<String, String> aggregatedProps = new Dictionary<String, String>();
            PopulateCss(t, aggregatedProps);
            return aggregatedProps;
        }

        public virtual void PopulateCss(Tag t, IDictionary<String, String> aggregatedProps) {
            List<CssRule> rules = new List<CssRule>();
            foreach (ICssFile cssFile in this.files) {
                rules.AddRange(cssFile.Get(t));
            }
            // C#-specific code: use stable sort
            SortUtil.MergeSort(rules);
            foreach (CssRule rule in rules)
                PopulateOneCss(aggregatedProps, rule.NormalDeclarations);
            foreach (CssRule rule in rules)
                PopulateOneCss(aggregatedProps, rule.ImportantDeclarations);
        }

        virtual public void PopulateOneCss(IDictionary<String, String> aggregatedProps, IDictionary<String, String> cssDeclaration) {
            IDictionary<String, String> css = new Dictionary<String, String>();
            foreach (KeyValuePair<String, String> e in cssDeclaration) {
                String key = utils.StripDoubleSpacesTrimAndToLowerCase(e.Key);
                String value = utils.StripDoubleSpacesAndTrim(e.Value);
                if (Util.EqualsIgnoreCase(CSS.Property.BORDER, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBorder(value));
                } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_TOP, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBorder(value, CSS.Property.BORDER_TOP));
                } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_BOTTOM, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBorder(value, CSS.Property.BORDER_BOTTOM));
                } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_LEFT, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBorder(value, CSS.Property.BORDER_LEFT));
                } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_RIGHT, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBorder(value, CSS.Property.BORDER_RIGHT));
                } else if (Util.EqualsIgnoreCase(CSS.Property.MARGIN, key)) {
                    IDictionary<String, String> margins = utils.ParseBoxValues(value, "margin-", "");
                    foreach (String marginKey in margins.Keys) {
                        if (!css.ContainsKey(marginKey)) {
                            css.Add(marginKey, margins[marginKey]);
                        }
                    }
                } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_WIDTH, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "border-", "-width"));
                } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_STYLE, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "border-", "-style"));
                } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_COLOR, key)) {
                    CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "border-", "-color"));
                } else if (Util.EqualsIgnoreCase(CSS.Property.PADDING, key)) {
                    IDictionary<String, String> paddings = utils.ParseBoxValues(value, "padding-", "");
                    foreach (String paddingKey in paddings.Keys) {
                        if (!css.ContainsKey(paddingKey)) {
                            css.Add(paddingKey, paddings[paddingKey]);
                        }
                    }
                    //CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "padding-", ""));
                } else if (Util.EqualsIgnoreCase(CSS.Property.FONT, key)) {
                    CssUtils.MapPutAll(css, utils.ProcessFont(value));
                } else if (Util.EqualsIgnoreCase(CSS.Property.LIST_STYLE, key)) {
                    CssUtils.MapPutAll(css, utils.ProcessListStyle(value));
                } else if (key.ToLowerInvariant().Contains(CSS.Property.BACKGROUND)) {
                    IDictionary<String, String> backgroundStyles = utils.ProcessBackground(value);
                    foreach (String backgroundKey in backgroundStyles.Keys) {
                        if (!css.ContainsKey(backgroundKey)) {
                            css.Add(backgroundKey, backgroundStyles[backgroundKey]);
                        }
                    }
                } else {
                    css[key] = value;
                }
            }
            CssUtils.MapPutAll(aggregatedProps, css);
        }

        virtual public void Add(ICssFile css) {
            if (css != null) {
                this.files.Add(css);
            }
        }

        virtual public void Clear() {
            for (int k = 0; k < files.Count; ++k) {
                if (!files[k].IsPersistent()) {
                    files.RemoveAt(k);
                    --k;
                }
            }
        }
    }

}
