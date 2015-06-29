using System.IO;
using System.Collections.Generic;
using iTextSharp.text.error_messages;
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
using iTextSharp.text.exceptions;

namespace iTextSharp.text.pdf {

    /**
    * Allows you to add one (or more) existing PDF document(s)
    * and add the form(s) of (an)other PDF document(s).
    * @since 2.1.5
    * @deprecated since 5.5.2
    */
    internal class PdfCopyFormsImp : PdfCopyFieldsImp {

        /**
        * This sets up the output document 
        * @param os The Outputstream pointing to the output document
        * @throws DocumentException
        */
        internal PdfCopyFormsImp(Stream os) : base(os) {
        }
        
        /**
        * This method feeds in the source document
        * @param reader The PDF reader containing the source document
        * @throws DocumentException
        */
        virtual public void CopyDocumentFields(PdfReader reader) {
            if (!reader.IsOpenedWithFullPermissions)
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            if (readers2intrefs.ContainsKey(reader)) {
                reader = new PdfReader(reader);
            }
            else {
                if (reader.Tampered)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.was.reused"));
                reader.ConsolidateNamedDestinations();
                reader.Tampered = true;
            }
            reader.ShuffleSubsetNames();
            readers2intrefs[reader] = new IntHashtable();

            visited[reader] = new IntHashtable();

            fields.Add(reader.AcroFields);
            UpdateCalculationOrder(reader);
        }

        /**
        * This merge fields is slightly different from the mergeFields method
        * of PdfCopyFields.
        */
        internal override void MergeFields() {
            for (int k = 0; k < fields.Count; ++k) {
                IDictionary<string,AcroFields.Item> fd = fields[k].Fields;
                MergeWithMaster(fd);
            }
        }
    }
}
