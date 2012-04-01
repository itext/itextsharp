/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: VVB, Bruno Lowagie, et al.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.util;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.net;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.svg.graphic;

namespace iTextSharp.tool.xml.svg.css {





    public class StyleAttrSvgCSSResolver : ICSSResolver {

	    /**
	     *
	     */
	    public static String STYLE = HTML.Attribute.STYLE;
	    private CssUtils utils;
	    private ICssInheritanceRules inherit;
	    private ICssFiles cssFiles;
	    private IFileRetrieve retrieve;
	    private IDictionary<String, int?> attributes;

	    /**
	     * Construct a new {@link StyleAttrCSSResolver} with default settings.
	     */
        public StyleAttrSvgCSSResolver()
            : this(new CssFilesImpl(), CssUtils.GetInstance()) {}

	    /**
	     * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and the {@link DefaultCssInheritanceRules}.
	     *
	     * @param cssFiles a {@link CssFiles} implementation.
	     */
        public StyleAttrSvgCSSResolver(ICssFiles cssFiles)
            : this(cssFiles, CssUtils.GetInstance()) {}

	    /**
	     * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and {@link CssUtils} and the
	     * {@link DefaultCssInheritanceRules}.
	     *
	     * @param cssFiles a {@link CssFiles} implementation.
	     * @param utils the CssUtils to use.
	     */
        public StyleAttrSvgCSSResolver(ICssFiles cssFiles, CssUtils utils)
            : this(new DefaultCssInheritanceRules(), cssFiles, utils) {}

	    /**
	     * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and {@link CssUtils}.
	     *
	     * @param rules the {@link CssInheritanceRules} to use.
	     * @param cssFiles a {@link CssFiles} implementation.
	     * @param utils the CssUtils to use.
	     */
        public StyleAttrSvgCSSResolver(ICssInheritanceRules rules, ICssFiles cssFiles, CssUtils utils)
            : this(rules, cssFiles, utils, new FileRetrieveImpl()) {}
	    /**
	     * Construct a new StyleAttrCSSResolver with the given {@link CssFiles} and {@link CssUtils}.
	     *
	     * @param rules the {@link CssInheritanceRules} to use.
	     * @param cssFiles a {@link CssFiles} implementation.
	     * @param utils the CssUtils to use.
	     * @param fileRetrieve the {@link FileRetrieve} implementation
	     */
	    public StyleAttrSvgCSSResolver(ICssInheritanceRules rules, ICssFiles cssFiles, CssUtils utils, IFileRetrieve fileRetrieve) {
		    this.utils = utils;
		    this.cssFiles = cssFiles;
		    this.inherit = rules;
		    this.retrieve = fileRetrieve;
		    this.attributes = SVGAttributes.GetSVGAttributesList();
	    }

	    /**
	     * @param cssFiles the {@link CssFile} implementation
	     * @param r the {@link FileRetrieve} implementation
	     */
        public StyleAttrSvgCSSResolver(ICssFiles cssFiles, IFileRetrieve r)
            : this(new DefaultCssInheritanceRules(), cssFiles, CssUtils.GetInstance(), r) {}

	    /**
	     * Also taking into account the CSS properties of any parent tag in the given tag.
	     *
	     * @see com.itextpdf.tool.xml.pipeline.css.CSSResolver#resolveStyles(com.itextpdf.tool.xml.Tag)
	     */
    	
