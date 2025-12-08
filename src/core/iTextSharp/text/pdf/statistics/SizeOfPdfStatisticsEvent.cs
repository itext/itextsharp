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
using System.Collections.Generic;
using iText.Commons.Actions;
using iText.Commons.Actions.Data;
using iText.Commons.Utils;

namespace iTextSharp.text.pdf.Statistics {
    /// <summary>Class which represents event related to size of the PDF document.</summary>
    /// <remarks>Class which represents event related to size of the PDF document. Only for internal usage.</remarks>
    public class SizeOfPdfStatisticsEvent : AbstractStatisticsEvent {
        private const String PDF_SIZE_STATISTICS = "pdfSize";

        private readonly long amountOfBytes;

        /// <summary>
        /// Creates an instance of this class based on the
        /// <see cref="iText.Commons.Actions.Data.ProductData"/>
        /// and the size of the document.
        /// </summary>
        /// <param name="amountOfBytes">the number of bytes in the PDF document during the processing of which the event was sent
        ///     </param>
        /// <param name="productData">is a description of the product which has generated an event</param>
        public SizeOfPdfStatisticsEvent(long amountOfBytes, ProductData productData)
            : base(productData) {
            if (amountOfBytes < 0) {
                throw new ArgumentException("Amount of bytes in the PDF document cannot be less than zero");
            }
            this.amountOfBytes = amountOfBytes;
        }

        /// <summary><inheritDoc/></summary>
        public override AbstractStatisticsAggregator CreateStatisticsAggregatorFromName(String statisticsName) {
            if (PDF_SIZE_STATISTICS.Equals(statisticsName)) {
                return new SizeOfPdfStatisticsAggregator();
            }
            return base.CreateStatisticsAggregatorFromName(statisticsName);
        }

        /// <summary><inheritDoc/></summary>
        public override IList<String> GetStatisticsNames() {
            return JavaCollectionsUtil.SingletonList(PDF_SIZE_STATISTICS);
        }

        /// <summary>Gets number of bytes in the PDF document during the processing of which the event was sent.</summary>
        /// <returns>the number of pages</returns>
        public virtual long GetAmountOfBytes() {
            return amountOfBytes;
        }
    }
}
