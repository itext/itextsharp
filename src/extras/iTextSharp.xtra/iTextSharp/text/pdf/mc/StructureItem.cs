/*
 * $Id: StructureItem.java 5943 2013-08-08 10:03:09Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2013 1T3XT BVBA
 * Authors: Bruno Lowagie, et al.
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
using System.Globalization;
using System.Text;

namespace iTextSharp.text.pdf.mc {

    /**
     * Object that stores an item that is part of the document structure.
     * It can refer to a Marked Content sequence in a page or an object
     * reference.
     */

    public class StructureItem {
        /** The structure element of which the properties are stored. */
        private PdfDictionary structElem;
        /** The reference of the structure element of which the properties are stored. */
        private PdfIndirectReference reference;
        /** MarkedContent IDs in case we're dealing with an MC sequence on a page. */
        private IList<int> mcids = new List<int>();
        /** Object reference in case we're dealing with an obj. */
        private PdfDictionary objr;

        /**
         * Creates a structure item based on a dictionary.
         * The dictionary can be of type StructElem, MCR or OBJR.
         * @param dict	the dictionary that needs to be examined
         */

        public StructureItem(PdfDictionary structElem, PdfIndirectReference reference) {
            this.structElem = structElem;
            this.reference = reference;
            PdfObject obj = structElem.GetDirectObject(PdfName.K);
            if (obj == null)
                return;
            InspectKids(obj);
        }

        /**
         * Inspects the value of a K entry and stores all MCIDs
         * or object references that are encountered.
         * @param object the value of a K-entry
         */

        protected void InspectKids(PdfObject obj) {
            if (obj == null)
                return;
            switch (obj.Type) {
                case PdfObject.NUMBER:
                    mcids.Add(((PdfNumber) obj).IntValue);
                    break;
                case PdfObject.ARRAY:
                    PdfArray array = (PdfArray) obj;
                    for (int i = 0; i < array.Size; i++) {
                        InspectKids(array.GetDirectObject(i));
                    }
                    break;
                case PdfObject.DICTIONARY:
                    PdfDictionary dict = (PdfDictionary) obj;
                    if (dict.CheckType(PdfName.MCR)) {
                        mcids.Add(dict.GetAsNumber(PdfName.MCID).IntValue);
                    } else if (dict.CheckType(PdfName.OBJR)) {
                        objr = dict;
                    }
                    break;
            }
        }

        /**
         * Checks if we're dealing with real content.
         * @return true if there's something to process
         */

        public bool IsRealContent() {
            return (mcids.Count > 0 || objr != null);
        }

        /**
         * Processes a MCID.
         * @param mcid the MCID
         * @return 0 in case there's an OBJR dictionary,
         *         1 in case all MCIDs are now encountered
         *         2 in case there are still MCIDs to process.
         */

        public int Process(int mcid) {
            if (mcids.Contains(mcid)) {
                mcids.Remove(mcid);
                return mcids.Count > 0 ? 2 : 1;
            }
            if (objr != null)
                return 0;
            return -1;
        }

        /**
         * Returns the structure element.
         * @return a dictionary
         */

        public PdfDictionary GetStructElem() {
            return structElem;
        }

        /**
         * Returns the structure element's reference.
         * @return a dictionary
         */

        public PdfIndirectReference GetRef() {
            return reference;
        }

        /**
         * Returns the OBJR dictionary (if present).
         * @return a dictionary of type OBJR or null
         */

        public PdfDictionary GetObjr() {
            return objr;
        }

        /**
         * Returns the object referred to by the OBJR dictionary.
         * Note that this method returns a dictionary which means
         * that only the stream dictionary will be passed in case
         * of an XObject.
         * @return the object referred to by OBJR as a dictionary
         */

        public PdfDictionary GetObj() {
            if (objr == null)
                return null;
            return objr.GetAsDict(PdfName.OBJ);
        }

        /**
         * Creates a String representation of the object.
         * @see java.lang.Object#toString()
         */

        public override String ToString() {
            StringBuilder buf = new StringBuilder();
            if (mcids.Count > 0) {
                buf.Append("MCID: ");
                foreach (int i in mcids) {
                    buf.Append(i.ToString(CultureInfo.InvariantCulture) + " ");
                }
            }
            if (objr != null) {
                buf.Append(objr);
            }
            return buf.ToString();
        }
    }
}
