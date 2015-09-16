using System;
using System.Collections.Generic;
using com.itextpdf.text.pdf;
using iTextSharp.text.log;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.error_messages;

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
 * Formats text in a columnwise form. The text is bound
 * on the left and on the right by a sequence of lines. This allows the column
 * to have any shape, not only rectangular.
 * <P>
 * Several parameters can be set like the first paragraph line indent and
 * extra space between paragraphs.
 * <P>
 * A call to the method <CODE>go</CODE> will return one of the following
 * situations: the column ended or the text ended.
 * <P>
 * I the column ended, a new column definition can be loaded with the method
 * <CODE>setColumns</CODE> and the method <CODE>go</CODE> can be called again.
 * <P>
 * If the text ended, more text can be loaded with <CODE>addText</CODE>
 * and the method <CODE>go</CODE> can be called again.<BR>
 * The only limitation is that one or more complete paragraphs must be loaded
 * each time.
 * <P>
 * Full bidirectional reordering is supported. If the run direction is
 * <CODE>PdfWriter.RUN_DIRECTION_RTL</CODE> the meaning of the horizontal
 * alignments and margins is mirrored.
 * @author Paulo Soares
 */

    public class ColumnText{

        private readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfPTable));

        /** Eliminate the arabic vowels */    
        public int AR_NOVOWEL = ArabicLigaturizer.ar_novowel;
        /** Compose the tashkeel in the ligatures. */    
        public const int AR_COMPOSEDTASHKEEL = ArabicLigaturizer.ar_composedtashkeel;
        /** Do some extra double ligatures. */    
        public const int AR_LIG = ArabicLigaturizer.ar_lig;
        /**
         * Digit shaping option: Replace European digits (U+0030...U+0039) by Arabic-Indic digits.
         */
        public const int DIGITS_EN2AN = ArabicLigaturizer.DIGITS_EN2AN;
    
        /**
         * Digit shaping option: Replace Arabic-Indic digits by European digits (U+0030...U+0039).
         */
        public const int DIGITS_AN2EN = ArabicLigaturizer.DIGITS_AN2EN;
    
        /**
         * Digit shaping option:
         * Replace European digits (U+0030...U+0039) by Arabic-Indic digits
         * if the most recent strongly directional character
         * is an Arabic letter (its Bidi direction value is RIGHT_TO_LEFT_ARABIC).
         * The initial state at the start of the text is assumed to be not an Arabic,
         * letter, so European digits at the start of the text will not change.
         * Compare to DIGITS_ALEN2AN_INIT_AL.
         */
        public const int DIGITS_EN2AN_INIT_LR = ArabicLigaturizer.DIGITS_EN2AN_INIT_LR;
    
        /**
         * Digit shaping option:
         * Replace European digits (U+0030...U+0039) by Arabic-Indic digits
         * if the most recent strongly directional character
         * is an Arabic letter (its Bidi direction value is RIGHT_TO_LEFT_ARABIC).
         * The initial state at the start of the text is assumed to be an Arabic,
         * letter, so European digits at the start of the text will change.
         * Compare to DIGITS_ALEN2AN_INT_LR.
         */
        public const int DIGITS_EN2AN_INIT_AL = ArabicLigaturizer.DIGITS_EN2AN_INIT_AL;
    
        /**
         * Digit type option: Use Arabic-Indic digits (U+0660...U+0669).
         */
        public const int DIGIT_TYPE_AN = ArabicLigaturizer.DIGIT_TYPE_AN;
    
        /**
         * Digit type option: Use Eastern (Extended) Arabic-Indic digits (U+06f0...U+06f9).
         */
        public const int DIGIT_TYPE_AN_EXTENDED = ArabicLigaturizer.DIGIT_TYPE_AN_EXTENDED;
    
        protected int runDirection = PdfWriter.RUN_DIRECTION_DEFAULT;
        public static float GLOBAL_SPACE_CHAR_RATIO = 0;
    
        /** Signals that there is no more text available. */
        public const int NO_MORE_TEXT = 1;
    
        /** Signals that there is no more column. */
        public const int NO_MORE_COLUMN = 2;
    
        /** The column is valid. */
        protected const int LINE_STATUS_OK = 0;
    
        /** The line is out the column limits. */
        protected const int LINE_STATUS_OFFLIMITS = 1;
    
        /** The line cannot fit this column position. */
        protected const int LINE_STATUS_NOLINE = 2;
    
        /** Upper bound of the column. */
        protected float maxY;
    
        /** Lower bound of the column. */
        protected float minY;
    
        protected float leftX;
    
        protected float rightX;
    
        /** The column Element. Default is left Element. */
        protected int alignment = Element.ALIGN_LEFT;
    
        /** The left column bound. */
        protected List<float[]> leftWall;
 
        /** The right column bound. */
        protected List<float[]> rightWall;
    
        /** The chunks that form the text. */
    //    protected ArrayList chunks = new ArrayList();
        protected BidiLine bidiLine;

        protected bool isWordSplit;
    
        /** The current y line location. Text will be written at this line minus the leading. */
        protected float yLine;
    
        /**
         * The X position after the last line that has been written.
         * @since 5.0.3
         */
        protected float lastX;

        /** The leading for the current line. */
        protected float currentLeading = 16;
    
        /** The fixed text leading. */
        protected float fixedLeading = 16;
    
        /** The text leading that is multiplied by the biggest font size in the line. */
        protected float multipliedLeading = 0;
    
        /** The <CODE>PdfContent</CODE> where the text will be written to. */
        protected PdfContentByte canvas;
    
        protected PdfContentByte[] canvases;
    
        /** The line status when trying to fit a line to a column. */
        protected int lineStatus;
    
        /** The first paragraph line indent. */
        protected float indent = 0;
    
        /** The following paragraph lines indent. */
        protected float followingIndent = 0;
    
        /** The right paragraph lines indent. */
        protected float rightIndent = 0;
    
        /** The extra space between paragraphs. */
        protected float extraParagraphSpace = 0;
        
        /** The width of the line when the column is defined as a simple rectangle. */
        protected float rectangularWidth = -1;
    
        protected bool rectangularMode = false;

        /** Holds value of property spaceCharRatio. */
        private float spaceCharRatio = GLOBAL_SPACE_CHAR_RATIO;

        private bool lastWasNewline = true;
        private bool repeatFirstLineIndent = true;

        /** Holds value of property linesWritten. */
        private int linesWritten;
    
        private float firstLineY;

        private bool firstLineYDone = false;
    
        /** Holds value of property arabicOptions. */
        private int arabicOptions = 0;
    
        protected float descender;
    
        protected bool composite = false;
    
        protected ColumnText compositeColumn;
    
        protected internal List<IElement> compositeElements;
    
        protected int listIdx = 0;
    
        /**
         * Pointer for the row in a table that is being dealt with
         * @since 5.1.0
         */
        protected int rowIdx = 0;

        /**
         * The index of the last row that needed to be splitted.
         * @since 5.0.1 changed a boolean into an int
         * -2 value mean it is the first attempt to split the first row.
         * -1 means that we try to avoid splitting current row.
         */
        private int splittedRow = -2;

    
        protected Phrase waitPhrase;
    
        /** if true, first line height is adjusted so that the max ascender touches the top */
        private bool useAscender = false;

        /**
         * @since 5.4.2
         */
        private bool inheritGraphicState = false;

        private bool ignoreSpacingBefore = true;

        /**
         * Creates a <CODE>ColumnText</CODE>.
         * @param text the place where the text will be written to. Can
         * be a template.
         */
        public ColumnText(PdfContentByte canvas) {
            this.canvas = canvas;
        }
    
        /** Creates an independent duplicated of the instance <CODE>org</CODE>.
         * @param org the original <CODE>ColumnText</CODE>
         * @return the duplicated
         */    
        public static ColumnText Duplicate(ColumnText org) {
            ColumnText ct = new ColumnText(null);
            ct.SetACopy(org);
            return ct;
        }
    
        /** Makes this instance an independent copy of <CODE>org</CODE>.
         * @param org the original <CODE>ColumnText</CODE>
         * @return itself
         */    
        virtual public ColumnText SetACopy(ColumnText org) {
            if (org != null) {
                SetSimpleVars(org);
                if (org.bidiLine != null)
                    bidiLine = new BidiLine(org.bidiLine);
            }
            return this;    
        }
    
        virtual protected internal void SetSimpleVars(ColumnText org) {
            maxY = org.maxY;
            minY = org.minY;
            alignment = org.alignment;
            leftWall = null;
            if (org.leftWall != null)
                leftWall = new List<float[]>(org.leftWall);
            rightWall = null;
            if (org.rightWall != null)
                rightWall = new List<float[]>(org.rightWall);
            yLine = org.yLine;
            currentLeading = org.currentLeading;
            fixedLeading = org.fixedLeading;
            multipliedLeading = org.multipliedLeading;
            canvas = org.canvas;
            canvases = org.canvases;
            lineStatus = org.lineStatus;
            indent = org.indent;
            followingIndent = org.followingIndent;
            rightIndent = org.rightIndent;
            extraParagraphSpace = org.extraParagraphSpace;
            rectangularWidth = org.rectangularWidth;
            rectangularMode = org.rectangularMode;
            spaceCharRatio = org.spaceCharRatio;
            lastWasNewline = org.lastWasNewline;
            repeatFirstLineIndent = org.repeatFirstLineIndent;
            linesWritten = org.linesWritten;
            arabicOptions = org.arabicOptions;
            runDirection = org.runDirection;
            descender = org.descender;
            composite = org.composite;
            splittedRow = org.splittedRow;
            if (org.composite) {
                compositeElements = new List<IElement>();
                foreach (IElement compositeElement in org.compositeElements) {
                    if (compositeElement is PdfPTable) {
                        compositeElements.Add(new PdfPTable((PdfPTable)compositeElement));
                    } else {
                        compositeElements.Add(compositeElement);
                    }   
                }
                if (org.compositeColumn != null)
                    compositeColumn = Duplicate(org.compositeColumn);
            }
            listIdx = org.listIdx;
            rowIdx = org.rowIdx;
            firstLineY = org.firstLineY;
            leftX = org.leftX;
            rightX = org.rightX;
            firstLineYDone = org.firstLineYDone;
            waitPhrase = org.waitPhrase;
            useAscender = org.useAscender;
            filledWidth = org.filledWidth;
            adjustFirstLine = org.adjustFirstLine;
            inheritGraphicState = org.inheritGraphicState;
            ignoreSpacingBefore = org.ignoreSpacingBefore;
        }
    
        private void AddWaitingPhrase() {
            if (bidiLine == null && waitPhrase != null) {
                bidiLine = new BidiLine();
                foreach (Chunk ck in waitPhrase.Chunks) {
                    bidiLine.AddChunk(new PdfChunk(ck, null, waitPhrase.TabSettings));
                }
                waitPhrase = null;
            }
        }

        /**
         * Adds a <CODE>Phrase</CODE> to the current text array.
         * @param phrase the text
         */
        virtual public void AddText(Phrase phrase) {
            if (phrase == null || composite)
                return;
            AddWaitingPhrase();
            if (bidiLine == null) {
                waitPhrase = phrase;
                return;
            }
            foreach (Chunk c in phrase.Chunks) {
                bidiLine.AddChunk(new PdfChunk(c, null, phrase.TabSettings));
            }
        }
    
        /**
         * Replaces the current text array with this <CODE>Phrase</CODE>.
         * Anything added previously with AddElement() is lost.
         * @param phrase the text
         */
        virtual public void SetText(Phrase phrase) {
            bidiLine = null;
            composite = false;
            compositeColumn = null;
            compositeElements = null;
            listIdx = 0;
            rowIdx = 0;
            splittedRow = -1;
            waitPhrase = phrase;
        }
    
        /**
         * Adds a <CODE>Chunk</CODE> to the current text array.
         * Will not have any effect if AddElement() was called before.
         * @param chunk the text
         */
        virtual public void AddText(Chunk chunk) {
            if (chunk == null || composite)
                return;
            AddText(new Phrase(chunk));
        }
    
        /**
         * Adds an element. Elements supported are <CODE>Paragraph</CODE>,
         * <CODE>List</CODE>, <CODE>PdfPTable</CODE>, <CODE>Image</CODE> and
         * <CODE>Graphic</CODE>. Also accepts a <code>Chunk</code> and a
         * <code>Phrase</code>, they are placed in a new <code>Paragraph<code>.
         * <p>
         * It removes all the text placed with <CODE>addText()</CODE>.
         *
         * @param element the <CODE>Element</CODE>
         */
        virtual public void AddElement(IElement element) {
            if (element == null)
                return;
            if (element is Image) {
                Image img = (Image)element;
                PdfPTable t = new PdfPTable(1);
                float w = img.WidthPercentage;
                if (w == 0) {
                    t.TotalWidth = img.ScaledWidth;
                    t.LockedWidth = true;
                }
                else
                    t.WidthPercentage = w;
                t.SpacingAfter = img.SpacingAfter;
                t.SpacingBefore = img.SpacingBefore;
                switch (img.Alignment) {
                    case Image.LEFT_ALIGN:
                        t.HorizontalAlignment = Element.ALIGN_LEFT;
                        break;
                    case Image.RIGHT_ALIGN:
                        t.HorizontalAlignment = Element.ALIGN_RIGHT;
                        break;
                    default:
                        t.HorizontalAlignment = Element.ALIGN_CENTER;
                        break;
                }
                PdfPCell c = new PdfPCell(img, true);
                c.Padding = 0;
                c.Border = img.Border;
                c.BorderColor = img.BorderColor;
                c.BorderWidth = img.BorderWidth;
                c.BackgroundColor = img.BackgroundColor;
                t.AddCell(c);
                element = t;
            }
            if (element.Type == Element.CHUNK) {
                element = new Paragraph((Chunk) element);
            } else if (element.Type == Element.PHRASE) {
                element = new Paragraph((Phrase) element);
            } else if (element.Type == Element.PTABLE) {
                ((PdfPTable) element).Init();
            }

            if (element.Type != Element.PARAGRAPH && element.Type != Element.LIST && element.Type != Element.PTABLE && element.Type != Element.YMARK && element.Type != Element.DIV)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("element.not.allowed"));
            if (!composite) {
                composite = true;
                compositeElements = new List<IElement>();
                bidiLine = null;
                waitPhrase = null;
            }
            if (element.Type == Element.PARAGRAPH) {
                Paragraph p = (Paragraph)element;
                IList<IElement> paragraphElements = p.BreakUp();
                foreach (IElement paragraphElement in paragraphElements) {
                    compositeElements.Add(paragraphElement);    
                }
                return;
            }
            compositeElements.Add(element);
        }

        public static bool isAllowedElement(IElement element) {
            int type = element.Type;
            if (type == Element.CHUNK || type == Element.PHRASE
                    || type == Element.PARAGRAPH || type == Element.LIST
                    || type == Element.YMARK || type == Element.PTABLE || type == Element.DIV) return true;
            if (element is Image) return true;
            return false;
        }
    
    
        /**
         * Converts a sequence of lines representing one of the column bounds into
         * an internal format.
         * <p>
         * Each array element will contain a <CODE>float[4]</CODE> representing
         * the line x = ax + b.
         * @param cLine the column array
         * @return the converted array
         */
        virtual protected List<float []> ConvertColumn(float[] cLine) {
            if (cLine.Length < 4)
                throw new Exception(MessageLocalization.GetComposedMessage("no.valid.column.line.found"));
            List<float []> cc = new List<float []>();
            for (int k = 0; k < cLine.Length - 2; k += 2) {
                float x1 = cLine[k];
                float y1 = cLine[k + 1];
                float x2 = cLine[k + 2];
                float y2 = cLine[k + 3];
                if (y1 == y2)
                    continue;
                // x = ay + b
                float a = (x1 - x2) / (y1 - y2);
                float b = x1 - a * y1;
                float[] r = new float[4];
                r[0] = Math.Min(y1, y2);
                r[1] = Math.Max(y1, y2);
                r[2] = a;
                r[3] = b;
                cc.Add(r);
                maxY = Math.Max(maxY, r[1]);
                minY = Math.Min(minY, r[0]);
            }
            if (cc.Count == 0)
                throw new Exception(MessageLocalization.GetComposedMessage("no.valid.column.line.found"));
            return cc;
        }
    
        /**
         * Finds the intersection between the <CODE>yLine</CODE> and the column. It will
         * set the <CODE>lineStatus</CODE> apropriatly.
         * @param wall the column to intersect
         * @return the x coordinate of the intersection
         */
        virtual protected float FindLimitsPoint(List<float []> wall) {
            lineStatus = LINE_STATUS_OK;
            if (yLine < minY || yLine > maxY) {
                lineStatus = LINE_STATUS_OFFLIMITS;
                return 0;
            }
            for (int k = 0; k < wall.Count; ++k) {
                float[] r = wall[k];
                if (yLine < r[0] || yLine > r[1])
                    continue;
                return r[2] * yLine + r[3];
            }
            lineStatus = LINE_STATUS_NOLINE;
            return 0;
        }
    
        /**
         * Finds the intersection between the <CODE>yLine</CODE> and the two
         * column bounds. It will set the <CODE>lineStatus</CODE> apropriatly.
         * @return a <CODE>float[2]</CODE>with the x coordinates of the intersection
         */
        virtual protected float[] FindLimitsOneLine() {
            float x1 = FindLimitsPoint(leftWall);
            if (lineStatus == LINE_STATUS_OFFLIMITS || lineStatus == LINE_STATUS_NOLINE)
                return null;
            float x2 = FindLimitsPoint(rightWall);
            if (lineStatus == LINE_STATUS_NOLINE)
                return null;
            return new float[]{x1, x2};
        }
    
        /**
         * Finds the intersection between the <CODE>yLine</CODE>,
         * the <CODE>yLine-leading</CODE>and the two
         * column bounds. It will set the <CODE>lineStatus</CODE> apropriatly.
         * @return a <CODE>float[4]</CODE>with the x coordinates of the intersection
         */
        virtual protected float[] FindLimitsTwoLines() {
            bool repeat = false;
            for (;;) {
                if (repeat && currentLeading == 0)
                    return null;
                repeat = true;
                float[] x1 = FindLimitsOneLine();
                if (lineStatus == LINE_STATUS_OFFLIMITS)
                    return null;
                yLine -= currentLeading;
                if (lineStatus == LINE_STATUS_NOLINE) {
                    continue;
                }
                float[] x2 = FindLimitsOneLine();
                if (lineStatus == LINE_STATUS_OFFLIMITS)
                    return null;
                if (lineStatus == LINE_STATUS_NOLINE) {
                    yLine -= currentLeading;
                    continue;
                }
                if (x1[0] >= x2[1] || x2[0] >= x1[1])
                    continue;
                return new float[]{x1[0], x1[1], x2[0], x2[1]};
            }
        }
    
        /**
         * Sets the columns bounds. Each column bound is described by a
         * <CODE>float[]</CODE> with the line points [x1,y1,x2,y2,...].
         * The array must have at least 4 elements.
         * @param leftLine the left column bound
         * @param rightLine the right column bound
         */
        virtual public void SetColumns(float[] leftLine, float[] rightLine) {
            maxY = -10e20f;
            minY = 10e20f;
            YLine = Math.Max(leftLine[1], leftLine[leftLine.Length - 1]);
            rightWall = ConvertColumn(rightLine);
            leftWall = ConvertColumn(leftLine);
            rectangularWidth = -1;
            rectangularMode = false;
        }
    
        /**
         * Simplified method for rectangular columns.
         * @param phrase a <CODE>Phrase</CODE>
         * @param llx the lower left x corner
         * @param lly the lower left y corner
         * @param urx the upper right x corner
         * @param ury the upper right y corner
         * @param leading the leading
         * @param alignment the column alignment
         */
        virtual public void SetSimpleColumn(Phrase phrase, float llx, float lly, float urx, float ury, float leading, int alignment) {
            AddText(phrase);
            SetSimpleColumn(llx, lly, urx, ury, leading, alignment);
        }
    
        /**
         * Simplified method for rectangular columns.
         * @param llx the lower left x corner
         * @param lly the lower left y corner
         * @param urx the upper right x corner
         * @param ury the upper right y corner
         * @param leading the leading
         * @param alignment the column alignment
         */
        virtual public void SetSimpleColumn(float llx, float lly, float urx, float ury, float leading, int alignment) {
            Leading = leading;
            this.alignment = alignment;
            SetSimpleColumn(llx, lly, urx, ury);
        }
    
        /**
         * Simplified method for rectangular columns.
         * @param llx
         * @param lly
         * @param urx
         * @param ury
         */
        virtual public void SetSimpleColumn(float llx, float lly, float urx, float ury) {
            leftX = Math.Min(llx, urx);
            maxY = Math.Max(lly, ury);
            minY = Math.Min(lly, ury);
            rightX = Math.Max(llx, urx);
            yLine = maxY;
            rectangularWidth = rightX - leftX;
            if (rectangularWidth < 0)
                rectangularWidth = 0;
            rectangularMode = true;
        }

        /**
         * Simplified method for rectangular columns.
         * @param rect  the rectangle for the column
         */
        virtual public void SetSimpleColumn(Rectangle rect) {
            SetSimpleColumn(rect.Left, rect.Bottom, rect.Right, rect.Top);
        }

        /**
         * Sets the leading fixed and variable. The resultant leading will be
         * fixedLeading+multipliedLeading*maxFontSize where maxFontSize is the
         * size of the bigest font in the line.
         * @param fixedLeading the fixed leading
         * @param multipliedLeading the variable leading
         */
        virtual public void SetLeading(float fixedLeading, float multipliedLeading) {
            this.fixedLeading = fixedLeading;
            this.multipliedLeading = multipliedLeading;
        }
    
        /**
         * Gets the fixed leading
         * @return the leading
         */
        virtual public float Leading {
            get {
                return fixedLeading;
            }

            set {
                this.fixedLeading = value;
                this.multipliedLeading = 0;
            }
        }
    
        /**
         * Gets the variable leading
         * @return the leading
         */
        virtual public float MultipliedLeading {
            get {
                return multipliedLeading;
            }
        }
    
        /**
         * Gets the yLine.
         * @return the yLine
         */
        virtual public float YLine {
            get {
                return yLine;
            }

            set {
                this.yLine = value;
            }
        }
    
        /**
         * Gets the number of rows that were drawn when a table is involved.
         */
        virtual public int RowsDrawn {
            get {
                return rowIdx;
            }
        }

        /**
         * Gets the Element.
         * @return the alignment
         */
        virtual public int Alignment{
            get {
                return alignment;
            }

            set {
                this.alignment = value;
            }
        }
    
        /**
         * Gets the first paragraph line indent.
         * @return the indent
         */
        virtual public float Indent {
            get {
                return indent;
            }

            	
           set {
                SetIndent(value, true);
            }
        }

        /**
         * Sets the first paragraph line indent.
         *
         * @param indent the indent
         * @param	repeatFirstLineIndent	do we need to repeat the indentation of the first line after a newline?
         */
        virtual public void SetIndent(float indent, bool repeatFirstLineIndent) {
            this.indent = indent;
            lastWasNewline = true;
            this.repeatFirstLineIndent = repeatFirstLineIndent;
        }
    
        /**
         * Gets the following paragraph lines indent.
         * @return the indent
         */
        virtual public float FollowingIndent {
            get {
                return followingIndent;
            }

            set {
                this.followingIndent = value;
                lastWasNewline = true;
            }
        }
    
        /**
         * Gets the right paragraph lines indent.
         * @return the indent
         */
        virtual public float RightIndent {
            get {
                return rightIndent;
            }

            set {
                this.rightIndent = value;
                lastWasNewline = true;
            }
        }

        /**
        * Gets the currentLeading.
        *
        * @return the currentLeading
        */
        virtual public float CurrentLeading {
            get { return currentLeading; }
        }

        virtual public bool InheritGraphicState
        {
            get { return inheritGraphicState; }
            set { inheritGraphicState = value; }
        }

        virtual public bool IgnoreSpacingBefore
        {
            get { return ignoreSpacingBefore; }
            set { ignoreSpacingBefore = value; }
        }

        /**
         * Outputs the lines to the document. It is equivalent to <CODE>go(false)</CODE>.
         * @return returns the result of the operation. It can be <CODE>NO_MORE_TEXT</CODE>
         * and/or <CODE>NO_MORE_COLUMN</CODE>
         * @throws DocumentException on error
         */
        virtual public int Go() {
            return Go(false);
        }
    
        /**
         * Outputs the lines to the document. The output can be simulated.
         * @param simulate <CODE>true</CODE> to simulate the writting to the document
         * @return returns the result of the operation. It can be <CODE>NO_MORE_TEXT</CODE>
         * and/or <CODE>NO_MORE_COLUMN</CODE>
         * @throws DocumentException on error
         */
        virtual public int Go(bool simulate) {
            return  Go(simulate, null);
        }

        virtual public int Go(bool simulate, IElement elementToGo) {
            isWordSplit = false;
            if (composite)
                return GoComposite(simulate);

            ListBody lBody = null;
            if (IsTagged(canvas) && elementToGo is ListItem) {
                lBody = ((ListItem)elementToGo).ListBody;
            }

            AddWaitingPhrase();
            if (bidiLine == null)
                return NO_MORE_TEXT;
            descender = 0;
            linesWritten = 0;
            lastX = 0;
            bool dirty = false;
            float ratio = spaceCharRatio;
            Object[] currentValues = new Object[2];
            PdfFont currentFont = null;
            float lastBaseFactor = 0F;
            currentValues[1] = lastBaseFactor;
            PdfDocument pdf = null;
            PdfContentByte graphics = null;
            PdfContentByte text = null;
            firstLineY = float.NaN;
            int localRunDirection = PdfWriter.RUN_DIRECTION_NO_BIDI;
            if (runDirection != PdfWriter.RUN_DIRECTION_DEFAULT)
                localRunDirection = runDirection;
            if (canvas != null) {
                graphics = canvas;
                pdf = canvas.PdfDocument;
                if (!IsTagged(canvas))
                    text = canvas.GetDuplicate(inheritGraphicState);
                else
                    text = canvas;
            }
            else if (!simulate)
                throw new Exception(MessageLocalization.GetComposedMessage("columntext.go.with.simulate.eq.eq.false.and.text.eq.eq.null"));
            if (!simulate) {
                if (ratio == GLOBAL_SPACE_CHAR_RATIO)
                    ratio = text.PdfWriter.SpaceCharRatio;
                else if (ratio < 0.001f)
                    ratio = 0.001f;
            }
            if (!rectangularMode) {
                float max = 0;
                foreach (PdfChunk c in bidiLine.chunks) {
                    max = Math.Max(max, c.Height());
                }
                currentLeading = fixedLeading + max * multipliedLeading;
            }
            float firstIndent = 0;
            PdfLine line;
            float x1;
            int status = 0;
            bool rtl = false;
            while (true) {
                firstIndent = (lastWasNewline ? indent : followingIndent); //
                if (rectangularMode) {
                    if (rectangularWidth <= firstIndent + rightIndent) {
                        status = NO_MORE_COLUMN;
                        if (bidiLine.IsEmpty())
                            status |= NO_MORE_TEXT;
                        break;
                    }
                    if (bidiLine.IsEmpty()) {
                        status = NO_MORE_TEXT;
                        break;
                    }
                    line = bidiLine.ProcessLine(leftX, rectangularWidth - firstIndent - rightIndent, alignment,
                        localRunDirection, arabicOptions, minY, yLine, descender);
                    isWordSplit |= bidiLine.IsWordSplit();
                    if (line == null) {
                        status = NO_MORE_TEXT;
                        break;
                    }
                    float[] maxSize = line.GetMaxSize(fixedLeading, multipliedLeading);
                    if (UseAscender && float.IsNaN(firstLineY))
                        currentLeading = line.Ascender;
                    else
                        currentLeading = Math.Max(maxSize[0], maxSize[1] - descender);
                    if (yLine > maxY || yLine - currentLeading < minY) {
                        status = NO_MORE_COLUMN;
                        bidiLine.Restore();
                        break;
                    }
                    yLine -= currentLeading;
                    if (!simulate && !dirty) {
                        if (line.isRTL && canvas.IsTagged()) {
                            canvas.BeginMarkedContentSequence(PdfName.REVERSEDCHARS);
                            rtl = true;
                        }
                        text.BeginText();
                        dirty = true;
                    }
                    if (float.IsNaN(firstLineY))
                        firstLineY = yLine;
                    UpdateFilledWidth(rectangularWidth - line.WidthLeft);
                    x1 = leftX;
                }
                else
                {
                    float yTemp = yLine - currentLeading;
                    float[] xx = FindLimitsTwoLines();
                    if (xx == null)
                    {
                        status = NO_MORE_COLUMN;
                        if (bidiLine.IsEmpty())
                            status |= NO_MORE_TEXT;
                        yLine = yTemp;
                        break;
                    }
                    if (bidiLine.IsEmpty())
                    {
                        status = NO_MORE_TEXT;
                        yLine = yTemp;
                        break;
                    }
                    x1 = Math.Max(xx[0], xx[2]);
                    float x2 = Math.Min(xx[1], xx[3]);
                    if (x2 - x1 <= firstIndent + rightIndent) {
                        continue;
                    }
                    line = bidiLine.ProcessLine(x1, x2 - x1 - firstIndent - rightIndent, alignment, localRunDirection, arabicOptions, minY, yLine, descender);
                    if (!simulate && !dirty) {
                        if (line.isRTL && canvas.IsTagged()) {
                            canvas.BeginMarkedContentSequence(PdfName.REVERSEDCHARS);
                            rtl = true;
                        }
                        text.BeginText();
                        dirty = true;
                    }
                    line = bidiLine.ProcessLine(x1, x2 - x1 - firstIndent - rightIndent, alignment, localRunDirection,
                                                arabicOptions, minY, yLine, descender);
                    if (line == null)
                    {
                        status = NO_MORE_TEXT;
                        yLine = yTemp;
                        break;
                    }
                }
                if (IsTagged(canvas) && elementToGo is ListItem)
                {
                    if (!float.IsNaN(firstLineY) && !firstLineYDone)
                    {
                        if (!simulate)
                        {
                            ListLabel lbl = ((ListItem)elementToGo).ListLabel;
                            canvas.OpenMCBlock(lbl);
                            Chunk symbol = new Chunk(((ListItem)elementToGo).ListSymbol);
                            symbol.Role = null;
                            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase(symbol), leftX + lbl.Indentation, firstLineY, 0);
                            canvas.CloseMCBlock(lbl);
                        }
                        firstLineYDone = true;
                    }
                }
                if (!simulate)
                {
                    if (lBody != null)
                    {
                        canvas.OpenMCBlock(lBody);
                        lBody = null;
                    }
                    currentValues[0] = currentFont;
                    text.SetTextMatrix(x1 + (line.RTL ? rightIndent : firstIndent) + line.IndentLeft, yLine);
                    lastX = pdf.WriteLineToContent(line, text, graphics, currentValues, ratio);
                    currentFont = (PdfFont) currentValues[0];
                }
                lastWasNewline = repeatFirstLineIndent && line.NewlineSplit;
                yLine -= line.NewlineSplit ? extraParagraphSpace : 0;
                ++linesWritten;
                descender = line.Descender;
            }

            if (dirty) {
                text.EndText();

                if (canvas != text) {
                    canvas.Add(text);
                }

                if (rtl && canvas.IsTagged()) {
                    canvas.EndMarkedContentSequence();
                }
            }
            return status;
        }

        /**
         * Call this after go() to know if any word was split into several lines.
         * @return
         */
        public virtual bool IsWordSplit() {
            return isWordSplit;
        }
    
        /**
         * Sets the extra space between paragraphs.
         * @return the extra space between paragraphs
         */
        virtual public float ExtraParagraphSpace {
            get {
                return extraParagraphSpace;
            }

            set {
                this.extraParagraphSpace = value;
            }
        }
    
        /**
         * Clears the chunk array. A call to <CODE>go()</CODE> will always return
         * NO_MORE_TEXT.
         */
        virtual public void ClearChunks() {
            if (bidiLine != null)
                bidiLine.ClearChunks();
        }
    
        /** Gets the space/character extra spacing ratio for
         * fully justified text.
         * @return the space/character extra spacing ratio
         */    
        virtual public float SpaceCharRatio {
            get {
                return spaceCharRatio;
            }

            set {
                this.spaceCharRatio = value;
            }
        }
    
        /** Gets the run direction.
         * @return the run direction
         */    
        virtual public int RunDirection {
            get {
                return runDirection;
            }

            set {
                if (value < PdfWriter.RUN_DIRECTION_DEFAULT || value > PdfWriter.RUN_DIRECTION_RTL)
                    throw new Exception(MessageLocalization.GetComposedMessage("invalid.run.direction.1", value));
                this.runDirection = value;
            }
        }

        /** Gets the number of lines written.
         * @return the number of lines written
         */
        virtual public int LinesWritten {
            get {
                return this.linesWritten;
            }
        }
    
        /**
         * Gets the X position of the end of the last line that has been written
         * (will not work in simulation mode!).
         * @since 5.0.3
         */
        virtual public float LastX {
            get {
                return lastX;
            }
        }

        /** Sets the arabic shaping options. The option can be AR_NOVOWEL,
         * AR_COMPOSEDTASHKEEL and AR_LIG.
         * @param arabicOptions the arabic shaping options
         */
        virtual public int ArabicOptions {
            set {
                this.arabicOptions = value;
            }
            get {
                return arabicOptions;
            }
        }
    
        /** Gets the biggest descender value of the last line written.
         * @return the biggest descender value of the last line written
         */    
        virtual public float Descender {
            get {
                return descender;
            }
        }
    
        /** Gets the width that the line will occupy after writing.
         * Only the width of the first line is returned.
         * @param phrase the <CODE>Phrase</CODE> containing the line
         * @param runDirection the run direction
         * @param arabicOptions the options for the arabic shaping
         * @return the width of the line
         */    
        public static float GetWidth(Phrase phrase, int runDirection, int arabicOptions) {
            ColumnText ct = new ColumnText(null);
            ct.AddText(phrase);
            ct.AddWaitingPhrase();
            PdfLine line = ct.bidiLine.ProcessLine(0, 20000, Element.ALIGN_LEFT, runDirection, arabicOptions, 0, 0, 0);
            if (line == null)
                return 0;
            else
                return 20000 - line.WidthLeft;
        }
    
        /** Gets the width that the line will occupy after writing.
         * Only the width of the first line is returned.
         * @param phrase the <CODE>Phrase</CODE> containing the line
         * @return the width of the line
         */    
        public static float GetWidth(Phrase phrase) {
            return GetWidth(phrase, PdfWriter.RUN_DIRECTION_NO_BIDI, 0);
        }
    
        /** Shows a line of text. Only the first line is written.
         * @param canvas where the text is to be written to
         * @param alignment the alignment. It is not influenced by the run direction
         * @param phrase the <CODE>Phrase</CODE> with the text
         * @param x the x reference position
         * @param y the y reference position
         * @param rotation the rotation to be applied in degrees counterclockwise
         * @param runDirection the run direction
         * @param arabicOptions the options for the arabic shaping
         */    
        public static void ShowTextAligned(PdfContentByte canvas, int alignment, Phrase phrase, float x, float y, float rotation, int runDirection, int arabicOptions) {
            if (alignment != Element.ALIGN_LEFT && alignment != Element.ALIGN_CENTER
                && alignment != Element.ALIGN_RIGHT)
                alignment = Element.ALIGN_LEFT;
            canvas.SaveState();
            ColumnText ct = new ColumnText(canvas);
            float lly = -1;
            float ury = 2;
            float llx;
            float urx;
            switch (alignment) {
                case Element.ALIGN_LEFT:
                    llx = 0;
                    urx = 20000;
                    break;
                case Element.ALIGN_RIGHT:
                    llx = -20000;
                    urx = 0;
                    break;
                default:
                    llx = -20000;
                    urx = 20000;
                    break;
            }
            if (rotation == 0) {
                llx += x;
                lly += y;
                urx += x;
                ury += y;
            }
            else {
                double alpha = rotation * Math.PI / 180.0;
                float cos = (float)Math.Cos(alpha);
                float sin = (float)Math.Sin(alpha);
                canvas.ConcatCTM(cos, sin, -sin, cos, x, y);
            }
            ct.SetSimpleColumn(phrase, llx, lly, urx, ury, 2, alignment);
            if (runDirection == PdfWriter.RUN_DIRECTION_RTL) {
                if (alignment == Element.ALIGN_LEFT)
                    alignment = Element.ALIGN_RIGHT;
                else if (alignment == Element.ALIGN_RIGHT)
                    alignment = Element.ALIGN_LEFT;
            }
            ct.Alignment = alignment;
            ct.ArabicOptions = arabicOptions;
            ct.RunDirection = runDirection;
            ct.Go();
            canvas.RestoreState();
        }

        /** Shows a line of text. Only the first line is written.
         * @param canvas where the text is to be written to
         * @param alignment the alignment
         * @param phrase the <CODE>Phrase</CODE> with the text
         * @param x the x reference position
         * @param y the y reference position
         * @param rotation the rotation to be applied in degrees counterclockwise
         */    
        public static void ShowTextAligned(PdfContentByte canvas, int alignment, Phrase phrase, float x, float y, float rotation) {
            ShowTextAligned(canvas, alignment, phrase, x, y, rotation, PdfWriter.RUN_DIRECTION_NO_BIDI, 0);
        }

        /**
         * Fits the text to some rectangle adjusting the font size as needed.
         * @param font the font to use
         * @param text the text
         * @param rect the rectangle where the text must fit
         * @param maxFontSize the maximum font size
         * @param runDirection the run direction
         * @return the calculated font size that makes the text fit
         */
        public static float FitText(Font font, String text, Rectangle rect, float maxFontSize, int runDirection) {
            ColumnText ct = null;
            int status = 0;
            if (maxFontSize <= 0) {
                int cr = 0;
                int lf = 0;
                char[] t = text.ToCharArray();
                for (int k = 0; k < t.Length; ++k) {
                    if (t[k] == '\n')
                        ++lf;
                    else if (t[k] == '\r')
                        ++cr;
                }
                int minLines = Math.Max(cr, lf) + 1;
                maxFontSize = Math.Abs(rect.Height) / minLines - 0.001f;
            }
            font.Size = maxFontSize;
            Phrase ph = new Phrase(text, font);
            ct = new ColumnText(null);
            ct.SetSimpleColumn(ph, rect.Left, rect.Bottom, rect.Right, rect.Top, maxFontSize, Element.ALIGN_LEFT);
            ct.RunDirection = runDirection;
            status = ct.Go(true);
            if ((status & NO_MORE_TEXT) != 0)
                return maxFontSize;
            float precision = 0.1f;
            float min = 0;
            float max = maxFontSize;
            float size = maxFontSize;
            for (int k = 0; k < 50; ++k) { //just in case it doesn't converge
                size = (min + max) / 2;
                ct = new ColumnText(null);
                font.Size = size;
                ct.SetSimpleColumn(new Phrase(text, font), rect.Left, rect.Bottom, rect.Right, rect.Top, size, Element.ALIGN_LEFT);
                ct.RunDirection = runDirection;
                status = ct.Go(true);
                if ((status & NO_MORE_TEXT) != 0) {
                    if (max - min < size * precision)
                        return size;
                    min = size;
                }
                else
                    max = size;
            }
            return size;
	    }

        virtual protected int GoComposite(bool simulate) {
            PdfDocument pdf = null;
            if (canvas != null)
                pdf = canvas.pdf;
            if (!rectangularMode)
                throw new DocumentException(MessageLocalization.GetComposedMessage("irregular.columns.are.not.supported.in.composite.mode"));
            linesWritten = 0;
            descender = 0;
            bool firstPass = true;
            bool isRTL = runDirection == PdfWriter.RUN_DIRECTION_RTL;
            main_loop:
            while (true) {
                if (compositeElements.Count == 0)
                    return NO_MORE_TEXT;
                IElement element = compositeElements[0];
                if (element.Type == Element.PARAGRAPH) {
                    Paragraph para = (Paragraph)element;
                    int status = 0;
                    for (int keep = 0; keep < 2; ++keep) {
                        float lastY = yLine;
                        bool createHere = false;
                        if (compositeColumn == null) {
                            compositeColumn = new ColumnText(canvas);
                            compositeColumn.Alignment = para.Alignment;
                            compositeColumn.SetIndent(para.IndentationLeft + para.FirstLineIndent, false);
                            compositeColumn.ExtraParagraphSpace = para.ExtraParagraphSpace;
                            compositeColumn.FollowingIndent = para.IndentationLeft;
                            compositeColumn.RightIndent = para.IndentationRight;
                            compositeColumn.SetLeading(para.Leading, para.MultipliedLeading);
                            compositeColumn.RunDirection = runDirection;
                            compositeColumn.ArabicOptions = arabicOptions;
                            compositeColumn.SpaceCharRatio = spaceCharRatio;
                            compositeColumn.AddText(para);
                            if (!(firstPass && adjustFirstLine)) {
                                yLine -= para.SpacingBefore;
                            }
                            createHere = true;
                        }
                        compositeColumn.UseAscender = ((firstPass || descender == 0) && adjustFirstLine ? useAscender : false);
                        compositeColumn.InheritGraphicState = inheritGraphicState;
                        compositeColumn.leftX = leftX;
                        compositeColumn.rightX = rightX;
                        compositeColumn.yLine = yLine;
                        compositeColumn.rectangularWidth = rectangularWidth;
                        compositeColumn.rectangularMode = rectangularMode;
                        compositeColumn.minY = minY;
                        compositeColumn.maxY = maxY;
                        bool keepCandidate = (para.KeepTogether && createHere && !(firstPass && adjustFirstLine));
                        bool s = simulate || keepCandidate && keep == 0;
                        if (IsTagged(canvas) && !s)
                        {
                            canvas.OpenMCBlock(para);
                        }
                        status = compositeColumn.Go(s);
                        if (IsTagged(canvas) && !s)
                        {
                            canvas.CloseMCBlock(para);
                        }
                        lastX = compositeColumn.LastX;
                        UpdateFilledWidth(compositeColumn.filledWidth);
                        if ((status & NO_MORE_TEXT) == 0 && keepCandidate) {
                            compositeColumn = null;
                            yLine = lastY;
                            return NO_MORE_COLUMN;
                        }
                        if (simulate || !keepCandidate)
                            break;
                        if (keep == 0) {
                            compositeColumn = null;
                            yLine = lastY;
                        }
                    }
                    firstPass = false;
                    if (compositeColumn.linesWritten > 0) {
                        yLine = compositeColumn.yLine;
                        linesWritten += compositeColumn.linesWritten;
                        descender = compositeColumn.descender;
                        isWordSplit |= compositeColumn.IsWordSplit();
                    }
                    currentLeading = compositeColumn.currentLeading;
                    if ((status & NO_MORE_TEXT) != 0) {
                        compositeColumn = null;
                        compositeElements.RemoveAt(0);
                        yLine -= para.SpacingAfter;
                    }
                    if ((status & NO_MORE_COLUMN) != 0) {
                        return NO_MORE_COLUMN;
                    }
                }
                else if (element.Type == Element.LIST) {
                    List list = (List)element;
                    List<IElement> items = list.Items;
                    ListItem item = null;
                    float listIndentation = list.IndentationLeft;
                    int count = 0;
                    Stack<Object[]> stack = new Stack<Object[]>();
                    for (int k = 0; k < items.Count; ++k) {
                        Object obj = items[k];
                        if (obj is ListItem) {
                            if (count == listIdx) {
                                item = (ListItem)obj;
                                break;
                            }
                            else ++count;
                        }
                        else if (obj is List) {
                            stack.Push(new Object[]{list, k, listIndentation});
                            list = (List)obj;
                            items = list.Items;
                            listIndentation += list.IndentationLeft;
                            k = -1;
                            continue;
                        }
                        while (k == items.Count - 1 && stack.Count > 0) {
                            Object[] objs = stack.Pop();
                            list = (List) objs[0];
                            items = list.Items;
                            k = (int) objs[1];
                            listIndentation = (float) objs[2];
                        }
                    }
                    int status = 0;
                    for (int keep = 0; keep < 2; ++keep) {
                        float lastY = yLine;
                        bool createHere = false;
                        if (compositeColumn == null) {
                            if (item == null) {
                                listIdx = 0;
                                compositeElements.RemoveAt(0);
                                goto main_loop;
                            }
                            compositeColumn = new ColumnText(canvas);

                            compositeColumn.UseAscender = ((firstPass || descender == 0) && adjustFirstLine ? useAscender : false);
                            compositeColumn.InheritGraphicState = inheritGraphicState;
                            compositeColumn.Alignment = item.Alignment;
                            compositeColumn.SetIndent(item.IndentationLeft + listIndentation + item.FirstLineIndent, false);
                            compositeColumn.ExtraParagraphSpace = item.ExtraParagraphSpace;
                            compositeColumn.FollowingIndent = compositeColumn.Indent;
                            compositeColumn.RightIndent = item.IndentationRight + list.IndentationRight;
                            compositeColumn.SetLeading(item.Leading, item.MultipliedLeading);
                            compositeColumn.RunDirection = runDirection;
                            compositeColumn.ArabicOptions = arabicOptions;
                            compositeColumn.SpaceCharRatio = spaceCharRatio;
                            compositeColumn.AddText(item);
                            if (!(firstPass && adjustFirstLine)) {
                                yLine -= item.SpacingBefore;
                            }
                            createHere = true;
                        }
                        compositeColumn.leftX = leftX;
                        compositeColumn.rightX = rightX;
                        compositeColumn.yLine = yLine;
                        compositeColumn.rectangularWidth = rectangularWidth;
                        compositeColumn.rectangularMode = rectangularMode;
                        compositeColumn.minY = minY;
                        compositeColumn.maxY = maxY;
                        bool keepCandidate = (item.KeepTogether && createHere && !(firstPass && adjustFirstLine));
                        bool s = simulate || keepCandidate && keep == 0;
                        if (IsTagged(canvas) && !s)
                        {
                            item.ListLabel.Indentation = listIndentation;
                            if (list.GetFirstItem() == item || (compositeColumn != null && compositeColumn.bidiLine != null))
                                canvas.OpenMCBlock(list);
                            canvas.OpenMCBlock(item);
                        }
                        status = compositeColumn.Go(simulate || keepCandidate && keep == 0, item);
                        if (IsTagged(canvas) && !s)
                        {
                            canvas.CloseMCBlock(item.ListBody);
                            canvas.CloseMCBlock(item);
                            if ((list.GetLastItem() == item && (status & NO_MORE_TEXT) != 0) || (status & NO_MORE_COLUMN) != 0)
                                canvas.CloseMCBlock(list);
                        }
                        lastX = compositeColumn.LastX;
                        UpdateFilledWidth(compositeColumn.filledWidth);
                        if ((status & NO_MORE_TEXT) == 0 && keepCandidate) {
                            compositeColumn = null;
                            yLine = lastY;
                            return NO_MORE_COLUMN;
                        }
                        if (simulate || !keepCandidate)
                            break;
                        if (keep == 0) {
                            compositeColumn = null;
                            yLine = lastY;
                        }
                    }
                    firstPass = false;
                    yLine = compositeColumn.yLine;
                    linesWritten += compositeColumn.linesWritten;
                    descender = compositeColumn.descender;
                    currentLeading = compositeColumn.currentLeading;
                    if (!IsTagged(canvas)) {
                        if (!float.IsNaN(compositeColumn.firstLineY) && !compositeColumn.firstLineYDone) {
                            if (!simulate) {
                                if (isRTL)
                                    ShowTextAligned(canvas, Element.ALIGN_RIGHT, new Phrase(item.ListSymbol), compositeColumn.lastX + item.IndentationLeft, compositeColumn.firstLineY, 0, runDirection, arabicOptions);
                                else
                                    ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase(item.ListSymbol), compositeColumn.leftX + listIndentation, compositeColumn.firstLineY, 0);
                            }
                            compositeColumn.firstLineYDone = true;
                        }
                    }
                    if ((status & NO_MORE_TEXT) != 0) {
                        compositeColumn = null;
                        ++listIdx;
                        yLine -= item.SpacingAfter;
                    }
                    if ((status & NO_MORE_COLUMN) != 0) {
                        return NO_MORE_COLUMN;
                    }
                }
                else if (element.Type == Element.PTABLE) {

                    // INITIALISATIONS
                    // get the PdfPTable element
                    PdfPTable table = (PdfPTable) element;
                
                    // tables without a body are dismissed
                    if (table.Size <= table.HeaderRows) {
                        compositeElements.RemoveAt(0);
                        continue;
                    }
                
                    // Y-offset
                    float yTemp = yLine;
                    yTemp += descender;
                    if (rowIdx == 0 && adjustFirstLine)
                        yTemp -= table.SpacingBefore;
                
                    // if there's no space left, ask for new column
                    if (yTemp < minY || yTemp > maxY) {
                        return NO_MORE_COLUMN;
                    }
                
                    // coordinates
                    float yLineWrite = yTemp;
                    float x1 = leftX;
                    currentLeading = 0;
                    // get the width of the table
                    float tableWidth;
                    if (table.LockedWidth) {
                        tableWidth = table.TotalWidth;
                        UpdateFilledWidth(tableWidth);
                    }
                    else {
                        tableWidth = rectangularWidth * table.WidthPercentage / 100f;
                        table.TotalWidth = tableWidth;
                    }
                
                    // HEADERS / FOOTERS

                    // how many header rows are real header rows; how many are footer rows?
                    table.NormalizeHeadersFooters();
                    int headerRows = table.HeaderRows;
                    int footerRows = table.FooterRows;
                    int realHeaderRows = headerRows - footerRows;
                    float footerHeight = table.FooterHeight;
                    float headerHeight = table.HeaderHeight - footerHeight;

                    // do we need to skip the header?
                    bool skipHeader = table.SkipFirstHeader && rowIdx <= realHeaderRows && (table.ElementComplete || rowIdx != realHeaderRows);

                    if (!skipHeader)
                    {
                        yTemp -= headerHeight;
                    }

                    // MEASURE NECESSARY SPACE
                    // how many real rows (not header or footer rows) fit on a page?
                    int k = 0;
                    if (rowIdx < headerRows)
                    {
                        rowIdx = headerRows;
                    }
                    PdfPTable.FittingRows fittingRows = null;
                    //if we skip the last header, firstly, we want to check if table is wholly fit to the page
                    if (table.SkipLastFooter)
                    {
                        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
                        fittingRows = table.GetFittingRows(yTemp - minY, rowIdx);
                    }
                    //if we skip the last footer, but the table doesn't fit to the page - we reserve space for footer
                    //and recalculate fitting rows
                    if (!table.SkipLastFooter || fittingRows.lastRow < table.Size - 1)
                    {
                        yTemp -= footerHeight;
                        fittingRows = table.GetFittingRows(yTemp - minY, rowIdx);
                    }

                    //we want to be able to add more than just a header and a footer
                    if (yTemp < minY || yTemp > maxY)
                    {
                        return NO_MORE_COLUMN;
                    }
                    k = fittingRows.lastRow + 1;
                    yTemp -= fittingRows.height;
                    // splitting row spans

                    LOGGER.Info("Want to split at row " + k);
                    int kTemp = k;
                    while ((kTemp > rowIdx && kTemp < table.Size && table.GetRow(kTemp).MayNotBreak)) {
                        kTemp--;
                    }
                    if (kTemp < (table.Size - 1) && !table.GetRow(kTemp).MayNotBreak) {
                        kTemp++;
                    }
                    if ((kTemp > rowIdx && kTemp < k) || (kTemp == headerRows && table.GetRow(headerRows).MayNotBreak && table.LoopCheck)) {
                        yTemp = minY;
                        k = kTemp;
                	    table.LoopCheck = false;
                    }
                    LOGGER.Info("Will split at row " + k);

                    // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
                    if (table.SplitLate && k > 0) {
                        fittingRows.CorrectLastRowChosen(table, k - 1);
                    }
                    // splitting row spans

                    // only for incomplete tables:
                    if (!table.ElementComplete) {
                        yTemp += footerHeight;
                    }
                
                    // IF ROWS MAY NOT BE SPLIT
                    if (!table.SplitRows) {
                        splittedRow = -1;
                        if (k == rowIdx) {
                            // drop the whole table
                            if (k == table.Size) {
                                compositeElements.RemoveAt(0);
                                continue;
                            }
                            // or drop the row
                            else {
                                // don't drop the row if the table is incomplete and if there's only one row (not counting the header rows)
                                // if there's only one row and this check wasn't here the row would have been deleted and not added at all
                                if (!(!table.Complete && k == 1)) {
                                    table.Rows.RemoveAt(k);
                                }
                                return NO_MORE_COLUMN;
                            }
                        }
                    }
                    // IF ROWS SHOULD NOT BE SPLIT
                    // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
                    //else if (table.isSplitLate() && !table.hasRowspan(k) && rowIdx < k) {
                    //if first row do not fit, splittedRow has value of -2, so in this case we try to avoid split.
                    // Separate constant for the first attempt of splitting first row save us from infinite loop.
                    // Also we check header rows, because in other case we may split right after header row,
                    // while header row can't split before regular rows.
                    else if (table.SplitLate && (rowIdx < k || (splittedRow == -2 && (table.HeaderRows == 0 || table.SkipFirstHeader))))
                    {
                        splittedRow = -1;
                    }
                    // SPLIT ROWS (IF WANTED AND NECESSARY)
                    else if (k < table.Size) {
                        // we calculate the remaining vertical space
                        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
                        // correct yTemp to only take completed rows into account
                        yTemp -= fittingRows.completedRowsHeight - fittingRows.height;
                        // splitting row spans
                        float h = yTemp - minY;
                        // we create a new row with the remaining content
                        PdfPRow newRow = table.GetRow(k).SplitRow(table, k, h);
                        // if the row isn't null add it as an extra row
                        if (newRow == null) {
                            LOGGER.Info("Didn't split row!");
                            splittedRow = -1;
                            if (rowIdx == k)
                                return NO_MORE_COLUMN;
                        }
                        else {
                            // if the row hasn't been split before, we duplicate (part of) the table
                            if (k != splittedRow) {
                                splittedRow = k + 1;
                                table = new PdfPTable(table);
                                compositeElements[0] = table;
                                List<PdfPRow> rows = table.Rows;
                                for (int i = headerRows; i < rowIdx; ++i)
                                    rows[i] = null;
                            }
                            yTemp = minY;
                            table.Rows.Insert(++k, newRow);
                            LOGGER.Info("Inserting row at position " + k);
                        }
                    }
                    // We're no longer in the first pass
                    firstPass = false;
                
                    // if not in simulation mode, draw the table
                    if (!simulate) {
                        // set the alignment
                        switch (table.HorizontalAlignment) {
                            case Element.ALIGN_RIGHT:
                                if (!isRTL)
                                    x1 += rectangularWidth - tableWidth;
                                break;
                            case Element.ALIGN_CENTER:
                                x1 += (rectangularWidth - tableWidth)/2f;
                                break;
                            case Element.ALIGN_LEFT:
                            default:
                                if (isRTL)
                                    x1 += rectangularWidth - tableWidth;
                                break;
                        }
                        // copy the rows that fit on the page in a new table nt
                        PdfPTable nt = PdfPTable.ShallowCopy(table);
                        List<PdfPRow> sub = nt.Rows;
                    
                        // first we add the real header rows (if necessary)
                        if (!skipHeader && realHeaderRows > 0) {
                            List<PdfPRow> rows = table.GetRows(0, realHeaderRows);
                            if (IsTagged(canvas))
                                nt.GetHeader().rows = rows;
                            sub.AddRange(rows);
                        }
                        else {
                            nt.HeaderRows = footerRows;
                        }
                        // then we add the real content
                        {
                            List<PdfPRow> rows = table.GetRows(rowIdx, k);
                            if (IsTagged(canvas)) {
                                nt.GetBody().rows = rows;
                            }
                            sub.AddRange(rows);
                        }
                        // do we need to show a footer?
                        bool showFooter = !table.SkipLastFooter;
                        bool newPageFollows = false;
                        if (k < table.Size) {
                            nt.ElementComplete = true;
                            showFooter = true;
                            newPageFollows = true;
                        }
                        // we add the footer rows if necessary (not for incomplete tables)
                        if (footerRows > 0 && nt.ElementComplete && showFooter) {
                            List<PdfPRow> rows = table.GetRows(realHeaderRows, realHeaderRows + footerRows);
                            if (IsTagged(canvas)) {
                                nt.GetFooter().rows = rows;
                            }
                            sub.AddRange(rows);
                        }
                        else {
                    	    footerRows = 0;
                        }

                        if (sub.Count > 0)
                        {
                            // we need a correction if the last row needs to be extended
                            float rowHeight = 0;
                            int lastIdx = sub.Count - 1 - footerRows;
                            PdfPRow last = sub[lastIdx];
                            if (table.IsExtendLastRow(newPageFollows))
                            {
                                rowHeight = last.MaxHeights;
                                last.MaxHeights = yTemp - minY + rowHeight;
                                yTemp = minY;
                            }

                            // newPageFollows indicates that this table is being split
                            if (newPageFollows)
                            {
                                IPdfPTableEvent tableEvent = table.TableEvent;
                                if (tableEvent is IPdfPTableEventSplit)
                                {
                                    ((IPdfPTableEventSplit) tableEvent).SplitTable(table);
                                }
                            }

                            // now we render the rows of the new table
                            if (canvases != null)
                            {
                                if (IsTagged(canvases[PdfPTable.TEXTCANVAS]))
                                {
                                    canvases[PdfPTable.TEXTCANVAS].OpenMCBlock(table);
                                }
                                nt.WriteSelectedRows(0, -1, 0, -1, x1, yLineWrite, canvases, false);
                                if (IsTagged(canvases[PdfPTable.TEXTCANVAS]))
                                {
                                    canvases[PdfPTable.TEXTCANVAS].CloseMCBlock(table);
                                }
                            }
                            else
                            {
                                if (IsTagged(canvas))
                                {
                                    canvas.OpenMCBlock(table);
                                }
                                nt.WriteSelectedRows(0, -1, 0, -1, x1, yLineWrite, canvas, false);
                                if (IsTagged(canvas))
                                {
                                    canvas.CloseMCBlock(table);
                                }
                            }

                            if (!table.Complete)
                            {
                                table.AddNumberOfRowsWritten(k);
                            }

                            // if the row was split, we copy the content of the last row
                            // that was consumed into the first row shown on the next page
                            if (splittedRow == k && k < table.Size)
                            {
                                PdfPRow splitted = table.Rows[k];
                                splitted.CopyRowContent(nt, lastIdx);
                            }
                            // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
                            else if (k > 0 && k < table.Size)
                            {
                                // continue rowspans on next page
                                // (as the row was not split there is no content to copy)
                                PdfPRow row = table.GetRow(k);
                                row.SplitRowspans(table, k - 1, nt, lastIdx);
                            }
                            // splitting row spans

                            // reset the row height of the last row
                            if (table.IsExtendLastRow(newPageFollows))
                            {
                                last.MaxHeights = rowHeight;
                            }

                            // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz)
                            // newPageFollows indicates that this table is being split
                            if (newPageFollows)
                            {
                                IPdfPTableEvent tableEvent = table.TableEvent;
                                if (tableEvent is IPdfPTableEventAfterSplit)
                                {
                                    PdfPRow row = table.GetRow(k);
                                    ((IPdfPTableEventAfterSplit) tableEvent).AfterSplitTable(table, row, k);
                                }
                            }
                        }
                    }
                    // in simulation mode, we need to take extendLastRow into account
                    else if (table.ExtendLastRow && minY > PdfPRow.BOTTOM_LIMIT) {
                        yTemp = minY;
                    }

                    yLine = yTemp;
                    descender = 0;
                    currentLeading = 0;
                    if (!(skipHeader || table.ElementComplete)) {
                        yLine += footerHeight;
                    }
                     while (k < table.Size) {
                	    if (table.GetRowHeight(k) > 0 || table.HasRowspan(k)) {
                		    break;
                	    }
                	    k++;
                    }
                    if (k >= table.Size) {
                        // Use up space no more than left
                        if (yLine - table.SpacingAfter < minY) {
                            yLine = minY;
                        }
                        else {
                            yLine -= table.SpacingAfter;
                        }
                        compositeElements.RemoveAt(0);
                        splittedRow = -1;
                        rowIdx = 0;
                    }
                    else {
                        if (splittedRow > -1) {
                            List<PdfPRow> rows = table.Rows;
                            for (int i = rowIdx; i < k; ++i)
                                rows[i] = null;
                        }
                        rowIdx = k;
                        return NO_MORE_COLUMN;
                    }
                }
                else if (element.Type == Element.YMARK) {
                    if (!simulate) {
                        IDrawInterface zh = (IDrawInterface)element;
                        zh.Draw(canvas, leftX, minY, rightX, maxY, yLine);
                    }
                    compositeElements.RemoveAt(0);
                } else if (element.Type == Element.DIV) {
                    List<IElement> floatingElements = new List<IElement>();
                    do {
                        floatingElements.Add(element);
                        compositeElements.RemoveAt(0);
                        element = compositeElements.Count > 0 ? compositeElements[0] : null;
                    } while (element != null && element.Type == Element.DIV);

                    FloatLayout fl = new FloatLayout(floatingElements, useAscender);
                    fl.SetSimpleColumn(leftX, minY, rightX, yLine);
                    fl.compositeColumn.IgnoreSpacingBefore = ignoreSpacingBefore;
                    int status = fl.Layout(canvas, simulate);

                    //firstPass = false;
                    yLine = fl.YLine;
                    descender = 0;
                    if ((status & NO_MORE_TEXT) == 0) {
                        foreach (IElement floatingElement in floatingElements) {
                            compositeElements.Add(floatingElement);
                        }
                        return status;
                    }
                } else
                    compositeElements.RemoveAt(0);
            }
        }
    
        /**
         * Sets the canvas.
         * @param canvas
         */
        virtual public PdfContentByte Canvas {
            set {
                canvas = value;
                canvases = null;
                if (compositeColumn != null)
                    compositeColumn.Canvas = value;
            }
            get {
                return canvas;
            }
        }
    
        /**
         * Sets the canvases.
         * @param canvas
         */
        virtual public PdfContentByte[] Canvases {
            set {
                canvases = value;
                canvas = canvases[PdfPTable.TEXTCANVAS];
                if (compositeColumn != null)
                    compositeColumn.Canvases = canvases;
            }
            get {
                return canvases;
            }
        }
    
        /**
         * Checks if the element has a height of 0.
         * @return true or false
         * @since 2.1.2
         */
        virtual public bool ZeroHeightElement() {
            return composite && compositeElements.Count != 0 && ((IElement)compositeElements[0]).Type == Element.YMARK;
        }

        virtual public IList<IElement> CompositeElements {
            get {
    	        return compositeElements;
            }
        }

        /**
         * Enables/Disables adjustment of first line height based on max ascender.
         * @param use enable adjustment if true
         */
        virtual public bool UseAscender {
            set {
                useAscender = value;
            }
            get {
                return useAscender;
            }
        }

        /**
         * Checks the status variable and looks if there's still some text.
         */
        public static bool HasMoreText(int status) {
            return (status & ColumnText.NO_MORE_TEXT) == 0;
        }
        /**
         * Holds value of property filledWidth.
         */
        private float filledWidth;

        /**
         * Sets the real width used by the largest line. Only used to set it
         * to zero to start another measurement.
         * @param filledWidth the real width used by the largest line
         */
        virtual public float FilledWidth {
            set {
                filledWidth = value;
            }
            get {
                return filledWidth;
            }
        }
    
        /**
         * Replaces the <CODE>filledWidth</CODE> if greater than the existing one.
         * @param w the new <CODE>filledWidth</CODE> if greater than the existing one
         */
        virtual public void UpdateFilledWidth(float w) {
            if (w > filledWidth)
                filledWidth = w;
        }

        private bool adjustFirstLine = true;

        /**
         * Sets the first line adjustment. Some objects have properties, like spacing before, that
         * behave differently if the object is the first to be written after go() or not. The first line adjustment is 
         * <CODE>true</CODE> by default but can be changed if several objects are to be placed one
         * after the other in the same column calling go() several times.
         * @param adjustFirstLine <CODE>true</CODE> to adjust the first line, <CODE>false</CODE> otherwise
         */
        virtual public bool AdjustFirstLine {
            set {
                adjustFirstLine = value;
            }
            get {
                return adjustFirstLine;
            }
        }

        
        private static bool IsTagged(PdfContentByte canvas) {
            return (canvas != null) && (canvas.pdf != null) && (canvas.writer != null) && canvas.writer.IsTagged();
        }
    }
}
