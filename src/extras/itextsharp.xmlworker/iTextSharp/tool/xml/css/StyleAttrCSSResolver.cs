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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.util;
using System.util.collections;
using iTextSharp.text.pdf.events;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.net;
using iTextSharp.tool.xml.pipeline.css;
namespace iTextSharp.tool.xml.css {

    /**
     * Resolves CSS properties.
     *
     * @author redlab_b
     *
     */
    public class StyleAttrCSSResolver : ICSSResolver {

        /**
         *
         */
        public const String STYLE = HTML.Attribute.STYLE;
        private CssUtils utils;
        private ICssInheritanceRules inherit;
        private ICssFiles cssFiles;
        private IFileRetrieve retrieve;

        /**
         * Construct a new {@link StyleAttrCSSResolver} with default settings.
         */
        public StyleAttrCSSResolver() : this(new CssFilesImpl(), CssUtils.GetInstance()) {
        }

        /**
         * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and the {@link DefaultCssInheritanceRules}.
         *
         * @param cssFiles a {@link CssFiles} implementation.
         */
        public StyleAttrCSSResolver(ICssFiles cssFiles) : this(cssFiles, CssUtils.GetInstance()) {
        }

        /**
         * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and {@link CssUtils} and the
         * {@link DefaultCssInheritanceRules}.
         *
         * @param cssFiles a {@link CssFiles} implementation.
         * @param utils the CssUtils to use.
         */
        public StyleAttrCSSResolver(ICssFiles cssFiles, CssUtils utils) : this(new DefaultCssInheritanceRules(), cssFiles, utils) {
        }

        /**
         * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and {@link CssUtils}.
         *
         * @param rules the {@link CssInheritanceRules} to use.
         * @param cssFiles a {@link CssFiles} implementation.
         * @param utils the CssUtils to use.
         */
        public StyleAttrCSSResolver(ICssInheritanceRules rules, ICssFiles cssFiles, CssUtils utils) : this(rules, cssFiles, utils, new FileRetrieveImpl()) {
        }
        /**
         * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and {@link CssUtils}.
         *
         * @param rules the {@link CssInheritanceRules} to use.
         * @param cssFiles a {@link CssFiles} implementation.
         * @param utils the CssUtils to use.
         * @param fileRetrieve the {@link FileRetrieve} implementation
         */
        public StyleAttrCSSResolver(ICssInheritanceRules rules, ICssFiles cssFiles, CssUtils utils, IFileRetrieve fileRetrieve) {
            this.utils = utils;
            this.cssFiles = cssFiles;
            this.inherit = rules;
            this.retrieve = fileRetrieve;
        }

        /**
         * @param cssFiles the {@link CssFile} implementation
         * @param r the {@link FileRetrieve} implementation
         */
        public StyleAttrCSSResolver(ICssFiles cssFiles, IFileRetrieve r) : this(new DefaultCssInheritanceRules(), cssFiles, CssUtils.GetInstance(), r) {
        }

        private static readonly char[] splitColon = {':'};

        /**
         * Also taking into account the CSS properties of any parent tag in the given tag.
         *
         * @see com.itextpdf.tool.xml.pipeline.css.CSSResolver#resolveStyles(com.itextpdf.tool.xml.Tag)
         */