	    //THE DIFFERENCE BETWEEN HTML AND SVG: SVG has for a lot of style properties the possibility to use still attributes that define the same 
	    public void ResolveStyles(Tag t) {
		    // get css for this tag from resolver
		    IDictionary<String, String> tagCss = new Dictionary<String, String>();
		    if (null != cssFiles && cssFiles.HasFiles()) {
			    tagCss = cssFiles.GetCSS(t);
		    }
    		
		    if (null != t.Attributes && t.Attributes.Count > 0) {
			    //first get the attributes that related to style but aren't in a style attribute, these can be overwritten by the same element that is defined in a style
			    //TODO check default values & incorrect values: 
			    //e.g. stroke="red" style="stroke:yellow" -> yellow but stroke="red" style="stroke:an non-existing color" -> red
			    //if both are wrong or missing -> take from parent 
    			
			    foreach (KeyValuePair<String, String> pair in t.Attributes) {
                    bool valid = SVGAttributes.IsValidAttribute(pair.Key, pair.Value, attributes);
				    if (valid){
                        tagCss[pair.Key] = pair.Value;
				    }
			    }

			    // get css from "style" attr			
			    String styleAtt;
			    if (t.Attributes.TryGetValue(HTML.Attribute.STYLE, out styleAtt) && !string.IsNullOrEmpty(styleAtt)) {
				    String[] styles = styleAtt.Split(new Char[]{';'});
				    foreach (String s in styles) {
                        String[] part = s.Split(new Char[]{':'}, 2);
					    if (part.Length == 2) {
						    String key = part[0].Trim();
						    String value = utils.StripDoubleSpacesAndTrim(part[1]);
    						
						    //ONLY add when it is a valid style attribute in SVG
						    if (SVGAttributes.IsValidAttribute(key, value, attributes)){
							    tagCss[key] = value;
						    }else{
							    //System.out.Println(key + " " + value);
						    }
    						
						    //splitRules(tagCss, key, value);
					    }
				    }
			    }
		    }
		    // inherit css from parent tags, as defined in provided CssInheritanceRules or if property = inherit
		    IDictionary<String, String> css = t.CSS;
		    if (MustInherit(t.Name) && null != t.Parent && null != t.Parent.CSS) {
			    if (null != this.inherit) {
				    foreach (KeyValuePair<String, String> entry in t.Parent.CSS) {
                        String key = entry.Key;
                        if ((tagCss.ContainsKey(key) && CSS.Value.INHERIT.Equals(tagCss.ContainsKey(key) ? tagCss[key] : null)) || CanInherite(t, key)) {
						    //splitRules(css, key, entry.GetValue());
    						
						    if (SVGAttributes.IsValidAttribute(key, entry.Value, attributes)){
							    css[key] = entry.Value;
						    }	
					    }
				    }
			    } else {
				    CssUtils.MapPutAll(css, t.Parent.CSS);
			    }
		    }
		    // overwrite properties (if value != inherit)
		    foreach (KeyValuePair<String, String> e in tagCss) {
                if (!Util.EqualsIgnoreCase(CSS.Value.INHERIT, e.Value)) {
				    css[e.Key] = e.Value;
			    }
		    }

	    }
    		
    	
	    /**
	     * @param css the css map to populate
	     * @param key the property
	     * @param value the value
	    private void SplitRules(IDictionary<String, String> css, String key, String value) {
		    if (CSS.Property.BORDER.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ParseBorder(value));
		    } else if (CSS.Property.MARGIN.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ParseBoxValues(value, "margin-", ""));
		    } else if (CSS.Property.BORDER_WIDTH.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ParseBoxValues(value, "border-", "-width"));
		    } else if (CSS.Property.BORDER_STYLE.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ParseBoxValues(value, "border-", "-style"));
		    } else if (CSS.Property.BORDER_COLOR.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ParseBoxValues(value, "border-", "-color"));
		    } else if (CSS.Property.PADDING.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ParseBoxValues(value, "padding-", ""));
		    } else if (CSS.Property.FONT.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ProcessFont(value));
		    } else if (CSS.Property.LIST_STYLE.EqualsIgnoreCase(key)) {
			    css.PutAll(utils.ProcessListStyle(value));
		    } else {
			    css[key] = value;
		    }
	    }
	     */

	    /**
	     * By setting an implementation of {@link CssInheritanceRules} a developer can set rules on what css selectors are
	     * inherited from parent tags.
	     *
	     * @param cssInheritanceRules the inherit to set
	     */
	    public void SetCssInheritance(ICssInheritanceRules cssInheritanceRules) {
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
	    public void AddCss(String content, String charSet, bool isPersistent) {
		    CssFileProcessor proc = new CssFileProcessor();
		    try {
                retrieve.ProcessFromStream(new MemoryStream(Encoding.GetEncoding(charSet).GetBytes(content)), proc);
			    ICssFile css = proc.GetCss();
			    css.IsPersistent(isPersistent);
			    this.cssFiles.Add(css);
            }
            catch (ArgumentException e) {
			    throw new CssResolverException(e);
		    } catch (IOException e) {
			    throw new CssResolverException(e);
		    }
	    }

	    /**
	     * Add a file to the CssFiles Collection.
	     *
	     * @param href the path, if it starts with http we try to retrieve the file
	     *            from the net, if not we try a normal file operation.
	     */
	    public void AddCssFile(String href, bool isPersistent) {
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
	    public void AddCss(ICssFile file) {
		    this.cssFiles.Add(file);
	    }

	    /* (non-Javadoc)
	     * @see com.itextpdf.tool.xml.pipeline.css.CSSResolver#addCss(java.lang.String)
	     */
	    public void AddCss(String content, bool isPersistent) {
		    CssFileProcessor proc = new CssFileProcessor();
		    IFileRetrieve retrieve = new FileRetrieveImpl();
		    try {
                retrieve.ProcessFromStream(new MemoryStream(Encoding.Default.GetBytes(content)), proc);
			    ICssFile css = proc.GetCss();
			    css.IsPersistent(isPersistent);
			    this.cssFiles.Add(css);
		    } catch (ArgumentException e) {
			    throw new CssResolverException(e);
		    } catch (IOException e) {
			    throw new CssResolverException(e);
		    }

	    }

	    /**
	     * @param inherit the inherit to set
	     */
	    public void SetCssInheritanceRules(ICssInheritanceRules inherit) {
		    this.inherit = inherit;
	    }

	    /**
	     * The {@link FileRetrieve} implementation to use in {@link StyleAttrCSSResolver#addCss(String, bool)}.
	     * @param retrieve the retrieve to set
	     */
        public IFileRetrieve FileRetrieve {
            set {
                this.retrieve = value;
            }
        }

	    /* (non-Javadoc)
	     * @see com.itextpdf.tool.xml.pipeline.css.CSSResolver#clear()
	     */
	    public ICSSResolver Clear() {
		    cssFiles.Clear();
		    return this;
	    }
    }


}
