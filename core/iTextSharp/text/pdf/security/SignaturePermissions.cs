/*
 * $Id: SignaturePermissions.java 5410 2012-09-14 14:34:22Z blowagie $
 *
 * This file is part of the iText (R) project.
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

/**
 * A helper class that tells you more about the type of signature
 * (certification or approval) and the signature's DMP settings.
 */
namespace iTextSharp.text.pdf.security {
	public class SignaturePermissions {

        /**
	     * Class that contains a field lock action and
	     * an array of the fields that are involved.
	     */
        public class FieldLock {
            /** Can be /All, /Exclude or /Include */
            readonly PdfName action;
            /** An array of PdfString values with fieldnames */
            readonly PdfArray fields;
            /** Creates a FieldLock instance */
            public FieldLock(PdfName action, PdfArray fields) {
                this.action = action;
                this.fields = fields;
            }
            /** Getter for the field lock action. */
            public PdfName Action {
                get { return action; }
            }
            /** Getter for the fields involved in the lock action. */
            public PdfArray Fields {
                get { return fields; }
            }
            /** toString method */
            public override String ToString() {
                return action + (fields == null ? "" : fields.ToString());
            }
        }

        /** Is the signature a cerification signature (true) or an approval signature (false)? */
        readonly bool certification;
        /** Is form filling allowed by this signature? */
        readonly bool fillInAllowed = true;
        /** Is adding annotations allowed by this signature? */
        readonly bool annotationsAllowed = true;
        /** Does this signature lock specific fields? */
        readonly List<FieldLock> fieldLocks = new List<FieldLock>();

        /**
         * Creates an object that can inform you about the type of signature
         * in a signature dictionary as well as some of the permissions
         * defined by the signature.
         */
        public SignaturePermissions(PdfDictionary sigDict, SignaturePermissions previous) {
	        if (previous != null) {
		        annotationsAllowed &= previous.AnnotationsAllowed;
		        fillInAllowed &= previous.FillInAllowed;
		        fieldLocks.AddRange(previous.FieldLocks);
	        }
	        PdfArray reference = sigDict.GetAsArray(PdfName.REFERENCE);
	        if (reference != null) {
		        for (int i = 0; i < reference.Size; i++) {
			        PdfDictionary dict = reference.GetAsDict(i);
			        PdfDictionary parameters = dict.GetAsDict(PdfName.TRANSFORMPARAMS);
			        if (PdfName.DOCMDP.Equals(dict.GetAsName(PdfName.TRANSFORMMETHOD)))
				        certification = true;
    			    
			        PdfName action = parameters.GetAsName(PdfName.ACTION);
			        if (action != null)
                        fieldLocks.Add(new FieldLock(action, parameters.GetAsArray(PdfName.FIELDS)));
    			    
			        PdfNumber p = parameters.GetAsNumber(PdfName.P);
			        if (p == null)
				        continue;
			        switch (p.IntValue) {
			        case 1:
				        fillInAllowed = false;
                        break;
			        case 2:
				        annotationsAllowed = false;
                        break;
			        }
		        }
	        }
        }

        /**
         * Getter to find out if the signature is a certification signature.
         * @return true if the signature is a certification signature, false for an approval signature.
         */
        public bool Certification {
            get { return certification; }
        }
        /**
         * Getter to find out if filling out fields is allowed after signing.
         * @return true if filling out fields is allowed
         */
        public bool FillInAllowed {
            get { return fillInAllowed; }
        }
        /**
         * Getter to find out if adding annotations is allowed after signing.
         * @return true if adding annotations is allowed
         */
        public bool AnnotationsAllowed {
            get { return annotationsAllowed; }
        }
        /**
         * Getter for the field lock actions, and fields that are impacted by the action
         * @return an Array with field names
         */
        public List<FieldLock> FieldLocks {
            get { return fieldLocks; }
        }
	}
}