        virtual public void ResolveStyles(Tag t)
        {
            // get css for this tag from resolver
            IDictionary<String, String> tagCss = new Dictionary<String, String>();
            IDictionary<String, String> listCss = null;
            if (null != cssFiles && cssFiles.HasFiles())
            {
                tagCss = cssFiles.GetCSS(t);
                if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.P) || Util.EqualsIgnoreCase(t.Name, HTML.Tag.TD))
                {
                    listCss = cssFiles.GetCSS(new Tag(HTML.Tag.UL));
                }
            }
            // get css from style attr
            if (null != t.Attributes && t.Attributes.Count != 0)
            {
                if (t.Attributes.ContainsKey(HTML.Attribute.CELLPADDING))
                {
                    CssUtils.MapPutAll(tagCss,
                                       utils.ParseBoxValues(t.Attributes[HTML.Attribute.CELLPADDING], "cellpadding-", ""));
                }
                if (t.Attributes.ContainsKey(HTML.Attribute.CELLSPACING))
                {
                    CssUtils.MapPutAll(tagCss,
                                       utils.ParseBoxValues(t.Attributes[HTML.Attribute.CELLSPACING], "cellspacing-", ""));
                }
                String styleAtt;
                t.Attributes.TryGetValue(HTML.Attribute.STYLE, out styleAtt);
                if (!string.IsNullOrEmpty(styleAtt))
                {
                    Dictionary<String, String> tagAttrCss = new Dictionary<string, string>();
                    String[] styles = styleAtt.Split(';');
                    foreach (String s in styles)
                    {
                        String[] part = s.Split(splitColon, 2);
                        if (part.Length == 2)
                        {
                            String key = utils.StripDoubleSpacesTrimAndToLowerCase(part[0]);
                            String value = utils.StripDoubleSpacesAndTrim(part[1]);
                            SplitRules(tagAttrCss, key, value);
                        }
                    }
                    foreach (KeyValuePair<String, String> e in tagAttrCss)
                    {
                        tagCss[e.Key] = e.Value;
                    }
                }
            }
            // inherit css from parent tags, as defined in provided CssInheritanceRules or if property = inherit
            IDictionary<String, String> css = t.CSS;
            if (t.Name != null)
            {
                if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.I) || Util.EqualsIgnoreCase(t.Name, HTML.Tag.CITE)
                    || Util.EqualsIgnoreCase(t.Name, HTML.Tag.EM) || Util.EqualsIgnoreCase(t.Name, HTML.Tag.VAR)
                    || Util.EqualsIgnoreCase(t.Name, HTML.Tag.DFN) || Util.EqualsIgnoreCase(t.Name, HTML.Tag.ADDRESS))
                {
                        tagCss[CSS.Property.FONT_STYLE] = CSS.Value.ITALIC;
                }
                else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.B) || Util.EqualsIgnoreCase(t.Name, HTML.Tag.STRONG))
                {
                    tagCss[CSS.Property.FONT_WEIGHT] = CSS.Value.BOLD;
                }
                else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.U) || Util.EqualsIgnoreCase(t.Name, HTML.Tag.INS))
                {
                    tagCss[CSS.Property.TEXT_DECORATION] = CSS.Value.UNDERLINE;
                }
                else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.S) || Util.EqualsIgnoreCase(t.Name, HTML.Tag.STRIKE)
                         || Util.EqualsIgnoreCase(t.Name, HTML.Tag.DEL))
                {
                             tagCss[CSS.Property.TEXT_DECORATION] = CSS.Value.LINE_THROUGH;
                }
                else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.BIG))
                {
                    tagCss[CSS.Property.FONT_SIZE] = CSS.Value.LARGER;
                }
                else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.SMALL))
                {
                    tagCss[CSS.Property.FONT_SIZE] = CSS.Value.SMALLER;
                }
            }


            if (listCss != null && listCss.ContainsKey(CSS.Property.LIST_STYLE_TYPE)) {
                    css[CSS.Property.LIST_STYLE_TYPE] = listCss[CSS.Property.LIST_STYLE_TYPE];
            }

		    if (MustInherit(t.Name) && null != t.Parent && null != t.Parent.CSS) {
			    if (null != this.inherit) {
				    foreach (KeyValuePair<String, String> entry in t.Parent.CSS) {
					    String key = entry.Key;
					    if ((tagCss.ContainsKey(key) && CSS.Value.INHERIT.Equals(tagCss[key]) ) || CanInherite(t, key)) {
                            if (key.Contains(CSS.Property.CELLPADDING)
                                    && (HTML.Tag.TD.Equals(t.Name) || HTML.Tag.TH.Equals(t.Name))) {
                                String paddingKey = key.Replace(CSS.Property.CELLPADDING, CSS.Property.PADDING);
                                //if (!tagCss.containsKey(paddingKey)) {
                                tagCss[paddingKey] =  entry.Value;
                                //continue;
                                //}
                            }
                            else {
						        //splitRules(css, key, entry.getValue());
                                css[key] = entry.Value;
                            }
					    }
				    }
			    }
                else {
                    foreach(KeyValuePair<string, string> entry in t.Parent.CSS)
                        css.Add(entry);
			    }
		    }

            if(t.Name != null) {
                if (t.Name.Equals(HTML.Tag.FONT)) {
                    String font_family;
                    if (t.Attributes.TryGetValue(HTML.Attribute.FACE, out font_family))
                        css[CSS.Property.FONT_FAMILY] = font_family;
                    String color;
                    if (t.Attributes.TryGetValue(HTML.Attribute.COLOR, out color))
                        css[CSS.Property.COLOR] = color;
                    String size;
                    if (t.Attributes.TryGetValue(HTML.Attribute.SIZE, out size)) {
                        if (size.Equals("1")) css[CSS.Property.FONT_SIZE] = CSS.Value.XX_SMALL;
                        else if (size.Equals("2")) css[CSS.Property.FONT_SIZE] = CSS.Value.X_SMALL;
                        else if (size.Equals("3")) css[CSS.Property.FONT_SIZE] = CSS.Value.SMALL;
                        else if (size.Equals("4")) css[CSS.Property.FONT_SIZE] = CSS.Value.MEDIUM;
                        else if (size.Equals("5")) css[CSS.Property.FONT_SIZE] = CSS.Value.LARGE;
                        else if (size.Equals("6")) css[CSS.Property.FONT_SIZE] = CSS.Value.X_LARGE;
                        else if (size.Equals("7")) css[CSS.Property.FONT_SIZE] = CSS.Value.XX_LARGE;

                    }
                }
                else if (t.Name.Equals(HTML.Tag.A)) {
                    css[CSS.Property.TEXT_DECORATION] = CSS.Value.UNDERLINE;
                    css[CSS.Property.COLOR] = "blue";
                }
            }


            // overwrite properties (if value != inherit)
            foreach (KeyValuePair<String, String> e in tagCss) {
                if (!Util.EqualsIgnoreCase(CSS.Value.INHERIT, e.Value)) {
                    if (e.Key.Equals(CSS.Property.TEXT_DECORATION)) {
                        String oldValue = null;
                        css.TryGetValue(e.Key, out oldValue);
                        css[e.Key] = MergeTextDecorationRules(oldValue, e.Value);
                    } else {
                        css[e.Key] = e.Value;
                    }
                }
            }
        }

        private String MergeTextDecorationRules(String oldRule, String newRule) {
            if (CSS.Value.NONE.Equals(newRule))
                return newRule;

            HashSet2<String> attrSet = new HashSet2<String>();
            if (oldRule != null)
                foreach (String attr in new Regex(@"\s+").Split(oldRule))
                    attrSet.Add(attr);
            if (newRule != null)
                foreach (String attr in new Regex(@"\s+").Split(newRule))
                    attrSet.Add(attr);
            StringBuilder resultantStr = new StringBuilder();
            foreach (String attr in attrSet) {
                if (attr.Equals(CSS.Value.NONE) || attr.Equals(CSS.Value.INHERIT))
                    continue;
                if (resultantStr.Length > 0)
                    resultantStr.Append(' ');
                resultantStr.Append(attr);
            }
            return resultantStr.Length == 0 ? null : resultantStr.ToString();
        }

        /**
         * @param css the css map to populate
         * @param key the property
         * @param value the value
         */
        private void SplitRules(IDictionary<String, String> css, String key, String value) {
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
                CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "margin-", ""));
            } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_WIDTH, key)) {
                CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "border-", "-width"));
            } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_STYLE, key)) {
                CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "border-", "-style"));
            } else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_COLOR, key)) {
                CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "border-", "-color"));
            } else if (Util.EqualsIgnoreCase(CSS.Property.PADDING, key)) {
                CssUtils.MapPutAll(css, utils.ParseBoxValues(value, "padding-", ""));
            } else if (Util.EqualsIgnoreCase(CSS.Property.FONT, key)) {
                CssUtils.MapPutAll(css, utils.ProcessFont(value));
            } else if (Util.EqualsIgnoreCase(CSS.Property.LIST_STYLE, key)) {
                CssUtils.MapPutAll(css, utils.ProcessListStyle(value));
            } else if (Util.EqualsIgnoreCase(CSS.Property.BACKGROUND, key)) {
                IDictionary<String, String> backgroundStyles = utils.ProcessBackground(value);
                foreach (String backgroundKey in backgroundStyles.Keys) {
                    if (!css.ContainsKey(backgroundKey)) {
                        css[backgroundKey] = backgroundStyles[backgroundKey];
                    }
                }
            } else {
                css[key] = value;
            }
        }

        /**
         * By setting an implementation of {@link CssInheritanceRules} a developer can set rules on what css selectors are
         * inherited from parent tags.
         *
         * @param cssInheritanceRules the inherit to set
         */
        virtual public void SetCssInheritance(ICssInheritanceRules cssInheritanceRules) {
            this.inherit = cssInheritanceRules;
        }

        /**
         * Defaults to true if no {@link CssInheritanceRules} implementation set.
         *
         * @param t
         * @param property
         * @return true if may be inherited false otherwise
         */
        private bool CanInherite(Tag t, String property) {
            if (null != this.inherit) {
                return this.inherit.InheritCssSelector(t, property);
            }
            return true;
        }

        /**
         * Defaults to true if no {@link CssInheritanceRules} implementation set.
         *
         * @param tag
         * @return true if must be inherited false otherwise
         */
        private bool MustInherit(String tag) {
            if (null != this.inherit) {
                return this.inherit.InheritCssTag(tag);
            }
            return true;
        }

        /*
         * (non-Javadoc)
         * @see com.itextpdf.tool.xml.pipeline.css.CSSResolver#addCss(java.lang.String, java.lang.String)
         */
        virtual public void AddCss(String content, String charSet, bool isPersistent) {
            CssFileProcessor proc = new CssFileProcessor();
            try {
                retrieve.ProcessFromStream(new MemoryStream(Encoding.GetEncoding(charSet).GetBytes(content)), proc);
                ICssFile css = proc.GetCss();
                css.IsPersistent(isPersistent);
                this.cssFiles.Add(css);
            } catch (ArgumentException e) {
                throw new CssResolverException(e);
            }
            catch (IOException e) {
                throw new CssResolverException(e);
            }
        }

        /**
         * Add a file to the CssFiles Collection.
         *
         * @param href the path, if it starts with http we try to retrieve the file
         *            from the net, if not we try a normal file operation.
         */
        virtual public void AddCssFile(String href, bool isPersistent) {
            CssFileProcessor cssFileProcessor = new CssFileProcessor();
            try {
                retrieve.ProcessFromHref(href, cssFileProcessor);
            } catch (IOException e) {
                throw new CssResolverException(e);
            }
            ICssFile css = cssFileProcessor.GetCss();
            css.IsPersistent(isPersistent);
            this.cssFiles.Add(css);
        }

        /**
         * Add a file to the CssFiles Collection.
         * @param file the CssFile to add.
         */
        virtual public void AddCss(ICssFile file) {
            this.cssFiles.Add(file);
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.pipeline.css.CSSResolver#addCss(java.lang.String)
         */
        virtual public void AddCss(String content, bool isPersistent) {
            CssFileProcessor proc = new CssFileProcessor();
            IFileRetrieve retrieve = new FileRetrieveImpl();
            try {
                retrieve.ProcessFromStream(new MemoryStream(Encoding.GetEncoding(1252).GetBytes(content)), proc);
                ICssFile css = proc.GetCss();
                css.IsPersistent(isPersistent);
                this.cssFiles.Add(css);
            } catch (IOException e) {
                throw new CssResolverException(e);
            }

        }

        /**
         * @param inherit the inherit to set
         */
        virtual public void SetCssInheritanceRules(ICssInheritanceRules inherit) {
            this.inherit = inherit;
        }

        /**
         * The {@link FileRetrieve} implementation to use in {@link StyleAttrCSSResolver#addCssFile(String)}.
         * @param retrieve the retrieve to set
         */
        virtual public IFileRetrieve FileRetrieve {
            set {
                this.retrieve = value;
            }
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.pipeline.css.CSSResolver#clear()
         */
        virtual public ICSSResolver Clear() {
            cssFiles.Clear();
            return this;
        }
    }
}
