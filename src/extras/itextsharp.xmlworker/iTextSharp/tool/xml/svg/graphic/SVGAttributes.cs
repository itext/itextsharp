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
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;

namespace iTextSharp.tool.xml.svg.graphic {
/*fill-opacity 1
fill-rule nonzero
fill-opacity 1
stroke-opacity 1
stroke-miterlimit 4
stroke-dasharray 0,11,8,1
stroke-opacity 1
stroke-miterlimit 10*/

    public class SVGAttributes {
        private enum STYLE_TYPE
        {
            OTHER = 0,
            COLOR = 1,
            LINE = 2,
            LINE_CAP = 3,
            LINE_JOIN = 4,
            LINE_DASH = 5
        }

        //a list of integer values
    	
	    static public String STROKE = "stroke";
        static public String STROKE_WIDTH = "stroke-width";
        static public String FILL = "fill";
        static public String LINE_CAP = "stroke-linecap";
        static public String LINE_JOIN = "stroke-linejoin";
        static public String FILL_OPACITY = "fill-opacity";
        static public String STROKE_OPACITY = "stroke-opacity";
        static public String FILL_RULE = "fill-rule";
        static public String STROKE_MASTERLIMIT = "stroke-miterlimit";
        static public String STROKE_DASHARRAY = "stroke-dasharray";
        static public String FONT_SIZE = "font-size";
        static public String FONT_FAMILY = "font-family";


        static public String[] LINE_CAP_VALUES = { "butt", "round", "square" };
        static public String[] LINE_JOIN_VALUES = { "miter", "round", "bevel" };
	    //TODo make this an instance
    	
	    public static IDictionary<String, int?> GetSVGAttributesList(){
            IDictionary<String, int?> attributes = new Dictionary<String, int?>();
		    attributes[STROKE] = (int)STYLE_TYPE.COLOR;
            attributes[STROKE_WIDTH] = (int)STYLE_TYPE.LINE;
            attributes[FILL] = (int) STYLE_TYPE.COLOR;
		    attributes[LINE_CAP] = (int) STYLE_TYPE.LINE_CAP;
		    attributes[LINE_JOIN] = (int) STYLE_TYPE.LINE_JOIN;
            attributes[STROKE_DASHARRAY] = (int)STYLE_TYPE.LINE_DASH;
            attributes[FONT_SIZE] = (int)STYLE_TYPE.LINE;
            attributes[FONT_FAMILY] = (int)STYLE_TYPE.OTHER;
		    return attributes;
	    }
    	
	    public static bool IsValidAttribute(String key, String value, IDictionary<String, int?> attributes) {
	        int? attrValue = null;
		    if (attributes.TryGetValue(key, out attrValue)) {
			    switch (attrValue) {
                case (int)STYLE_TYPE.LINE:
				    return IsValidValueForLine(value);
                case (int)STYLE_TYPE.COLOR:
				    return IsValidColor(value);
                case (int)STYLE_TYPE.LINE_CAP:
				    return IsValidString(value, LINE_CAP_VALUES);
                case (int)STYLE_TYPE.LINE_JOIN:
				    return IsValidString(value, LINE_JOIN_VALUES);
                case (int)STYLE_TYPE.LINE_DASH:
				    return IsValidDashArray(value);
                case (int)STYLE_TYPE.OTHER:
				    return true;
			    default:
				    //System.out.Println(key);
				    break;
			    }
		    }
		    return false;
	    }
    	
	    public static bool IsValidDashArray(String value){
		    //comma or space separated list of integers or NONE?
            if (Util.EqualsIgnoreCase("none", value)) return true;
    		
		    IList<String> list = tags.TagUtils.SplitValueList(value);
		    foreach (String str in list) {
			    try{
				    int result = int.Parse(str);
				    if(result < 0){
					    return false;
				    }
			    }catch(Exception exp){
				    return false;
			    }			
		    }
    		
		    return true;
	    }
    	
	    public static bool IsValidString(String value, String[] possibleValues){
		    if(value == null) return false;		
    		
		    for (int i = 0; i < possibleValues.Length; i++) {
			    if(possibleValues[i].Equals(value)){
				    return true;
			    }
		    }
		    return false;
	    }
    		
	    public static bool IsValidColor(String value){
		    if(value == null) return false;
		    if(value.Equals("none")){
			    return true;
		    }
		    if(HtmlUtilities.DecodeColor(value) != null){
			    return true;
		    }
		    if(CleanColorString(value) != null){
			    return true;
		    }		
		    return false;
	    }
    	
	    //TODO: dit is echt zwaar foefelen
	    //gecopieerd van WebColors and aangepast omdat in de SVG van openstreetmap, kleuren staan met decimale waarden ipv integers
	    public static BaseColor CleanColorString(String name){
		    int[] c = { 0, 0, 0, 255 };
		    name = name.ToLower();
		    if (name.StartsWith("rgb(")) {
                StringTokenizer tok = new StringTokenizer(name, "rgb(), \t\r\n\f");
                for (int k = 0; k < 3; ++k) {
                    String v = tok.NextToken();
                    if (v.EndsWith("%"))
                        c[k] = (int)double.Parse(v.Substring(0, v.Length - 1)) * 255 / 100;
                    else
                        c[k] = (int)double.Parse(v);
                    if (c[k] < 0)
                        c[k] = 0;
                    else if (c[k] > 255)
                        c[k] = 255;
                }
                return new BaseColor(c[0], c[1], c[2], c[3]);
            }
		    return null; 

	    }
    	
	    public static bool IsValidValueForLine(String value){
		    if(value == null) return false;
		    try{
                double result = double.Parse(value);
			    if(result >= 0){
				    return true;
			    }
		    }catch(Exception exp){
			    return false;
		    }
		    return false;
	    }
    }
}
