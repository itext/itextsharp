/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2022 iText Group NV
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

namespace iTextSharp.text.pdf.mc {

    /**
     * The abstract StructureItem class is extended by StructureMCID and StructureObject.
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public abstract class StructureItem {

        /** The object number of the page to which this structure item belongs. */
        protected int pageref = -1;

        /**
         * Returns the number of the page object to which the structure item belongs.
         * @return a number of the reference of a page
         */
        virtual public int GetPageref() {
            return pageref;
        }

        /**
         * Checks if an MCID corresponds with the MCID stored in the StructureItem.
         * @param pageref	the page reference that needs to be checked
         * @param mcid		the MCID that needs to be checked
         * @return  0 in case there's no MCID (in case of a StructureObject),
         * 		    1 in case the MCID matches,
         * 		   -1 in case there's no match.
         */
        public virtual int CheckMCID(int pageref, int mcid) {
            return 0;
        }

        /**
         * Checks if a StructParent corresponds with the StructParent stored in the StructureItem.
         * @param pageref	the page reference that needs to be checked
         * @param structParent	the structParent that needs to be checked
         * @return  0 in case there's no StructParent (in case of a StructureMCID)
         *          1 in case the StructParent matches,
         *         -1 in case there's no match.
         */
        public virtual int CheckStructParent(int pageref, int structParent) {
            return 0;
        }
    }
}
