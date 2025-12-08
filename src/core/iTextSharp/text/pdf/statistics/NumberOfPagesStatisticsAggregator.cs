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
using iText.Commons.Utils;

namespace iTextSharp.text.pdf.Statistics {
    /// <summary>Statistics aggregator which aggregates number of pages in PDF documents.</summary>
    public class NumberOfPagesStatisticsAggregator : AbstractStatisticsAggregator {
        private const int ONE = 1;

        private const int TEN = 10;

        private const int HUNDRED = 100;

        private const int THOUSAND = 1000;

        private const String STRING_FOR_ONE_PAGE = "1";

        private const String STRING_FOR_TEN_PAGES = "2-10";

        private const String STRING_FOR_HUNDRED_PAGES = "11-100";

        private const String STRING_FOR_THOUSAND_PAGES = "101-1000";

        private const String STRING_FOR_INF = "1001+";

        private static readonly IDictionary<int, String> NUMBERS_OF_PAGES;

        // This List must be sorted.
        private static readonly IList<int> SORTED_UPPER_BOUNDS_OF_PAGES = JavaUtil.ArraysAsList(ONE, TEN, HUNDRED, 
            THOUSAND);

        static NumberOfPagesStatisticsAggregator() {
            IDictionary<int, String> temp = new Dictionary<int, String>();
            temp.Put(ONE, STRING_FOR_ONE_PAGE);
            temp.Put(TEN, STRING_FOR_TEN_PAGES);
            temp.Put(HUNDRED, STRING_FOR_HUNDRED_PAGES);
            temp.Put(THOUSAND, STRING_FOR_THOUSAND_PAGES);
            NUMBERS_OF_PAGES = JavaCollectionsUtil.UnmodifiableMap(temp);
        }

        private readonly Object Lock = new Object();

        private readonly IDictionary<String, long?> numberOfDocuments = new LinkedDictionary<String, long?>();

        /// <summary>Aggregates number of pages from the provided event.</summary>
        /// <param name="event">
        /// 
        /// <see cref="NumberOfPagesStatisticsEvent"/>
        /// instance
        /// </param>
        public override void Aggregate(AbstractStatisticsEvent @event) {
            if (!(@event is NumberOfPagesStatisticsEvent)) {
                return;
            }
            int numberOfPages = ((NumberOfPagesStatisticsEvent)@event).GetNumberOfPages();
            String range = STRING_FOR_INF;
            foreach (int upperBound in SORTED_UPPER_BOUNDS_OF_PAGES) {
                if (numberOfPages <= upperBound) {
                    range = NUMBERS_OF_PAGES.Get(upperBound);
                    break;
                }
            }
            lock (Lock) {
                long? documentsOfThisRange = numberOfDocuments.Get(range);
                long? currentValue = documentsOfThisRange == null ? 1L : (documentsOfThisRange.Value + 1L);
                numberOfDocuments.Put(range, currentValue);
            }
        }

        /// <summary>Retrieves Map where keys are ranges of pages and values are the amounts of such PDF documents.</summary>
        /// <returns>
        /// aggregated
        /// <see cref="System.Collections.IDictionary{K, V}"/>
        /// </returns>
        public override Object RetrieveAggregation() {
            return JavaCollectionsUtil.UnmodifiableMap(numberOfDocuments);
        }

        /// <summary>Merges data about amounts of ranges of pages from the provided aggregator into this aggregator.</summary>
        /// <param name="aggregator">
        /// 
        /// <see cref="NumberOfPagesStatisticsAggregator"/>
        /// from which data will be taken.
        /// </param>
        public override void Merge(AbstractStatisticsAggregator aggregator) {
            if (!(aggregator is NumberOfPagesStatisticsAggregator)) {
                return;
            }
            IDictionary<String, long?> numberOfDocuments = ((NumberOfPagesStatisticsAggregator)aggregator).numberOfDocuments;
            lock (Lock) {
                Merge(this.numberOfDocuments, numberOfDocuments);
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
