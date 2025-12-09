/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2026 iText Group NV
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
using iText.Commons.Utils;

namespace iTextSharp.text.pdf.Statistics {
    /// <summary>Statistics aggregator which aggregates size of PDF documents.</summary>
    public class SizeOfPdfStatisticsAggregator : AbstractStatisticsAggregator {
        private const long MEASURE_COEFFICIENT = 1024;

        private const long SIZE_128KB = 128 * MEASURE_COEFFICIENT;

        private const long SIZE_1MB = MEASURE_COEFFICIENT * MEASURE_COEFFICIENT;

        private const long SIZE_16MB = 16 * MEASURE_COEFFICIENT * MEASURE_COEFFICIENT;

        private const long SIZE_128MB = 128 * MEASURE_COEFFICIENT * MEASURE_COEFFICIENT;

        private const String STRING_FOR_128KB = "<128kb";

        private const String STRING_FOR_1MB = "128kb-1mb";

        private const String STRING_FOR_16MB = "1mb-16mb";

        private const String STRING_FOR_128MB = "16mb-128mb";

        private const String STRING_FOR_INF = "128mb+";

        private static readonly IDictionary<long, String> DOCUMENT_SIZES;

        // This List must be sorted.
        private static readonly IList<long> SORTED_UPPER_BOUNDS_OF_SIZES = JavaUtil.ArraysAsList(SIZE_128KB, SIZE_1MB
            , SIZE_16MB, SIZE_128MB);

        static SizeOfPdfStatisticsAggregator() {
            IDictionary<long, String> temp = new Dictionary<long, String>();
            temp.Put(SIZE_128KB, STRING_FOR_128KB);
            temp.Put(SIZE_1MB, STRING_FOR_1MB);
            temp.Put(SIZE_16MB, STRING_FOR_16MB);
            temp.Put(SIZE_128MB, STRING_FOR_128MB);
            DOCUMENT_SIZES = JavaCollectionsUtil.UnmodifiableMap(temp);
        }

        private readonly Object Lock = new Object();

        private readonly IDictionary<String, long?> numberOfDocuments = new LinkedDictionary<String, long?>();

        /// <summary>Aggregates size of the PDF document from the provided event.</summary>
        /// <param name="event">
        /// 
        /// <see cref="SizeOfPdfStatisticsEvent"/>
        /// instance
        /// </param>
        public override void Aggregate(AbstractStatisticsEvent @event) {
            if (!(@event is SizeOfPdfStatisticsEvent)) {
                return;
            }
            long sizeOfPdf = ((SizeOfPdfStatisticsEvent)@event).GetAmountOfBytes();
            String range = STRING_FOR_INF;
            foreach (long upperBound in SORTED_UPPER_BOUNDS_OF_SIZES) {
                if (sizeOfPdf <= upperBound) {
                    range = DOCUMENT_SIZES.Get(upperBound);
                    break;
                }
            }
            lock (Lock) {
                long? documentsOfThisRange = numberOfDocuments.Get(range);
                long? currentValue = documentsOfThisRange == null ? 1L : (documentsOfThisRange.Value + 1L);
                numberOfDocuments.Put(range, currentValue);
            }
        }

        /// <summary>Retrieves Map where keys are ranges of document sizes and values are the amounts of such PDF documents.
        ///     </summary>
        /// <returns>
        /// aggregated
        /// <see cref="System.Collections.IDictionary{K, V}"/>
        /// </returns>
        public override Object RetrieveAggregation() {
            return JavaCollectionsUtil.UnmodifiableMap(numberOfDocuments);
        }

        /// <summary>Merges data about amounts of ranges of document sizes from the provided aggregator into this aggregator.
        ///     </summary>
        /// <param name="aggregator">
        /// 
        /// <see cref="SizeOfPdfStatisticsAggregator"/>
        /// from which data will be taken.
        /// </param>
        public override void Merge(AbstractStatisticsAggregator aggregator) {
            if (!(aggregator is SizeOfPdfStatisticsAggregator)) {
                return;
            }
            IDictionary<String, long?> amountOfDocuments = ((SizeOfPdfStatisticsAggregator)aggregator).numberOfDocuments;
            lock (Lock) {
                Merge(this.numberOfDocuments, amountOfDocuments);
            }
        }

        // Copied from com.itextpdf.commons.utils.MapUtil.merge, but Java 5 doesn't support lambdas
        private static void Merge(IDictionary<String, long?> destination, IDictionary<String, long?> source) {
            if (destination == source) {
                return;
            }
            foreach (KeyValuePair<String, long?> entry in source) {
                long? value = destination.Get(entry.Key);
                if (value == null) {
                    destination.Put(entry.Key, entry.Value);
                }
                else {
                    if (entry.Value == null) {
                        destination.Put(entry.Key, value);
                    }
                    else {
                        destination.Put(entry.Key, value + entry.Value);
                    }
                }
            }
        }
    }
}
