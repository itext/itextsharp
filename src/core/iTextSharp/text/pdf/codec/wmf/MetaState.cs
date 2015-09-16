using System;
using System.Collections.Generic;

/*
 * $Id$
 * 
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

namespace iTextSharp.text.pdf.codec.wmf {
    public class MetaState {
    
        public static int TA_NOUPDATECP = 0;
        public static int TA_UPDATECP = 1;
        public static int TA_LEFT = 0;
        public static int TA_RIGHT = 2;
        public static int TA_CENTER = 6;
        public static int TA_TOP = 0;
        public static int TA_BOTTOM = 8;
        public static int TA_BASELINE = 24;
    
        public static int TRANSPARENT = 1;
        public static int OPAQUE = 2;

        public static int ALTERNATE = 1;
        public static int WINDING = 2;

        public Stack<MetaState> savedStates;
        public List<MetaObject> MetaObjects;
        public Point currentPoint;
        public MetaPen currentPen;
        public MetaBrush currentBrush;
        public MetaFont currentFont;
        public BaseColor currentBackgroundColor = BaseColor.WHITE;
        public BaseColor currentTextColor = BaseColor.BLACK;
        public int backgroundMode = OPAQUE;
        public int polyFillMode = ALTERNATE;
        public int lineJoin = 1;
        public int textAlign;
        public int offsetWx;
        public int offsetWy;
        public int extentWx;
        public int extentWy;
        public float scalingX;
        public float scalingY;
    

        /** Creates new MetaState */
        public MetaState() {
            savedStates = new Stack<MetaState>();
            MetaObjects = new List<MetaObject>();
            currentPoint = new Point(0, 0);
            currentPen = new MetaPen();
            currentBrush = new MetaBrush();
            currentFont = new MetaFont();
        }

        public MetaState(MetaState state) {
            metaState = state;
        }
    
        virtual public MetaState metaState {
            set {
                savedStates = value.savedStates;
                MetaObjects = value.MetaObjects;
                currentPoint = value.currentPoint;
                currentPen = value.currentPen;
                currentBrush = value.currentBrush;
                currentFont = value.currentFont;
                currentBackgroundColor = value.currentBackgroundColor;
                currentTextColor = value.currentTextColor;
                backgroundMode = value.backgroundMode;
                polyFillMode = value.polyFillMode;
                textAlign = value.textAlign;
                lineJoin = value.lineJoin;
                offsetWx = value.offsetWx;
                offsetWy = value.offsetWy;
                extentWx = value.extentWx;
                extentWy = value.extentWy;
                scalingX = value.scalingX;
                scalingY = value.scalingY;
            }
        }

        virtual public void AddMetaObject(MetaObject obj) {
            for (int k = 0; k < MetaObjects.Count; ++k) {
                if (MetaObjects[k] == null) {
                    MetaObjects[k] = obj;
                    return;
                }
            }
            MetaObjects.Add(obj);
        }
    
        virtual public void SelectMetaObject(int index, PdfContentByte cb) {
            MetaObject obj = MetaObjects[index];
            if (obj == null)
                return;
            int style;
            switch (obj.Type) {
                case MetaObject.META_BRUSH:
                    currentBrush = (MetaBrush)obj;
                    style = currentBrush.Style;
                    if (style == MetaBrush.BS_SOLID) {
                        BaseColor color = currentBrush.Color;
                        cb.SetColorFill(color);
                    }
                    else if (style == MetaBrush.BS_HATCHED) {
                        BaseColor color = currentBackgroundColor;
                        cb.SetColorFill(color);
                    }
                    break;
                case MetaObject.META_PEN: {
                    currentPen = (MetaPen)obj;
                    style = currentPen.Style;
                    if (style != MetaPen.PS_NULL) {
                        BaseColor color = currentPen.Color;
                        cb.SetColorStroke(color);
                        cb.SetLineWidth(Math.Abs(currentPen.PenWidth * scalingX / extentWx));
                        switch (style) {
                            case MetaPen.PS_DASH:
                                cb.SetLineDash(18, 6, 0);
                                break;
                            case MetaPen.PS_DASHDOT:
                                cb.SetLiteral("[9 6 3 6]0 d\n");
                                break;
                            case MetaPen.PS_DASHDOTDOT:
                                cb.SetLiteral("[9 3 3 3 3 3]0 d\n");
                                break;
                            case MetaPen.PS_DOT:
                                cb.SetLineDash(3, 0);
                                break;
                            default:
                                cb.SetLineDash(0);
                                break;                            
                        }
                    }
                    break;
                }
                case MetaObject.META_FONT: {
                    currentFont = (MetaFont)obj;
                    break;
                }
            }
        }
    
        virtual public void DeleteMetaObject(int index) {
            MetaObjects[index] =  null;
        }
    
        virtual public void SaveState(PdfContentByte cb) {
            cb.SaveState();
            MetaState state = new MetaState(this);
            savedStates.Push(state);
        }

        virtual public void RestoreState(int index, PdfContentByte cb) {
            int pops;
            if (index < 0)
                pops = Math.Min(-index, savedStates.Count);
            else
                pops = Math.Max(savedStates.Count - index, 0);
            if (pops == 0)
                return;
            MetaState state = null;
            while (pops-- != 0) {
                cb.RestoreState();
                state = savedStates.Pop();
            }
            metaState = state;
        }
    
        virtual public void Cleanup(PdfContentByte cb) {
            int k = savedStates.Count;
            while (k-- > 0)
                cb.RestoreState();
        }

        virtual public float TransformX(int x) {
            return ((float)x - offsetWx) * scalingX / extentWx;
        }

        virtual public float TransformY(int y) {
            return (1f - ((float)y - offsetWy) / extentWy) * scalingY;
        }
    
        virtual public float ScalingX {
            set {
                this.scalingX = value;
            }
        }
    
        virtual public float ScalingY {
            set {
                this.scalingY = value;
            }
        }
    
        virtual public int OffsetWx {
            set {
                this.offsetWx = value;
            }
        }
    
        virtual public int OffsetWy {
            set {
                this.offsetWy = value;
            }
        }
    
        virtual public int ExtentWx {
            set {
                this.extentWx = value;
            }
        }
    
        virtual public int ExtentWy {
            set {
                this.extentWy = value;
            }
        }
    
        virtual public float TransformAngle(float angle) {
            float ta = scalingY < 0 ? -angle : angle;
            return (float)(scalingX < 0 ? Math.PI - ta : ta);
        }
        
        virtual public Point CurrentPoint {
            get {
                return currentPoint;
            }

            set {
                currentPoint = value;
            }
        }
    
        virtual public MetaBrush CurrentBrush {
            get {
                return currentBrush;
            }
        }

        virtual public MetaPen CurrentPen {
            get {
                return currentPen;
            }
        }

        virtual public MetaFont CurrentFont {
            get {
                return currentFont;
            }
        }
    
        /** Getter for property currentBackgroundColor.
         * @return Value of property currentBackgroundColor.
         */
        virtual public BaseColor CurrentBackgroundColor {
            get {
                return currentBackgroundColor;
            }

            set {
                this.currentBackgroundColor = value;
            }
        }
    
        /** Getter for property currentTextColor.
         * @return Value of property currentTextColor.
         */
        virtual public BaseColor CurrentTextColor {
            get {
                return currentTextColor;
            }

            set {
                this.currentTextColor = value;
            }
        }
    
        /** Getter for property backgroundMode.
         * @return Value of property backgroundMode.
         */
        virtual public int BackgroundMode {
            get {
                return backgroundMode;
            }

            set {
                this.backgroundMode = value;
            }
        }
    
        /** Getter for property textAlign.
         * @return Value of property textAlign.
         */
        virtual public int TextAlign {
            get {
                return textAlign;
            }

            set {
                this.textAlign = value;
            }
        }
    
        /** Getter for property polyFillMode.
         * @return Value of property polyFillMode.
         */
        virtual public int PolyFillMode {
            get {
                return polyFillMode;
            }

            set {
                this.polyFillMode = value;
            }
        }
    
        virtual public PdfContentByte LineJoinRectangle {
            set {
                if (lineJoin != 0) {
                    lineJoin = 0;
                    value.SetLineJoin(0);
                }
            }
        }
    
        virtual public PdfContentByte LineJoinPolygon {
            set {
                if (lineJoin == 0) {
                    lineJoin = 1;
                    value.SetLineJoin(1);
                }
            }
        }
    
        virtual public bool LineNeutral {
            get {
                return (lineJoin == 0);
            }
        }
    }
}
