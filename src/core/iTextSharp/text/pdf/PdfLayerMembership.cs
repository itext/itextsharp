using System;
using System.Collections.Generic;
/*
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

namespace iTextSharp.text.pdf {
    /**
    * Content typically belongs to a single optional content group,
    * and is visible when the group is <B>ON</B> and invisible when it is <B>OFF</B>. To express more
    * complex visibility policies, content should not declare itself to belong to an optional
    * content group directly, but rather to an optional content membership dictionary
    * represented by this class.
    *
    * @author Paulo Soares
    */
    public class PdfLayerMembership : PdfDictionary, IPdfOCG {
        
        /**
        * Visible only if all of the entries are <B>ON</B>.
        */    
        public static readonly PdfName ALLON = new PdfName("AllOn");
        /**
        * Visible if any of the entries are <B>ON</B>.
        */    
        public static readonly PdfName ANYON = new PdfName("AnyOn");
        /**
        * Visible if any of the entries are <B>OFF</B>.
        */    
        public static readonly PdfName ANYOFF = new PdfName("AnyOff");
        /**
        * Visible only if all of the entries are <B>OFF</B>.
        */    
        public static readonly PdfName ALLOFF = new PdfName("AllOff");

        internal PdfIndirectReference refi;
        internal PdfArray members = new PdfArray();
        internal Dictionary<PdfLayer,object> layers = new Dictionary<PdfLayer,object>();
        
        /**
        * Creates a new, empty, membership layer.
        * @param writer the writer
        */    
        public PdfLayerMembership(PdfWriter writer) : base(PdfName.OCMD) {
            Put(PdfName.OCGS, members);
            refi = writer.PdfIndirectReference;
        }
        
        /**
        * Gets the <CODE>PdfIndirectReference</CODE> that represents this membership layer.
        * @return the <CODE>PdfIndirectReference</CODE> that represents this layer
        */    
        virtual public PdfIndirectReference Ref {
            get {
                return refi;
            }
        }
        
        /**
        * Adds a new member to the layer.
        * @param layer the new member to the layer
        */    
        virtual public void AddMember(PdfLayer layer) {
            if (!layers.ContainsKey(layer)) {
                members.Add(layer.Ref);
                layers[layer] = null;
            }
        }
        
        /**
        * Gets the member layers.
        * @return the member layers
        */    
        virtual public Dictionary<PdfLayer,object>.KeyCollection Layers {
            get {
                return layers.Keys;
            }
        }
        
        /**
        * Sets the visibility policy for content belonging to this
        * membership dictionary. Possible values are ALLON, ANYON, ANYOFF and ALLOFF.
        * The default value is ANYON.
        * @param type the visibility policy
        */    
        virtual public PdfName VisibilityPolicy {
            set {
                Put(PdfName.P, value);
            }
        }
        
        /**
         * Sets the visibility expression for content belonging to this
         * membership dictionary.
         * @param ve A (nested) array of which the first value is /And, /Or, or /Not
         * followed by a series of indirect references to OCGs or other visibility
         * expressions.
         * @since 5.0.2
         */
        virtual public PdfVisibilityExpression VisibilityExpression {
            set {
                Put(PdfName.VE, value);
            }
        }

        /**
        * Gets the dictionary representing the membership layer. It just returns <CODE>this</CODE>.
        * @return the dictionary representing the layer
        */    
        virtual public PdfObject PdfObject {
            get {
                return this;
            }
        }
    }
}
