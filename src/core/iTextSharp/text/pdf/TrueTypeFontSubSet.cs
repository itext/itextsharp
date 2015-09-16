using System;
using System.IO;
using System.Collections.Generic;
using System.util.collections;
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

    /** Subsets a True Type font by removing the unneeded glyphs from
     * the font.
     *
     * @author  Paulo Soares
     */
    public class TrueTypeFontSubSet {
        internal static string[] tableNamesSimple = {"cvt ", "fpgm", "glyf", "head",
                                               "hhea", "hmtx", "loca", "maxp", "prep"};
        internal static string[] tableNamesCmap = {"cmap", "cvt ", "fpgm", "glyf", "head",
                                             "hhea", "hmtx", "loca", "maxp", "prep"};
        internal static string[] tableNamesExtra = {"OS/2", "cmap", "cvt ", "fpgm", "glyf", "head",
            "hhea", "hmtx", "loca", "maxp", "name, prep"};
        internal static int[] entrySelectors = {0,0,1,1,2,2,2,2,3,3,3,3,3,3,3,3,4,4,4,4,4};
        internal static int TABLE_CHECKSUM = 0;
        internal static int TABLE_OFFSET = 1;
        internal static int TABLE_LENGTH = 2;
        internal static int HEAD_LOCA_FORMAT_OFFSET = 51;

        internal static int ARG_1_AND_2_ARE_WORDS = 1;
        internal static int WE_HAVE_A_SCALE = 8;
        internal static int MORE_COMPONENTS = 32;
        internal static int WE_HAVE_AN_X_AND_Y_SCALE = 64;
        internal static int WE_HAVE_A_TWO_BY_TWO = 128;
    
    
        /** Contains the location of the several tables. The key is the name of
         * the table and the value is an <CODE>int[3]</CODE> where position 0
         * is the checksum, position 1 is the offset from the start of the file
         * and position 2 is the length of the table.
         */
        protected Dictionary<string, int[]> tableDirectory;
        /** The file in use.
         */
        protected RandomAccessFileOrArray rf;
        /** The file name.
         */
        protected string fileName;
        protected bool includeCmap;
        protected bool includeExtras;
        protected bool locaShortTable;
        protected int[] locaTable;
        protected HashSet2<int> glyphsUsed;
        protected List<int> glyphsInList;
        protected int tableGlyphOffset;
        protected int[] newLocaTable;
        protected byte[] newLocaTableOut;
        protected byte[] newGlyfTable;
        protected int glyfTableRealSize;
        protected int locaTableRealSize;
        protected byte[] outFont;
        protected int fontPtr;
        protected int directoryOffset;

        /** Creates a new TrueTypeFontSubSet
         * @param directoryOffset The offset from the start of the file to the table directory
         * @param fileName the file name of the font
         * @param glyphsUsed the glyphs used
         * @param includeCmap <CODE>true</CODE> if the table cmap is to be included in the generated font
         */
        public TrueTypeFontSubSet(string fileName, RandomAccessFileOrArray rf, HashSet2<int> glyphsUsed, int directoryOffset, bool includeCmap, bool includeExtras) {
            this.fileName = fileName;
            this.rf = rf;
            this.glyphsUsed = glyphsUsed;
            this.includeCmap = includeCmap;
            this.includeExtras = includeExtras;
            this.directoryOffset = directoryOffset;
            glyphsInList = new List<int>(glyphsUsed);
        }
    
        /** Does the actual work of subsetting the font.
         * @throws IOException on error
         * @throws DocumentException on error
         * @return the subset font
         */    
        virtual public byte[] Process() {
            try {
                rf.ReOpen();
                CreateTableDirectory();
                ReadLoca();
                FlatGlyphs();
                CreateNewGlyphTables();
                LocaTobytes();
                AssembleFont();
                return outFont;
            }
            finally {
                try {
                    rf.Close();
                }
                catch  {
                    // empty on purpose
                }
            }
        }
    
        virtual protected void AssembleFont() {
            int[] tableLocation;
            int fullFontSize = 0;
            string[] tableNames;
            if (includeExtras)
                tableNames = tableNamesExtra;
            else {
                if (includeCmap)
                    tableNames = tableNamesCmap;
                else
                    tableNames = tableNamesSimple;
            }
            int tablesUsed = 2;
            int len = 0;
            for (int k = 0; k < tableNames.Length; ++k) {
                string name = tableNames[k];
                if (name.Equals("glyf") || name.Equals("loca"))
                    continue;
                tableDirectory.TryGetValue(name, out tableLocation);
                if (tableLocation == null)
                    continue;
                ++tablesUsed;
                fullFontSize += (tableLocation[TABLE_LENGTH] + 3) & (~3);
            }
            fullFontSize += newLocaTableOut.Length;
            fullFontSize += newGlyfTable.Length;
            int iref = 16 * tablesUsed + 12;
            fullFontSize += iref;
            outFont = new byte[fullFontSize];
            fontPtr = 0;
            WriteFontInt(0x00010000);
            WriteFontShort(tablesUsed);
            int selector = entrySelectors[tablesUsed];
            WriteFontShort((1 << selector) * 16);
            WriteFontShort(selector);
            WriteFontShort((tablesUsed - (1 << selector)) * 16);
            for (int k = 0; k < tableNames.Length; ++k) {
                string name = tableNames[k];
                tableDirectory.TryGetValue(name, out tableLocation);
                if (tableLocation == null)
                    continue;
                WriteFontString(name);
                if (name.Equals("glyf")) {
                    WriteFontInt(CalculateChecksum(newGlyfTable));
                    len = glyfTableRealSize;
                }
                else if (name.Equals("loca")) {
                    WriteFontInt(CalculateChecksum(newLocaTableOut));
                    len = locaTableRealSize;
                }
                else {
                    WriteFontInt(tableLocation[TABLE_CHECKSUM]);
                    len = tableLocation[TABLE_LENGTH];
                }
                WriteFontInt(iref);
                WriteFontInt(len);
                iref += (len + 3) & (~3);
            }
            for (int k = 0; k < tableNames.Length; ++k) {
                string name = tableNames[k];
                tableDirectory.TryGetValue(name, out tableLocation);
                if (tableLocation == null)
                    continue;
                if (name.Equals("glyf")) {
                    Array.Copy(newGlyfTable, 0, outFont, fontPtr, newGlyfTable.Length);
                    fontPtr += newGlyfTable.Length;
                    newGlyfTable = null;
                }
                else if (name.Equals("loca")) {
                    Array.Copy(newLocaTableOut, 0, outFont, fontPtr, newLocaTableOut.Length);
                    fontPtr += newLocaTableOut.Length;
                    newLocaTableOut = null;
                }
                else {
                    rf.Seek(tableLocation[TABLE_OFFSET]);
                    rf.ReadFully(outFont, fontPtr, tableLocation[TABLE_LENGTH]);
                    fontPtr += (tableLocation[TABLE_LENGTH] + 3) & (~3);
                }
            }
        }
    
        virtual protected void CreateTableDirectory() {
            tableDirectory = new Dictionary<string,int[]>();
            rf.Seek(directoryOffset);
            int id = rf.ReadInt();
            if (id != 0x00010000)
                throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.true.type.file", fileName));
            int num_tables = rf.ReadUnsignedShort();
            rf.SkipBytes(6);
            for (int k = 0; k < num_tables; ++k) {
                string tag = ReadStandardString(4);
                int[] tableLocation = new int[3];
                tableLocation[TABLE_CHECKSUM] = rf.ReadInt();
                tableLocation[TABLE_OFFSET] = rf.ReadInt();
                tableLocation[TABLE_LENGTH] = rf.ReadInt();
                tableDirectory[tag] = tableLocation;
            }
        }
    
        virtual protected void ReadLoca() {
            int[] tableLocation;
            tableDirectory.TryGetValue("head", out tableLocation);
            if (tableLocation == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "head", fileName));
            rf.Seek(tableLocation[TABLE_OFFSET] + HEAD_LOCA_FORMAT_OFFSET);
            locaShortTable = (rf.ReadUnsignedShort() == 0);
            tableDirectory.TryGetValue("loca", out tableLocation);
            if (tableLocation == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "loca", fileName));
            rf.Seek(tableLocation[TABLE_OFFSET]);
            if (locaShortTable) {
                int entries = tableLocation[TABLE_LENGTH] / 2;
                locaTable = new int[entries];
                for (int k = 0; k < entries; ++k)
                    locaTable[k] = rf.ReadUnsignedShort() * 2;
            }
            else {
                int entries = tableLocation[TABLE_LENGTH] / 4;
                locaTable = new int[entries];
                for (int k = 0; k < entries; ++k)
                    locaTable[k] = rf.ReadInt();
            }
        }
    
        virtual protected void CreateNewGlyphTables() {
            newLocaTable = new int[locaTable.Length];
            int[] activeGlyphs = new int[glyphsInList.Count];
            for (int k = 0; k < activeGlyphs.Length; ++k)
                activeGlyphs[k] = glyphsInList[k];
            Array.Sort(activeGlyphs);
            int glyfSize = 0;
            for (int k = 0; k < activeGlyphs.Length; ++k) {
                int glyph = activeGlyphs[k];
                glyfSize += locaTable[glyph + 1] - locaTable[glyph];
            }
            glyfTableRealSize = glyfSize;
            glyfSize = (glyfSize + 3) & (~3);
            newGlyfTable = new byte[glyfSize];
            int glyfPtr = 0;
            int listGlyf = 0;
            for (int k = 0; k < newLocaTable.Length; ++k) {
                newLocaTable[k] = glyfPtr;
                if (listGlyf < activeGlyphs.Length && activeGlyphs[listGlyf] == k) {
                    ++listGlyf;
                    newLocaTable[k] = glyfPtr;
                    int start = locaTable[k];
                    int len = locaTable[k + 1] - start;
                    if (len > 0) {
                        rf.Seek(tableGlyphOffset + start);
                        rf.ReadFully(newGlyfTable, glyfPtr, len);
                        glyfPtr += len;
                    }
                }
            }
        }
    
        virtual protected void LocaTobytes() {
            if (locaShortTable)
                locaTableRealSize = newLocaTable.Length * 2;
            else
                locaTableRealSize = newLocaTable.Length * 4;
            newLocaTableOut = new byte[(locaTableRealSize + 3) & (~3)];
            outFont = newLocaTableOut;
            fontPtr = 0;
            for (int k = 0; k < newLocaTable.Length; ++k) {
                if (locaShortTable)
                    WriteFontShort(newLocaTable[k] / 2);
                else
                    WriteFontInt(newLocaTable[k]);
            }
        
        }
    
        virtual protected void FlatGlyphs() {
            int[] tableLocation;
            tableDirectory.TryGetValue("glyf", out tableLocation);
            if (tableLocation == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "glyf", fileName));
            int glyph0 = 0;
            if (!glyphsUsed.Contains(glyph0)) {
                glyphsUsed.Add(glyph0);
                glyphsInList.Add(glyph0);
            }
            tableGlyphOffset = tableLocation[TABLE_OFFSET];
            for (int k = 0; k < glyphsInList.Count; ++k) {
                int glyph = glyphsInList[k];
                CheckGlyphComposite(glyph);
            }
        }

        virtual protected void CheckGlyphComposite(int glyph) {
            int start = locaTable[glyph];
            if (start == locaTable[glyph + 1]) // no contour
                return;
            rf.Seek(tableGlyphOffset + start);
            int numContours = rf.ReadShort();
            if (numContours >= 0)
                return;
            rf.SkipBytes(8);
            for(;;) {
                int flags = rf.ReadUnsignedShort();
                int cGlyph = rf.ReadUnsignedShort();
                if (!glyphsUsed.Contains(cGlyph)) {
                    glyphsUsed.Add(cGlyph);
                    glyphsInList.Add(cGlyph);
                }
                if ((flags & MORE_COMPONENTS) == 0)
                    return;
                int skip;
                if ((flags & ARG_1_AND_2_ARE_WORDS) != 0)
                    skip = 4;
                else
                    skip = 2;
                if ((flags & WE_HAVE_A_SCALE) != 0)
                    skip += 2;
                else if ((flags & WE_HAVE_AN_X_AND_Y_SCALE) != 0)
                    skip += 4;
                if ((flags & WE_HAVE_A_TWO_BY_TWO) != 0)
                    skip += 8;
                rf.SkipBytes(skip);
            }
        }
    
        /** Reads a <CODE>string</CODE> from the font file as bytes using the Cp1252
         *  encoding.
         * @param length the length of bytes to read
         * @return the <CODE>string</CODE> read
         * @throws IOException the font file could not be read
         */
        virtual protected string ReadStandardString(int length) {
            byte[] buf = new byte[length];
            rf.ReadFully(buf);
            return System.Text.Encoding.GetEncoding(1252).GetString(buf);
        }
    
        virtual protected void WriteFontShort(int n) {
            outFont[fontPtr++] = (byte)(n >> 8);
            outFont[fontPtr++] = (byte)(n);
        }

        virtual protected void WriteFontInt(int n) {
            outFont[fontPtr++] = (byte)(n >> 24);
            outFont[fontPtr++] = (byte)(n >> 16);
            outFont[fontPtr++] = (byte)(n >> 8);
            outFont[fontPtr++] = (byte)(n);
        }

        virtual protected void WriteFontString(string s) {
            byte[] b = PdfEncodings.ConvertToBytes(s, BaseFont.WINANSI);
            Array.Copy(b, 0, outFont, fontPtr, b.Length);
            fontPtr += b.Length;
        }
    
        virtual protected int CalculateChecksum(byte[] b) {
            int len = b.Length / 4;
            int v0 = 0;
            int v1 = 0;
            int v2 = 0;
            int v3 = 0;
            int ptr = 0;
            for (int k = 0; k < len; ++k) {
                v3 += (int)b[ptr++] & 0xff;
                v2 += (int)b[ptr++] & 0xff;
                v1 += (int)b[ptr++] & 0xff;
                v0 += (int)b[ptr++] & 0xff;
            }
            return v0 + (v1 << 8) + (v2 << 16) + (v3 << 24);
        }
    }
}
