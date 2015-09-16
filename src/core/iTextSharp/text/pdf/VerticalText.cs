using System;
using System.Collections.Generic;
using iTextSharp.text.error_messages;

using iTextSharp.text;

/*
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
namespace iTextSharp.text.pdf {

	/** Writes text vertically. Note that the naming is done according
	 * to horizontal text although it referrs to vertical text.
	 * A line with the alignment Element.LEFT_ALIGN will actually
	 * be top aligned.
	 */
	public class VerticalText {

		/** Signals that there are no more text available. */    
		public static int NO_MORE_TEXT = 1;
	
		/** Signals that there is no more column. */    
		public static int NO_MORE_COLUMN = 2;

		/** The chunks that form the text. */    
		protected List<PdfChunk> chunks = new List<PdfChunk>();

		/** The <CODE>PdfContent</CODE> where the text will be written to. */    
		protected PdfContentByte text;
    
		/** The column Element. Default is left Element. */
		protected int alignment = Element.ALIGN_LEFT;

		/** Marks the chunks to be eliminated when the line is written. */
		protected int currentChunkMarker = -1;
    
		/** The chunk created by the splitting. */
		protected PdfChunk currentStandbyChunk;
    
		/** The chunk created by the splitting. */
		protected string splittedChunkText;

		/** The leading
		 */    
		protected float leading;
    
		/** The X coordinate.
		 */    
		protected float startX;
    
		/** The Y coordinate.
		 */    
		protected float startY;
    
		/** The maximum number of vertical lines.
		 */    
		protected int maxLines;
    
		/** The height of the text.
		 */    
		protected float height;
    
		/** Creates new VerticalText
		 * @param text the place where the text will be written to. Can
		 * be a template.
		 */
		public VerticalText(PdfContentByte text) {
			this.text = text;
		}
    
		/**
		 * Adds a <CODE>Phrase</CODE> to the current text array.
		 * @param phrase the text
		 */
		virtual public void AddText(Phrase phrase) {
			foreach(Chunk c in phrase.Chunks) {
				chunks.Add(new PdfChunk(c, null));
			}
		}
    
		/**
		 * Adds a <CODE>Chunk</CODE> to the current text array.
		 * @param chunk the text
		 */
		virtual public void AddText(Chunk chunk) {
			chunks.Add(new PdfChunk(chunk, null));
		}

		/** Sets the layout.
		 * @param startX the top right X line position
		 * @param startY the top right Y line position
		 * @param height the height of the lines
		 * @param maxLines the maximum number of lines
		 * @param leading the separation between the lines
		 */    
		virtual public void SetVerticalLayout(float startX, float startY, float height, int maxLines, float leading) {
			this.startX = startX;
			this.startY = startY;
			this.height = height;
			this.maxLines = maxLines;
			Leading = leading;
		}
    
		/** Gets the separation between the vertical lines.
		 * @return the vertical line separation
		 */    
		virtual public float Leading {
			get {
				return leading;
			}

			set {
				this.leading = value;
			}
		}
    
		/**
		 * Creates a line from the chunk array.
		 * @param width the width of the line
		 * @return the line or null if no more chunks
		 */
		virtual protected PdfLine CreateLine(float width) {
			if (chunks.Count == 0)
				return null;
			splittedChunkText = null;
			currentStandbyChunk = null;
			PdfLine line = new PdfLine(0, width, alignment, 0);
			string total;
			for (currentChunkMarker = 0; currentChunkMarker < chunks.Count; ++currentChunkMarker) {
				PdfChunk original = chunks[currentChunkMarker];
				total = original.ToString();
				currentStandbyChunk = line.Add(original);
				if (currentStandbyChunk != null) {
					splittedChunkText = original.ToString();
					original.Value = total;
					return line;
				}
			}
			return line;
		}
    
		/**
		 * Normalizes the list of chunks when the line is accepted.
		 */
		virtual protected void ShortenChunkArray() {
			if (currentChunkMarker < 0)
				return;
			if (currentChunkMarker >= chunks.Count) {
				chunks.Clear();
				return;
			}
			PdfChunk split = chunks[currentChunkMarker];
			split.Value = splittedChunkText;
			chunks[currentChunkMarker] = currentStandbyChunk;
			for (int j = currentChunkMarker - 1; j >= 0; --j)
				chunks.RemoveAt(j);
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
			bool dirty = false;
			PdfContentByte graphics = null;
			if (text != null) {
				graphics = text.Duplicate;
			}
			else if (!simulate)
				throw new Exception(MessageLocalization.GetComposedMessage("verticaltext.go.with.simulate.eq.eq.false.and.text.eq.eq.null"));
			int status = 0;
			for (;;) {
				if (maxLines <= 0) {
					status = NO_MORE_COLUMN;
					if (chunks.Count == 0)
						status |= NO_MORE_TEXT;
					break;
				}
				if (chunks.Count == 0) {
					status = NO_MORE_TEXT;
					break;
				}
				PdfLine line = CreateLine(height);
				if (!simulate && !dirty) {
					text.BeginText();
					dirty = true;
				}
				ShortenChunkArray();
				if (!simulate) {
					text.SetTextMatrix(startX, startY - line.IndentLeft);
					WriteLine(line, text, graphics);
				}
				--maxLines;
				startX -= leading;
			}
			if (dirty) {
				text.EndText();
				text.Add(graphics);
			}
			return status;
		}
    
        private float curCharSpace = 0f;
        
        internal void WriteLine(PdfLine line, PdfContentByte text, PdfContentByte graphics)  {
			PdfFont currentFont = null;
			foreach(PdfChunk chunk in line) {
				if (!chunk.IsImage() && chunk.Font.CompareTo(currentFont) != 0) {
					currentFont = chunk.Font;
					text.SetFontAndSize(currentFont.Font, currentFont.Size);
				}
                Object[] textRender = (Object[])chunk.GetAttribute(Chunk.TEXTRENDERMODE);
                int tr = 0;
                float strokeWidth = 1;
				BaseColor color = chunk.Color;
                BaseColor strokeColor = null;
                if (textRender != null) {
                    tr = (int)textRender[0] & 3;
                    if (tr != PdfContentByte.TEXT_RENDER_MODE_FILL)
                        text.SetTextRenderingMode(tr);
                    if (tr == PdfContentByte.TEXT_RENDER_MODE_STROKE || tr == PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE) {
                        strokeWidth = (float)textRender[1];
                        if (strokeWidth != 1)
                            text.SetLineWidth(strokeWidth);
                        strokeColor = (BaseColor)textRender[2];
                        if (strokeColor == null)
                            strokeColor = color;
                        if (strokeColor != null)
                            text.SetColorStroke(strokeColor);
                    }
                }

                object charSpace = chunk.GetAttribute(Chunk.CHAR_SPACING);
                // no char space setting means "leave it as is".
                if (charSpace != null && !curCharSpace.Equals(charSpace)) {
            	    curCharSpace = (float)charSpace;
            	    text.SetCharacterSpacing(curCharSpace);
                }
				if (color != null)
					text.SetColorFill(color);
				text.ShowText(chunk.ToString());
				if (color != null)
					text.ResetRGBColorFill();
                if (tr != PdfContentByte.TEXT_RENDER_MODE_FILL)
                    text.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);
                if (strokeColor != null)
                    text.ResetRGBColorStroke();
                if (strokeWidth != 1)
                    text.SetLineWidth(1);
			}
		}
    
		/** Sets the new text origin.
		 * @param startX the X coordinate
		 * @param startY the Y coordinate
		 */    
		virtual public void SetOrigin(float startX, float startY) {
			this.startX = startX;
			this.startY = startY;
		}
    
		/** Gets the X coordinate where the next line will be writen. This value will change
		 * after each call to <code>go()</code>.
		 * @return  the X coordinate
		 */    
		virtual public float OriginX {
			get {
				return startX;
			}
		}

		/** Gets the Y coordinate where the next line will be writen.
		 * @return  the Y coordinate
		 */    
		virtual public float OriginY {
			get {
				return startY;
			}
		}
    
		/** Gets the maximum number of available lines. This value will change
		 * after each call to <code>go()</code>.
		 * @return Value of property maxLines.
		 */
		virtual public int MaxLines {
			get {
				return maxLines;
			}

			set {
				this.maxLines = value;
			}
		}
    
		/** Gets the height of the line
		 * @return the height
		 */
		virtual public float Height {
			get {
				return height;
			}

			set {
				this.height = value;
			}
		}
    
		/**
		 * Gets the Element.
		 * @return the alignment
		 */
		virtual public int Alignment {
			get {
				return alignment;
			}

			set {
				this.alignment = value;
			}
		}
	}
}
