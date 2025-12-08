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
using iText.Commons.Actions.Confirmations;
using iText.Commons.Actions.Producer;
using iTextSharp.text.Actions.Data;
using iTextSharp.text.Actions.Events;
using iTextSharp.text.pdf.Statistics;

namespace iTextSharp.text {
    internal sealed class UnifiedVersionUtil {
        private static readonly UnifiedVersionUtil.UnifiedVersionEvent UNIFIED_VERSION_EVENT = new UnifiedVersionUtil.UnifiedVersionEvent
            ();

        private UnifiedVersionUtil() {
            // empty constructor
        }
        
        internal static void OnEventUsage() {
            IText5ProductEvent processPdfUnifiedEvent = IText5ProductEvent.CreateProcessPdfEvent();
            EventManager.GetInstance().OnEvent(processPdfUnifiedEvent);
            EventManager.GetInstance().OnEvent(new ConfirmEvent(processPdfUnifiedEvent));
        }

        internal static void OnEventStatistic(long amountOfBytes, int numberOfPages) {
            EventManager.GetInstance().OnEvent(new SizeOfPdfStatisticsEvent(amountOfBytes, IText5ProductData.GetInstance
                ()));
            EventManager.GetInstance().OnEvent(new NumberOfPagesStatisticsEvent(numberOfPages, IText5ProductData.GetInstance
                ()));
        }
        
        internal static String GetProducer(String oldProducer) {
            IText5ProductEvent processPdfUnifiedEvent = IText5ProductEvent.CreateProcessPdfEvent();
            IList<IText5ProductEvent> unifiedEvents = new List<IText5ProductEvent>();
            unifiedEvents.Add(processPdfUnifiedEvent);
            return ProducerBuilder.ModifyProducer(unifiedEvents, oldProducer);
        }

        internal static bool IsAGPLVersion() {
            // returns false if unified license has been loaded, otherwise true
            return UNIFIED_VERSION_EVENT.IsAGPLVersion();
        }

        private class UnifiedVersionEvent : AbstractITextConfigurationEvent {
            protected override void DoAction() {
                throw new InvalidOperationException("Configuration events for util internal purposes are not expected to be sent"
                    );
            }
            
            internal virtual bool IsAGPLVersion() {
                return GetActiveProcessor(IText5ProductData.GetInstance().GetProductName()) == null;
            }
        }
    }
}
