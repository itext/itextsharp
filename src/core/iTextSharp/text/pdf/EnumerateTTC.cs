using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.error_messages;

using iTextSharp.text;

/*
 * $Id$
 * 
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

namespace iTextSharp.text.pdf {

    /** Enumerates all the fonts inside a True Type Collection.
     *
     * @author  Paulo Soares
     */
    internal class EnumerateTTC : TrueTypeFont {

        protected String[] names;

        internal EnumerateTTC(String ttcFile) {
            fileName = ttcFile;
            rf = new RandomAccessFileOrArray(ttcFile);
            FindNames();
        }

        internal EnumerateTTC(byte[] ttcArray) {
            fileName = "Byte array TTC";
            rf = new RandomAccessFileOrArray(ttcArray);
            FindNames();
        }
    
        internal void FindNames() {
            tables = new Dictionary<String, int[]>();
        
            try {
                String mainTag = ReadStandardString(4);
                if (!mainTag.Equals("ttcf"))
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.ttc.file", fileName));
                rf.SkipBytes(4);
                int dirCount = rf.ReadInt();
                names = new String[dirCount];
                int dirPos = (int)rf.FilePointer;
                for (int dirIdx = 0; dirIdx < dirCount; ++dirIdx) {
                    tables.Clear();
                    rf.Seek(dirPos);
                    rf.SkipBytes(dirIdx * 4);
                    directoryOffset = rf.ReadInt();
                    rf.Seek(directoryOffset);
                    if (rf.ReadInt() != 0x00010000)
                        throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.ttf.file", fileName));
                    int num_tables = rf.ReadUnsignedShort();
                    rf.SkipBytes(6);
                    for (int k = 0; k < num_tables; ++k) {
                        String tag = ReadStandardString(4);
                        rf.SkipBytes(4);
                        int[] table_location = new int[2];
                            table_location[0] = rf.ReadInt();
                        table_location[1] = rf.ReadInt();
                        tables[tag] = table_location;
                    }
                    names[dirIdx] = this.BaseFont;
                }
            }
            finally {
                if (rf != null)
                    rf.Close();
            }
        }
    
        internal String[] Names {
            get {
                return names;
            }
        }
    }
}
