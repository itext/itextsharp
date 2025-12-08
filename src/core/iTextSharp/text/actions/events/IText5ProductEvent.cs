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
using iText.Commons.Actions;
using iText.Commons.Actions.Confirmations;
using iText.Commons.Actions.Contexts;
using iText.Commons.Actions.Sequence;
using iTextSharp.text.Actions.Data;

namespace iTextSharp.text.Actions.Events {
    /// <summary>Class represents events registered in iText 5.</summary>
    public class IText5ProductEvent : AbstractProductProcessITextEvent {
        /// <summary>Process PDF event type.</summary>
        public const String PROCESS_PDF = "process-pdf-itext5";

        private readonly String eventType;

        /// <summary>Creates an event associated with a general identifier and additional metadata.</summary>
        /// <param name="sequenceId">is an identifier associated with the event</param>
        /// <param name="metaInfo">is an additional meta info</param>
        /// <param name="eventType">is a string description of the event</param>
        private IText5ProductEvent(SequenceId sequenceId, IMetaInfo metaInfo, String eventType)
            : base(sequenceId, IText5ProductData.GetInstance(), metaInfo, EventConfirmationType.ON_DEMAND) {
            this.eventType = eventType;
        }

        /// <summary>Creates a process-pdf event which is associated with a general identifier and additional metadata.
        ///     </summary>
        /// <returns>the process pdf iText 5 event</returns>
        public static iTextSharp.text.Actions.Events.IText5ProductEvent CreateProcessPdfEvent() {
            return new iTextSharp.text.Actions.Events.IText5ProductEvent(null, null, PROCESS_PDF);
        }

        public override String GetEventType() {
            return eventType;
        }
    }
}
