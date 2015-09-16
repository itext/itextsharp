using System;

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

    public class PdfTransition {
        /**
         *  Out Vertical Split
         */
        public const int SPLITVOUT      = 1;
        /**
         *  Out Horizontal Split
         */
        public const int SPLITHOUT      = 2;
        /**
         *  In Vertical Split
         */
        public const int SPLITVIN      = 3;
        /**
         *  IN Horizontal Split
         */
        public const int SPLITHIN      = 4;
        /**
         *  Vertical Blinds
         */
        public const int BLINDV      = 5;
        /**
         *  Vertical Blinds
         */
        public const int BLINDH      = 6;
        /**
         *  Inward Box
         */
        public const int INBOX       = 7;
        /**
         *  Outward Box
         */
        public const int OUTBOX      = 8;
        /**
         *  Left-Right Wipe
         */
        public const int LRWIPE      = 9;
        /**
         *  Right-Left Wipe
         */
        public const int RLWIPE     = 10;
        /**
         *  Bottom-Top Wipe
         */
        public const int BTWIPE     = 11;
        /**
         *  Top-Bottom Wipe
         */
        public const int TBWIPE     = 12;
        /**
         *  Dissolve
         */
        public const int DISSOLVE    = 13;
        /**
         *  Left-Right Glitter
         */
        public const int LRGLITTER   = 14;
        /**
         *  Top-Bottom Glitter
         */
        public const int TBGLITTER  = 15;
        /**
         *  Diagonal Glitter
         */
        public const int DGLITTER  = 16;
    
        /**
         *  duration of the transition effect
         */
        protected int duration;
        /**
         *  type of the transition effect
         */
        protected int type;
    
        /**
         *  Constructs a <CODE>Transition</CODE>.
         *
         */
        public PdfTransition() : this(BLINDH) {}
    
        /**
         *  Constructs a <CODE>Transition</CODE>.
         *
         *@param  type      type of the transition effect
         */
        public PdfTransition(int type) : this(type,1) {}
    
        /**
         *  Constructs a <CODE>Transition</CODE>.
         *
         *@param  type      type of the transition effect
         *@param  duration  duration of the transition effect
         */
        public PdfTransition(int type, int duration) {
            this.duration = duration;
            this.type = type;
        }
    
    
        virtual public int Duration {
            get {
                return duration;
            }
        }
    
    
        virtual public int Type {
            get {
                return type;
            }
        }

        virtual public PdfDictionary TransitionDictionary {
            get {
                PdfDictionary trans = new PdfDictionary(PdfName.TRANS);
                switch (type) {
                    case SPLITVOUT:
                        trans.Put(PdfName.S,PdfName.SPLIT);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DM,PdfName.V);
                        trans.Put(PdfName.M,PdfName.O);
                        break;
                    case SPLITHOUT:
                        trans.Put(PdfName.S,PdfName.SPLIT);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DM,PdfName.H);
                        trans.Put(PdfName.M,PdfName.O);
                        break;
                    case SPLITVIN:
                        trans.Put(PdfName.S,PdfName.SPLIT);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DM,PdfName.V);
                        trans.Put(PdfName.M,PdfName.I);
                        break;
                    case SPLITHIN:
                        trans.Put(PdfName.S,PdfName.SPLIT);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DM,PdfName.H);
                        trans.Put(PdfName.M,PdfName.I);
                        break;
                    case BLINDV:
                        trans.Put(PdfName.S,PdfName.BLINDS);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DM,PdfName.V);
                        break;
                    case BLINDH:
                        trans.Put(PdfName.S,PdfName.BLINDS);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DM,PdfName.H);
                        break;
                    case INBOX:
                        trans.Put(PdfName.S,PdfName.BOX);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.M,PdfName.I);
                        break;
                    case OUTBOX:
                        trans.Put(PdfName.S,PdfName.BOX);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.M,PdfName.O);
                        break;
                    case LRWIPE:
                        trans.Put(PdfName.S,PdfName.WIPE);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DI,new PdfNumber(0));
                        break;
                    case RLWIPE:
                        trans.Put(PdfName.S,PdfName.WIPE);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DI,new PdfNumber(180));
                        break;
                    case BTWIPE:
                        trans.Put(PdfName.S,PdfName.WIPE);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DI,new PdfNumber(90));
                        break;
                    case TBWIPE:
                        trans.Put(PdfName.S,PdfName.WIPE);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DI,new PdfNumber(270));
                        break;
                    case DISSOLVE:
                        trans.Put(PdfName.S,PdfName.DISSOLVE);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        break;
                    case LRGLITTER:
                        trans.Put(PdfName.S,PdfName.GLITTER);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DI,new PdfNumber(0));
                        break;
                    case TBGLITTER:
                        trans.Put(PdfName.S,PdfName.GLITTER);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DI,new PdfNumber(270));
                        break;
                    case DGLITTER:
                        trans.Put(PdfName.S,PdfName.GLITTER);
                        trans.Put(PdfName.D,new PdfNumber(duration));
                        trans.Put(PdfName.DI,new PdfNumber(315));
                        break;
                }
                return trans;
            }
        }
    }
}
