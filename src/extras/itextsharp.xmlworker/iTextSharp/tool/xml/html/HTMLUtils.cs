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
using iTextSharp.text;

namespace iTextSharp.tool.xml.html {

    /**
     * @author redlab_b
     *
     */
    public static class HTMLUtils {

        /**
         * @param str the string to sanitize
         * @param trim to trim or not to trim
         * @return sanitized string
         */
        private static List<Chunk> Sanitize(String str, bool preserveWhiteSpace, bool replaceNonBreakableSpaces) {
		    StringBuilder builder = new StringBuilder();
            StringBuilder whitespaceBuilder = new StringBuilder();
            List<Chunk> chunkList = new List<Chunk>();
		    bool isWhitespace = str.Length > 0 ? IsWhiteSpace(str[0]) : true;
		    foreach (char c in str) {
			    if (isWhitespace && !IsWhiteSpace(c)) {
                    if (builder.Length == 0) {
                        chunkList.Add(Chunk.CreateWhitespace(whitespaceBuilder.ToString(), preserveWhiteSpace));
                    } else {
                        builder.Append(preserveWhiteSpace ? whitespaceBuilder : new StringBuilder(" "));
                    }
                    whitespaceBuilder = new StringBuilder();
                }

                isWhitespace = IsWhiteSpace(c);
                if (isWhitespace) {
                    whitespaceBuilder.Append(c);
                } else {
                    builder.Append(c);
                }
		    }

            if (builder.Length > 0) {
                chunkList.Add(new Chunk(replaceNonBreakableSpaces ? builder.ToString().Replace(Char.ConvertFromUtf32(0x00a0), " ") : builder.ToString()));
            }

            if (whitespaceBuilder.Length > 0) {
                chunkList.Add(Chunk.CreateWhitespace(whitespaceBuilder.ToString(), preserveWhiteSpace));
            }

		    return chunkList;
	    }

    public static List<Chunk> Sanitize(String str, bool preserveWhiteSpace) {
        return Sanitize(str, preserveWhiteSpace, false);
    }
	/**
	 * Sanitize the String for use in in-line tags.
	 * @param str the string to sanitize
	 * @return a sanitized String for use in in-line tags
	 */
	public static List<Chunk> SanitizeInline(String str, bool preserveWhiteSpace) {
		return Sanitize(str, preserveWhiteSpace, false);
	}

    public static List<Chunk> SanitizeInline(String str, bool preserveWhiteSpace, bool replaceNonBreakableSpaces) {
		return Sanitize(str, preserveWhiteSpace, replaceNonBreakableSpaces);
	}

        /// <summary>
        /// Whitespace as Java sees it.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsWhiteSpace(char c) {
            if (c == '\u00a0' || c == '\u2007' || c == '\u202f')
                return false;
            return char.IsWhiteSpace(c);
        }
    }
}
