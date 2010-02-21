using System;
using System.Collections.Generic;
/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

namespace iTextSharp.text.html.simpleparser {

public class ChainedProperties {
    
        public static int[] fontSizes = {8, 10, 12, 14, 18, 24, 36};
        public sealed class ChainedProperty {
            internal String key;
            internal Dictionary<String, String> property;
            internal ChainedProperty(String key, Dictionary<String, String> property) {
                this.key = key;
                this.property = property;
            }
        }
        public List<ChainedProperty> chain = new List<ChainedProperty>();
        
        /** Creates a new instance of ChainedProperties */
        public ChainedProperties() {
        }
        
        public String this[String key] {
            get {
                for (int k = chain.Count - 1; k >= 0; --k) {
                    ChainedProperty p = chain[k];
                    Dictionary<String, String> prop = p.property;
                    if (prop.ContainsKey(key))
                        return prop[key];
                }
                return null;
            }
        }
        
        public bool HasProperty(String key) {
            for (int k = chain.Count - 1; k >= 0; --k) {
                ChainedProperty p = chain[k];
                Dictionary<String, String> prop = p.property;
                if (prop.ContainsKey(key))
                    return true;
            }
            return false;
        }
        
        public void AddToChain(String key, Dictionary<String, String> prop) {
            // adjust the font size
            String value = prop[ElementTags.SIZE];
            if (value != null) {
                if (value.EndsWith("pt")) {
                    prop[ElementTags.SIZE] = value.Substring(0, value.Length - 2);
                }
                else {
                    int s = 0;
                    if (value.StartsWith("+") || value.StartsWith("-")) {
                        String old = this["basefontsize"];
                        if (old == null)
                            old = "12";
                        float f = float.Parse(old, System.Globalization.NumberFormatInfo.InvariantInfo);
                        int c = (int)f;
                        for (int k = fontSizes.Length - 1; k >= 0; --k) {
                            if (c >= fontSizes[k]) {
                                s = k;
                                break;
                            }
                        }
                        int inc = int.Parse(value.StartsWith("+") ? value.Substring(1) : value);
                        s += inc;
                    }
                    else {
                        try {
                            s = int.Parse(value) - 1;
                        }
                        catch {
                            s = 0;
                        }
                    }
                    if (s < 0)
                        s = 0;
                    else if (s >= fontSizes.Length)
                        s = fontSizes.Length - 1;
                    prop[ElementTags.SIZE] = fontSizes[s].ToString();
                }
            }
            chain.Add(new ChainedProperty(key, prop));
        }
        
        public void RemoveChain(String key) {
            for (int k = chain.Count - 1; k >= 0; --k) {
                if (key.Equals(chain[k].key)) {
                    chain.RemoveAt(k);
                    return;
                }
            }
        }
    }
}
