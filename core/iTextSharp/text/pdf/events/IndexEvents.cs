using System;
using System.Collections.Generic;
using System.Text;
using System.util;
using iTextSharp.text;

/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
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
namespace iTextSharp.text.pdf.events {

    /**
    * Class for an index.
    * 
    * @author Michael Niedermair
    */
    public class IndexEvents : PdfPageEventHelper {

        /**
        * keeps the indextag with the pagenumber
        */
        private Dictionary<string,int> indextag = new Dictionary<string,int>();

        /**
        * All the text that is passed to this event, gets registered in the indexentry.
        * 
        * @see com.lowagie.text.pdf.PdfPageEventHelper#onGenericTag(
        *      com.lowagie.text.pdf.PdfWriter, com.lowagie.text.Document,
        *      com.lowagie.text.Rectangle, java.lang.String)
        */
        public override void OnGenericTag(PdfWriter writer, Document document,
                Rectangle rect, String text) {
            indextag[text] = writer.PageNumber;
        }

        // --------------------------------------------------------------------
        /**
        * indexcounter
        */
        private long indexcounter = 0;

        /**
        * the list for the index entry
        */
        private List<Entry> indexentry = new List<Entry>();

        /**
        * Create an index entry.
        *
        * @param text  The text for the Chunk.
        * @param in1   The first level.
        * @param in2   The second level.
        * @param in3   The third level.
        * @return Returns the Chunk.
        */
        public Chunk Create(String text, String in1, String in2,
                String in3) {

            Chunk chunk = new Chunk(text);
            String tag = "idx_" + (indexcounter++);
            chunk.SetGenericTag(tag);
            chunk.SetLocalDestination(tag);
            Entry entry = new Entry(in1, in2, in3, tag, this);
            indexentry.Add(entry);
            return chunk;
        }

        /**
        * Create an index entry.
        *
        * @param text  The text for the Chunk.
        * @param in1   The first level.
        * @return Returns the Chunk.
        */
        public Chunk Create(String text, String in1) {
            return Create(text, in1, "", "");
        }

        /**
        * Create an index entry.
        *
        * @param text  The text for the Chunk.
        * @param in1   The first level.
        * @param in2   The second level.
        * @return Returns the Chunk.
        */
        public Chunk Create(String text, String in1, String in2) {
            return Create(text, in1, in2, "");
        }

        /**
        * Create an index entry.
        *
        * @param text  The text.
        * @param in1   The first level.
        * @param in2   The second level.
        * @param in3   The third level.
        */
        public void Create(Chunk text, String in1, String in2,
                String in3) {

            String tag = "idx_" + (indexcounter++);
            text.SetGenericTag(tag);
            text.SetLocalDestination(tag);
            Entry entry = new Entry(in1, in2, in3, tag, this);
            indexentry.Add(entry);
        }

        /**
        * Create an index entry.
        *
        * @param text  The text.
        * @param in1   The first level.
        */
        public void Create(Chunk text, String in1) {
            Create(text, in1, "", "");
        }

        /**
        * Create an index entry.
        *
        * @param text  The text.
        * @param in1   The first level.
        * @param in2   The second level.
        */
        public void Create(Chunk text, String in1, String in2) {
            Create(text, in1, in2, "");
        }

        private class ISortIndex : IComparer<Entry> {
        
            public int Compare(Entry en1, Entry en2) {

                int rt = 0;
                if (en1.GetIn1() != null && en2.GetIn1() != null) {
                    if ((rt = Util.CompareToIgnoreCase(en1.GetIn1(),en2.GetIn1())) == 0) {
                        // in1 equals
                        if (en1.GetIn2() != null && en2.GetIn2() != null) {
                            if ((rt = Util.CompareToIgnoreCase(en1.GetIn2(), en2.GetIn2())) == 0) {
                                // in2 equals
                                if (en1.GetIn3() != null && en2.GetIn3() != null) {
                                    rt = Util.CompareToIgnoreCase(en1.GetIn3(), en2.GetIn3());
                                }
                            }
                        }
                    }
                }
                return rt;
            }
        }

        /**
        * Comparator for sorting the index
        */
        private IComparer<Entry> comparator = new ISortIndex();

