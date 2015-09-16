using System;
using System.Collections.Generic;
using System.Text;
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
    /** Supports text, combo and list fields generating the correct appearances.
    * All the option in the Acrobat GUI are supported in an easy to use API.
    * @author Paulo Soares
    */
    public class TextField : BaseField {
        
        /** Holds value of property defaultText. */
        private String defaultText;
        
        /** Holds value of property choices. */
        private String[] choices;
        
        /** Holds value of property choiceExports. */
        private String[] choiceExports;
        
        /** Holds value of property choiceSelection. */
        private List<int> choiceSelections = new List<int>();
        
        private int topFirst;

        /** Represents the /TI value */
        private int visibleTopChoice = -1;

        private float extraMarginLeft;
        private float extraMarginTop;

        /** Creates a new <CODE>TextField</CODE>.
        * @param writer the document <CODE>PdfWriter</CODE>
        * @param box the field location and dimensions
        * @param fieldName the field name. If <CODE>null</CODE> only the widget keys
        * will be included in the field allowing it to be used as a kid field.
        */
        public TextField(PdfWriter writer, Rectangle box, String fieldName) : base(writer, box, fieldName) {
        }
        
        private static bool CheckRTL(String text) {
            if (text == null || text.Length == 0)
                return false;
            char[] cc = text.ToCharArray();
            for (int k = 0; k < cc.Length; ++k) {
                int c = (int)cc[k];
                if (c >= 0x590 && c < 0x0780)
                    return true;
            }
            return false;
        }
        
        private static void ChangeFontSize(Phrase p, float size) {
            foreach (Chunk ck in p) {
                ck.Font.Size = size;
            }
        }
        
        private Phrase ComposePhrase(String text, BaseFont ufont, BaseColor color, float fontSize) {
            Phrase phrase = null;
            if (extensionFont == null && (substitutionFonts == null || substitutionFonts.Count == 0))
                phrase = new Phrase(new Chunk(text, new Font(ufont, fontSize, 0, color)));
            else {
                FontSelector fs = new FontSelector();
                fs.AddFont(new Font(ufont, fontSize, 0, color));
                if (extensionFont != null)
                    fs.AddFont(new Font(extensionFont, fontSize, 0, color));
                if (substitutionFonts != null) {
                    foreach (BaseFont bf in substitutionFonts) {
                        fs.AddFont(new Font(bf, fontSize, 0, color));
                    }
                }
                phrase = fs.Process(text);
            }
            return phrase;
        }
        
        public static String RemoveCRLF(String text) {
            if (text.IndexOf('\n') >= 0 || text.IndexOf('\r') >= 0) {
                char[] p = text.ToCharArray();
                StringBuilder sb = new StringBuilder(p.Length);
                for (int k = 0; k < p.Length; ++k) {
                    char c = p[k];
                    if (c == '\n')
                        sb.Append(' ');
                    else if (c == '\r') {
                        sb.Append(' ');
                        if (k < p.Length - 1 && p[k + 1] == '\n')
                            ++k;
                    }
                    else
                        sb.Append(c);
                }
                return sb.ToString();
            }
            return text;
        }
        
        /**
        * Obfuscates a password <code>String</code>.
        * Every character is replaced by an asterisk (*).
        * 
        * @param text 
        * @return String
        * @since   2.1.5
        */
        public static String ObfuscatePassword(String text) {
            return new string('*', text.Length);
        }
        
        /**
        * Get the <code>PdfAppearance</code> of a text or combo field
        * @throws IOException on error
        * @throws DocumentException on error
        * @return A <code>PdfAppearance</code>
        */
        virtual public PdfAppearance GetAppearance() {
            PdfAppearance app = GetBorderAppearance();
            app.BeginVariableText();
            if (text == null || text.Length == 0) {
                app.EndVariableText();
                return app;
            }
            bool borderExtra = borderStyle == PdfBorderDictionary.STYLE_BEVELED || borderStyle == PdfBorderDictionary.STYLE_INSET;
            float h = box.Height - borderWidth * 2 - extraMarginTop;
            float bw2 = borderWidth;
            if (borderExtra) {
                h -= borderWidth * 2;
                bw2 *= 2;
            }
            float offsetX = Math.Max(bw2, 1);
            float offX = Math.Min(bw2, offsetX);
            app.SaveState();
            app.Rectangle(offX, offX, box.Width - 2 * offX, box.Height - 2 * offX);
            app.Clip();
            app.NewPath();
            String ptext;
            if ((options & PASSWORD) != 0)
                ptext = ObfuscatePassword(text);
            else if ((options & MULTILINE) == 0)
                ptext = RemoveCRLF(text);
            else
                ptext = text; //fixed by Kazuya Ujihara (ujihara.jp)
            BaseFont ufont = RealFont;
            BaseColor fcolor = (textColor == null) ? GrayColor.GRAYBLACK : textColor;
            int rtl = CheckRTL(ptext) ? PdfWriter.RUN_DIRECTION_LTR : PdfWriter.RUN_DIRECTION_NO_BIDI;
            float usize = fontSize;
            Phrase phrase = ComposePhrase(ptext, ufont, fcolor, usize);
            if ((options & MULTILINE) != 0) {
                float width = box.Width - 4 * offsetX - extraMarginLeft;
                float factor = ufont.GetFontDescriptor(BaseFont.BBOXURY, 1) - ufont.GetFontDescriptor(BaseFont.BBOXLLY, 1);
                ColumnText ct = new ColumnText(null);
                if (usize == 0) {
                    usize = h / factor;
                    if (usize > 4) {
                        if (usize > 12)
                            usize = 12;
                        float step = Math.Max((usize - 4) / 10, 0.2f);
                        ct.SetSimpleColumn(0, -h, width, 0);
                        ct.Alignment = alignment;
                        ct.RunDirection = rtl;
                        for (; usize > 4; usize -= step) {
                            ct.YLine = 0;
                            ChangeFontSize(phrase, usize);
                            ct.SetText(phrase);
                            ct.Leading = factor * usize;
                            int status = ct.Go(true);
                            if ((status & ColumnText.NO_MORE_COLUMN) == 0)
                                break;
                        }
                    }
                    if (usize < 4) {
                        usize = 4;
                    }
                }
                ChangeFontSize(phrase, usize);
                ct.Canvas = app;
                float leading = usize * factor;
                float offsetY = offsetX + h - ufont.GetFontDescriptor(BaseFont.BBOXURY, usize);
                ct.SetSimpleColumn(extraMarginLeft + 2 * offsetX, -20000, box.Width - 2 * offsetX, offsetY + leading);
                ct.Leading = leading;
                ct.Alignment = alignment;
                ct.RunDirection = rtl;
                ct.SetText(phrase);
                ct.Go();
            }
            else {
                if (usize == 0) {
                    float maxCalculatedSize = h / (ufont.GetFontDescriptor(BaseFont.BBOXURX, 1) - ufont.GetFontDescriptor(BaseFont.BBOXLLY, 1));
                    ChangeFontSize(phrase, 1);
                    float wd = ColumnText.GetWidth(phrase, rtl, 0);
                    if (wd == 0)
                        usize = maxCalculatedSize;
                    else
                        usize = Math.Min(maxCalculatedSize, (box.Width - extraMarginLeft - 4 * offsetX) / wd);
                    if (usize < 4)
                        usize = 4;
                }
                ChangeFontSize(phrase, usize);
                float offsetY = offX + ((box.Height - 2*offX) - ufont.GetFontDescriptor(BaseFont.ASCENT, usize)) / 2;
                if (offsetY < offX)
                    offsetY = offX;
                if (offsetY - offX < -ufont.GetFontDescriptor(BaseFont.DESCENT, usize)) {
                    float ny = -ufont.GetFontDescriptor(BaseFont.DESCENT, usize) + offX;
                    float dy = box.Height - offX - ufont.GetFontDescriptor(BaseFont.ASCENT, usize);
                    offsetY = Math.Min(ny, Math.Max(offsetY, dy));
                }
                if ((options & COMB) != 0 && maxCharacterLength > 0) {
                    int textLen = Math.Min(maxCharacterLength, ptext.Length);
                    int position = 0;
                    if (alignment == Element.ALIGN_RIGHT) {
                        position = maxCharacterLength - textLen;
                    }
                    else if (alignment == Element.ALIGN_CENTER) {
                        position = (maxCharacterLength - textLen) / 2;
                    }
                    float step = (box.Width - extraMarginLeft) / maxCharacterLength;
                    float start = step / 2 + position * step;
                    if (textColor == null)
                        app.SetGrayFill(0);
                    else
                        app.SetColorFill(textColor);
                    app.BeginText();
                    foreach (Chunk ck in phrase) {
                        BaseFont bf = ck.Font.BaseFont;
                        app.SetFontAndSize(bf, usize);
                        StringBuilder sb = ck.Append("");
                        for (int j = 0; j < sb.Length; ++j) {
                            String c = sb.ToString(j, 1);
                            float wd = bf.GetWidthPoint(c, usize);
                            app.SetTextMatrix(extraMarginLeft + start - wd / 2, offsetY - extraMarginTop);
                            app.ShowText(c);
                            start += step;
                        }
                    }
                    app.EndText();
                }
                else {
                    float x;
                    switch (alignment) {
                        case Element.ALIGN_RIGHT:
                            x = extraMarginLeft + box.Width - (2 * offsetX);
                            break;
                        case Element.ALIGN_CENTER:
                            x = extraMarginLeft + (box.Width / 2);
                            break;
                        default:
                            x = extraMarginLeft + (2 * offsetX);
                            break;
                    }
                    ColumnText.ShowTextAligned(app, alignment, phrase, x, offsetY - extraMarginTop, 0, rtl, 0);
                }
            }
            app.RestoreState();
            app.EndVariableText();
            return app;
        }

        /**
        * Get the <code>PdfAppearance</code> of a list field
        * @throws IOException on error
        * @throws DocumentException on error
        * @return A <code>PdfAppearance</code>
        */
        internal PdfAppearance GetListAppearance() {
            PdfAppearance app = GetBorderAppearance();
            if (choices == null || choices.Length == 0) {
                return app;
            }
            app.BeginVariableText();

            int topChoice = GetTopChoice();

            BaseFont ufont = RealFont;
            float usize = fontSize;
            if (usize == 0)
                usize = 12;
            bool borderExtra = borderStyle == PdfBorderDictionary.STYLE_BEVELED || borderStyle == PdfBorderDictionary.STYLE_INSET;
            float h = box.Height - borderWidth * 2;
            float offsetX = borderWidth;
            if (borderExtra) {
                h -= borderWidth * 2;
                offsetX *= 2;
            }
            float leading = ufont.GetFontDescriptor(BaseFont.BBOXURY, usize) - ufont.GetFontDescriptor(BaseFont.BBOXLLY, usize);
            int maxFit = (int)(h / leading) + 1;
            int first = 0;
            int last = 0;
            first = topChoice;
            last = first + maxFit;
            if (last > choices.Length)
                last = choices.Length;
            topFirst = first;
            app.SaveState();
            app.Rectangle(offsetX, offsetX, box.Width - 2 * offsetX, box.Height - 2 * offsetX);
            app.Clip();
            app.NewPath();
            BaseColor fcolor = (textColor == null) ? GrayColor.GRAYBLACK : textColor;
        
            // background boxes for selected value[s]
            app.SetColorFill(new BaseColor(10, 36, 106));
            for (int curVal = 0; curVal < choiceSelections.Count; ++curVal) {
                int curChoice = choiceSelections[curVal];
                // only draw selections within our display range... not strictly necessary with 
                // that clipping rect from above, but it certainly doesn't hurt either 
                if (curChoice >= first && curChoice <= last) {
                    app.Rectangle(offsetX, offsetX + h - (curChoice - first + 1) * leading, box.Width - 2 * offsetX, leading);
                    app.Fill();
                }
            }
            float xp = offsetX * 2;
            float yp = offsetX + h - ufont.GetFontDescriptor(BaseFont.BBOXURY, usize);
            for (int idx = first; idx < last; ++idx, yp -= leading) {
                String ptext = choices[idx];
                int rtl = CheckRTL(ptext) ? PdfWriter.RUN_DIRECTION_LTR : PdfWriter.RUN_DIRECTION_NO_BIDI;
                ptext = RemoveCRLF(ptext);
                // highlight selected values against their (presumably) darker background
                BaseColor textCol = choiceSelections.Contains(idx) ? GrayColor.GRAYWHITE : fcolor;
                Phrase phrase = ComposePhrase(ptext, ufont, textCol, usize);
                ColumnText.ShowTextAligned(app, Element.ALIGN_LEFT, phrase, xp, yp, 0, rtl, 0);
            }
            app.RestoreState();
            app.EndVariableText();
            return app;
        }

        /** Gets a new text field.
        * @throws IOException on error
        * @throws DocumentException on error
        * @return a new text field
        */    
        virtual public PdfFormField GetTextField() {
            if (maxCharacterLength <= 0)
                options &= ~COMB;
            if ((options & COMB) != 0)
                options &= ~MULTILINE;
            PdfFormField field = PdfFormField.CreateTextField(writer, false, false, maxCharacterLength);
            field.SetWidget(box, PdfAnnotation.HIGHLIGHT_INVERT);
            switch (alignment) {
                case Element.ALIGN_CENTER:
                    field.Quadding = PdfFormField.Q_CENTER;
                    break;
                case Element.ALIGN_RIGHT:
                    field.Quadding = PdfFormField.Q_RIGHT;
                    break;
            }
            if (rotation != 0)
                field.MKRotation = rotation;
            if (fieldName != null) {
                field.FieldName = fieldName;
                if (!"".Equals(text))
                    field.ValueAsString = text;
                if (defaultText != null)
                    field.DefaultValueAsString = defaultText;
                if ((options & READ_ONLY) != 0)
                    field.SetFieldFlags(PdfFormField.FF_READ_ONLY);
                if ((options & REQUIRED) != 0)
                    field.SetFieldFlags(PdfFormField.FF_REQUIRED);
                if ((options & MULTILINE) != 0)
                    field.SetFieldFlags(PdfFormField.FF_MULTILINE);
                if ((options & DO_NOT_SCROLL) != 0)
                    field.SetFieldFlags(PdfFormField.FF_DONOTSCROLL);
                if ((options & PASSWORD) != 0)
                    field.SetFieldFlags(PdfFormField.FF_PASSWORD);
                if ((options & FILE_SELECTION) != 0)
                    field.SetFieldFlags(PdfFormField.FF_FILESELECT);
                if ((options & DO_NOT_SPELL_CHECK) != 0)
                    field.SetFieldFlags(PdfFormField.FF_DONOTSPELLCHECK);
                if ((options & COMB) != 0)
                    field.SetFieldFlags(PdfFormField.FF_COMB);
            }
            field.BorderStyle = new PdfBorderDictionary(borderWidth, borderStyle, new PdfDashPattern(3));
            PdfAppearance tp = GetAppearance();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, tp);
            PdfAppearance da = (PdfAppearance)tp.Duplicate;
            da.SetFontAndSize(RealFont, fontSize);
            if (textColor == null)
                da.SetGrayFill(0);
            else
                da.SetColorFill(textColor);
            field.DefaultAppearanceString = da;
            if (borderColor != null)
                field.MKBorderColor = borderColor;
            if (backgroundColor != null)
                field.MKBackgroundColor = backgroundColor;
            switch (visibility) {
                case HIDDEN:
                    field.Flags = PdfAnnotation.FLAGS_PRINT | PdfAnnotation.FLAGS_HIDDEN;
                    break;
                case VISIBLE_BUT_DOES_NOT_PRINT:
                    break;
                case HIDDEN_BUT_PRINTABLE:
                    field.Flags = PdfAnnotation.FLAGS_PRINT | PdfAnnotation.FLAGS_NOVIEW;
                    break;
                default:
                    field.Flags = PdfAnnotation.FLAGS_PRINT;
                    break;
            }
            return field;
        }
        
        /** Gets a new combo field.
        * @throws IOException on error
        * @throws DocumentException on error
        * @return a new combo field
        */    
        virtual public PdfFormField GetComboField() {
            return GetChoiceField(false);
        }
        
        /** Gets a new list field.
        * @throws IOException on error
        * @throws DocumentException on error
        * @return a new list field
        */    
        virtual public PdfFormField GetListField() {
            return GetChoiceField(true);
        }

        private int GetTopChoice() {
    	    if (choiceSelections == null || choiceSelections.Count ==0) {
    		    return 0;
    	    }
        	
    	    int firstValue = choiceSelections[0];
        	
    	    int topChoice = 0;
    	    if (choices != null) {
    	        if (visibleTopChoice != -1) {
    	            return visibleTopChoice;
    	        }

    	        topChoice = firstValue;
    		    topChoice = Math.Min( topChoice, choices.Length );
    		    topChoice = Math.Max( 0, topChoice);
    	    } // else topChoice still 0
    	    return topChoice;
        }

        virtual protected PdfFormField GetChoiceField(bool isList) {
            options &= (~MULTILINE) & (~COMB);
            String[] uchoices = choices;
            if (uchoices == null)
                uchoices = new String[0];
            int topChoice = GetTopChoice();
            if (uchoices.Length > topChoice)
                text = uchoices[topChoice];

            if (text == null)
                text = "";
        
            PdfFormField field = null;
            String[,] mix = null;
            if (choiceExports == null) {
                if (isList)
                    field = PdfFormField.CreateList(writer, uchoices, topChoice);
                else
                    field = PdfFormField.CreateCombo(writer, (options & EDIT) != 0, uchoices, topChoice);
            }
            else {
                mix = new String[uchoices.Length, 2];
                for (int k = 0; k < mix.GetLength(0); ++k)
                    mix[k, 0] = mix[k, 1] = uchoices[k];
                int top = Math.Min(uchoices.Length, choiceExports.Length);
                for (int k = 0; k < top; ++k) {
                    if (choiceExports[k] != null)
                        mix[k, 0] = choiceExports[k];
                }
                if (isList)
                    field = PdfFormField.CreateList(writer, mix, topChoice);
                else
                    field = PdfFormField.CreateCombo(writer, (options & EDIT) != 0, mix, topChoice);
            }
            field.SetWidget(box, PdfAnnotation.HIGHLIGHT_INVERT);
            if (rotation != 0)
                field.MKRotation = rotation;
            if (fieldName != null) {
                field.FieldName = fieldName;
                if (uchoices.Length > 0) {
                    if (mix != null) {
                        if (choiceSelections.Count < 2) {
                            field.ValueAsString = mix[topChoice,0];
                            field.DefaultValueAsString = mix[topChoice,0];
                        } else {
                            WriteMultipleValues( field, mix);
                        }
                    } else {
                        if (choiceSelections.Count < 2) {
                            field.ValueAsString = text;
                            field.DefaultValueAsString = text;
                        } else {
                            WriteMultipleValues( field, null );
                        }
                    }
                }
                if ((options & READ_ONLY) != 0)
                    field.SetFieldFlags(PdfFormField.FF_READ_ONLY);
                if ((options & REQUIRED) != 0)
                    field.SetFieldFlags(PdfFormField.FF_REQUIRED);
                if ((options & DO_NOT_SPELL_CHECK) != 0)
                    field.SetFieldFlags(PdfFormField.FF_DONOTSPELLCHECK);
                if ((options & MULTISELECT) != 0) {
                    field.SetFieldFlags( PdfFormField.FF_MULTISELECT );
                }
            }
            field.BorderStyle = new PdfBorderDictionary(borderWidth, borderStyle, new PdfDashPattern(3));
            PdfAppearance tp;
            if (isList) {
                tp = GetListAppearance();
                if (topFirst > 0)
                    field.Put(PdfName.TI, new PdfNumber(topFirst));
            }
            else
                tp = GetAppearance();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, tp);
            PdfAppearance da = (PdfAppearance)tp.Duplicate;
            da.SetFontAndSize(RealFont, fontSize);
            if (textColor == null)
                da.SetGrayFill(0);
            else
                da.SetColorFill(textColor);
            field.DefaultAppearanceString = da;
            if (borderColor != null)
                field.MKBorderColor = borderColor;
            if (backgroundColor != null)
                field.MKBackgroundColor = backgroundColor;
            switch (visibility) {
                case HIDDEN:
                    field.Flags = PdfAnnotation.FLAGS_PRINT | PdfAnnotation.FLAGS_HIDDEN;
                    break;
                case VISIBLE_BUT_DOES_NOT_PRINT:
                    break;
                case HIDDEN_BUT_PRINTABLE:
                    field.Flags = PdfAnnotation.FLAGS_PRINT | PdfAnnotation.FLAGS_NOVIEW;
                    break;
                default:
                    field.Flags = PdfAnnotation.FLAGS_PRINT;
                    break;
            }
            return field;
        }
        
        private void WriteMultipleValues(PdfFormField field, String[,] mix) {
            PdfArray indexes = new PdfArray();
            PdfArray values = new PdfArray();
            for (int i = 0; i < choiceSelections.Count; ++i) {
                int idx = choiceSelections[i];
                indexes.Add( new PdfNumber( idx ) );
                
                if (mix != null) 
                    values.Add( new PdfString( mix[idx,0] ) );
                else if (choices != null)
                    values.Add( new PdfString( choices[ idx ] ) );
            }
            
            field.Put( PdfName.V, values );
            field.Put( PdfName.I, indexes );
        }
        
        /** Sets the default text. It is only meaningful for text fields.
        * @param defaultText the default text
        */
        virtual public string DefaultText {
            get {
                return defaultText;
            }
            set {
                defaultText = value;
            }
        }

        /** Sets the choices to be presented to the user in list/combo
        * fields.
        * @param choices the choices to be presented to the user
        */
        virtual public string[] Choices {
            get {
                return choices;
            }
            set {
                choices = value;
            }
        }

        /** Sets the export values in list/combo fields. If this array
        * is <CODE>null</CODE> then the choice values will also be used
        * as the export values.
        * @param choiceExports the export values in list/combo fields
        */
        virtual public string[] ChoiceExports {
            get {
                return choiceExports;
            }
            set {
                choiceExports = value;
            }
        }
        
        /** Sets the zero based index of the selected item.
        * @param choiceSelection the zero based index of the selected item
        */
        virtual public int ChoiceSelection {
            get {
                return GetTopChoice();
            }
            set {
                choiceSelections = new List<int>();
                choiceSelections.Add(value);
            }
        }
        
        virtual public List<int> ChoiceSelections {
            get {
                return choiceSelections;
            }
            set {
                if (value != null) {
                    choiceSelections = new List<int>(value);
                    if (choiceSelections.Count > 1 && (options & BaseField.MULTISELECT) == 0 ) {
                        // can't have multiple selections in a single-select field
                        while (choiceSelections.Count > 1) {
                            choiceSelections.RemoveAt(1);
                        }
                    }
                } else { 
                    choiceSelections.Clear();
                }
            }
        }

        /**
         * Sets the top visible choice for lists;
         *
         * @since 5.5.3
         * @param visibleTopChoice index of the first visible item (zero-based array)
         */
        /**
         * Returns the index of the top visible choice of a list. Default is -1.
         * @return the index of the top visible choice
         */
        public virtual int VisibleTopChoice {
            get { return visibleTopChoice; }
            set {
                if (value < 0) {
                    return;
                }

                if (choices != null) {
                    if (value < choices.Length) {
                        this.visibleTopChoice = value;
                    }
                }
            }
        }

        /**
        * Adds another (or a first I suppose) selection to a MULTISELECT list.
        * This doesn't do anything unless this.options & MUTLISELECT != 0 
        * @param selection new selection
        */
        virtual public void AddChoiceSelection(int selection) {
            if ((this.options & BaseField.MULTISELECT) != 0) {
                choiceSelections.Add(selection);
            }
        }
        
        internal int TopFirst {
            get {
                return topFirst;
            }
        }

        /**
        * Sets extra margins in text fields to better mimic the Acrobat layout.
        * @param extraMarginLeft the extra marging left
        * @param extraMarginTop the extra margin top
        */    
        virtual public void SetExtraMargin(float extraMarginLeft, float extraMarginTop) {
            this.extraMarginLeft = extraMarginLeft;
            this.extraMarginTop = extraMarginTop;
        }

        /**
        * Holds value of property substitutionFonts.
        */
        private List<BaseFont> substitutionFonts;

        /**
        * Sets a list of substitution fonts. The list is composed of <CODE>BaseFont</CODE> and can also be <CODE>null</CODE>. The fonts in this list will be used if the original
        * font doesn't contain the needed glyphs.
        * @param substitutionFonts the list
        */
        virtual public List<BaseFont> SubstitutionFonts {
            set {
                substitutionFonts = value;
            }
            get {
                return substitutionFonts;
            }
        }

        /**
        * Holds value of property extensionFont.
        */
        private BaseFont extensionFont;

        /**
        * Sets the extensionFont. This font will be searched before the
        * substitution fonts. It may be <code>null</code>.
        * @param extensionFont New value of property extensionFont.
        */
        virtual public BaseFont ExtensionFont {
            set {
                extensionFont = value;
            }
            get {
                return extensionFont;
            }
        }
    }
}
