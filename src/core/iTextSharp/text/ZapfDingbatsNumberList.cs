using System;

/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
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

namespace iTextSharp.text
{
    /**
    * 
    * A special-version of <CODE>LIST</CODE> whitch use zapfdingbats-numbers (1..10).
    * 
    * @see com.lowagie.text.List
    * @version 2003-06-22
    * @author Michael Niedermair
    */
    public class ZapfDingbatsNumberList : List {

        /**
        * which type
        */
        protected int type;

        /**
        * Creates a ZapdDingbatsNumberList
        * @param type the type of list
        * @param symbolIndent  indent
        */
        public ZapfDingbatsNumberList(int type) : base(true) {
            this.type = type;
            float fontsize = symbol.Font.Size;
            symbol.Font = FontFactory.GetFont(FontFactory.ZAPFDINGBATS, fontsize, Font.NORMAL);
            postSymbol = " ";
        }

        /**
        * Creates a ZapdDingbatsNumberList
        * @param type the type of list
        * @param symbolIndent    indent
        */
        public ZapfDingbatsNumberList(int type, int symbolIndent) : base(true, symbolIndent) {
            this.type = type;
            float fontsize = symbol.Font.Size;
            symbol.Font = FontFactory.GetFont(FontFactory.ZAPFDINGBATS, fontsize, Font.NORMAL);
            postSymbol = " ";
        }

        /**
        * get the type
        *
        * @return    char-number
        */
        virtual public int NumberType {
            get {
                return type;
            }
            set {
                type = value;
            }
        }

        /**
        * Adds an <CODE>Object</CODE> to the <CODE>List</CODE>.
        *
        * @param    o    the object to add.
        * @return true if adding the object succeeded
        */
        public override bool Add(IElement o) {
            if (o is ListItem) {
                ListItem item = (ListItem) o;
                Chunk chunk = new Chunk(preSymbol, symbol.Font);
                chunk.Attributes = symbol.Attributes;
                switch (type ) {
                    case 0:
                        chunk.Append(((char)(first + list.Count + 171)).ToString());
                        break;
                    case 1:
                        chunk.Append(((char)(first + list.Count + 181)).ToString());
                        break;
                    case 2:
                        chunk.Append(((char)(first + list.Count + 191)).ToString());
                        break;
                    default:
                        chunk.Append(((char)(first + list.Count + 201)).ToString());
                        break;
                }
                chunk.Append(postSymbol);
                item.ListSymbol = chunk;
                item.SetIndentationLeft(symbolIndent, autoindent);
                item.IndentationRight = 0;
                list.Add(item);
                return true;
            } else if (o is List) {
                List nested = (List) o;
                nested.IndentationLeft = nested.IndentationLeft + symbolIndent;
                first--;
                list.Add(nested);
                return true;
            }
            return false;
        }

	    public override List CloneShallow() {
		    ZapfDingbatsNumberList clone = new ZapfDingbatsNumberList(type);
		    PopulateProperties(clone);
		    return clone;
	    }
    }
}