        /**
        * Set the comparator.
        * @param aComparator The comparator to set.
        */
        public void SetComparator(IComparer<Entry> aComparator) {
            comparator = aComparator;
        }

        /**
        * Returns the sorted list with the entries and the collected page numbers.
        * @return Returns the sorted list with the entries and teh collected page numbers.
        */
        public List<Entry> GetSortedEntries() {

            Dictionary<string,Entry> grouped = new Dictionary<string,Entry>();

            for (int i = 0; i < indexentry.Count; i++) {
                Entry e = indexentry[i];
                String key = e.GetKey();

                Entry master;
                grouped.TryGetValue(key, out master);
                if (master != null) {
                    master.AddPageNumberAndTag(e.GetPageNumber(), e.GetTag());
                } else {
                    e.AddPageNumberAndTag(e.GetPageNumber(), e.GetTag());
                    grouped[key] = e;
                }
            }

            // copy to a list and sort it
            List<Entry> sorted = new List<Entry>(grouped.Values);
            sorted.Sort(0, sorted.Count, comparator);
            return sorted;
        }

        // --------------------------------------------------------------------
        /**
        * Class for an index entry.
        * <p>
        * In the first step, only in1, in2,in3 and tag are used.
        * After the collections of the index entries, pagenumbers are used.
        * </p>
        */
        public class Entry {

            /**
            * first level
            */
            private String in1;

            /**
            * second level
            */
            private String in2;

            /**
            * third level
            */
            private String in3;

            /**
            * the tag
            */
            private String tag;

            /**
            * the lsit of all page numbers.
            */
            private List<int> pagenumbers = new List<int>();

            /**
            * the lsit of all tags.
            */
            private List<string> tags = new List<string>();
            private IndexEvents parent;

            /**
            * Create a new object.
            * @param aIn1   The first level.
            * @param aIn2   The second level.
            * @param aIn3   The third level.
            * @param aTag   The tag.
            */
            public Entry(String aIn1, String aIn2, String aIn3,
                    String aTag, IndexEvents parent) {
                in1 = aIn1;
                in2 = aIn2;
                in3 = aIn3;
                tag = aTag;
                this.parent = parent;
            }

            /**
            * Returns the in1.
            * @return Returns the in1.
            */
            public String GetIn1() {
                return in1;
            }

            /**
            * Returns the in2.
            * @return Returns the in2.
            */
            public String GetIn2() {
                return in2;
            }

            /**
            * Returns the in3.
            * @return Returns the in3.
            */
            public String GetIn3() {
                return in3;
            }

            /**
            * Returns the tag.
            * @return Returns the tag.
            */
            public String GetTag() {
                return tag;
            }

            /**
            * Returns the pagenumer for this entry.
            * @return Returns the pagenumer for this entry.
            */
            public int GetPageNumber() {
                if (parent.indextag.ContainsKey(tag))
                    return parent.indextag[tag];
                else
                    return -1;
            }

            /**
            * Add a pagenumber.
            * @param number    The page number.
            * @param tag
            */
            public void AddPageNumberAndTag(int number, String tag) {
                pagenumbers.Add(number);
                tags.Add(tag);
            }

            /**
            * Returns the key for the map-entry.
            * @return Returns the key for the map-entry.
            */
            public String GetKey() {
                return in1 + "!" + in2 + "!" + in3;
            }

            /**
            * Returns the pagenumbers.
            * @return Returns the pagenumbers.
            */
            public List<int> GetPagenumbers() {
                return pagenumbers;
            }

            /**
            * Returns the tags.
            * @return Returns the tags.
            */
            public List<string> GetTags() {
                return tags;
            }

            /**
            * print the entry (only for test)
            * @return the toString implementation of the entry
            */
            public override String ToString() {
                StringBuilder buf = new StringBuilder();
                buf.Append(in1).Append(' ');
                buf.Append(in2).Append(' ');
                buf.Append(in3).Append(' ');
                for (int i = 0; i < pagenumbers.Count; i++) {
                    buf.Append(pagenumbers[i]).Append(' ');
                }
                return buf.ToString();
            }
        }
    }
}
