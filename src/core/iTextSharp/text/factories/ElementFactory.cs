using System;
using System.Collections;
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.factories;
using iTextSharp.text.error_messages;
/*
 * $Id$
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
namespace iTextSharp.text.factories {

    /**
    * This class is able to create Element objects based on a list of properties.
    */

    public class ElementFactory {

        public static Chunk GetChunk(Properties attributes) {
            Chunk chunk = new Chunk();
            
            chunk.Font = FontFactory.GetFont(attributes);
            String value;
            
            value = attributes[ElementTags.ITEXT];
            if (value != null) {
                chunk.Append(value);
            }
            value = attributes[ElementTags.LOCALGOTO];
            if (value != null) {
                chunk.SetLocalGoto(value);
            }
            value = attributes[ElementTags.REMOTEGOTO];
            if (value != null) {
                String page = attributes[ElementTags.PAGE];
                if (page != null) {
                    chunk.SetRemoteGoto(value, int.Parse(page));
                }
                else {
                    String destination = attributes[ElementTags.DESTINATION];
                    if (destination != null) {
                        chunk.SetRemoteGoto(value, destination);
                    }
                }
            }
            value = attributes[ElementTags.LOCALDESTINATION];
            if (value != null) {
                chunk.SetLocalDestination(value);
            }
            value = attributes[ElementTags.SUBSUPSCRIPT];
            if (value != null) {
                chunk.SetTextRise(float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            value = attributes[Markup.CSS_KEY_VERTICALALIGN];
            if (value != null && value.EndsWith("%")) {
                float p = float.Parse(value.Substring(0, value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo) / 100f;
                chunk.SetTextRise(p * chunk.Font.Size);
            }
            value = attributes[ElementTags.GENERICTAG];
            if (value != null) {
                chunk.SetGenericTag(value);
            }
            value = attributes[ElementTags.BACKGROUNDCOLOR];
            if (value != null) {
                chunk.SetBackground(Markup.DecodeColor(value));
            }
            return chunk;
        }
        
        public static Phrase GetPhrase(Properties attributes) {
            Phrase phrase = new Phrase();
            phrase.Font = FontFactory.GetFont(attributes);
            String value;
            value = attributes[ElementTags.LEADING];
            if (value != null) {
                phrase.Leading = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            value = attributes[Markup.CSS_KEY_LINEHEIGHT];
            if (value != null) {
                phrase.Leading = Markup.ParseLength(value, Markup.DEFAULT_FONT_SIZE);
            }
            value = attributes[ElementTags.ITEXT];
            if (value != null) {
                Chunk chunk = new Chunk(value);
                if ((value = attributes[ElementTags.GENERICTAG]) != null) {
                    chunk.SetGenericTag(value);
                }
                phrase.Add(chunk);
            }
            return phrase;
        }
        
        public static Anchor GetAnchor(Properties attributes) {
            Anchor anchor = new Anchor(GetPhrase(attributes));
            String value;
            value = attributes[ElementTags.NAME];
            if (value != null) {
                anchor.Name = value;
            }
            value = (String)attributes.Remove(ElementTags.REFERENCE);
            if (value != null) {
                anchor.Reference = value;
            }
            return anchor;
        }
        
        public static Paragraph GetParagraph(Properties attributes) {
            Paragraph paragraph = new Paragraph(GetPhrase(attributes));
            String value;
            value = attributes[ElementTags.ALIGN];
            if (value != null) {
                paragraph.SetAlignment(value);
            }
            value = attributes[ElementTags.INDENTATIONLEFT];
            if (value != null) {
                paragraph.IndentationLeft = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            value = attributes[ElementTags.INDENTATIONRIGHT];
            if (value != null) {
                paragraph.IndentationRight = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            return paragraph;
        }
        
        public static ListItem GetListItem(Properties attributes) {
            ListItem item = new ListItem(GetParagraph(attributes));
            return item;
        }
        
        public static List GetList(Properties attributes) {
            List list = new List();

            list.Numbered = Utilities.CheckTrueOrFalse(attributes, ElementTags.NUMBERED);
            list.Lettered = Utilities.CheckTrueOrFalse(attributes, ElementTags.LETTERED);
            list.Lowercase = Utilities.CheckTrueOrFalse(attributes, ElementTags.LOWERCASE);
            list.Autoindent = Utilities.CheckTrueOrFalse(attributes, ElementTags.AUTO_INDENT_ITEMS);
            list.Alignindent = Utilities.CheckTrueOrFalse(attributes, ElementTags.ALIGN_INDENTATION_ITEMS);
            
            String value;
            
            value = attributes[ElementTags.FIRST];
            if (value != null) {
                char character = value[0];
                if (char.IsLetter(character) ) {
                    list.First = (int)character;
                }
                else {
                    list.First = int.Parse(value);
                }
            }
            
            value = attributes[ElementTags.LISTSYMBOL];
            if (value != null) {
                list.ListSymbol = new Chunk(value, FontFactory.GetFont(attributes));
            }
            
            value = attributes[ElementTags.INDENTATIONLEFT];
            if (value != null) {
                list.IndentationLeft = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            
            value = attributes[ElementTags.INDENTATIONRIGHT];
            if (value != null) {
                list.IndentationRight = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            
            value = attributes[ElementTags.SYMBOLINDENT];
            if (value != null) {
                list.SymbolIndent = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            
            return list;
        }

        public static ChapterAutoNumber GetChapter(Properties attributes) {
            ChapterAutoNumber chapter = new ChapterAutoNumber("");
            SetSectionParameters(chapter, attributes);
            return chapter;
        }
        
        public static Section GetSection(Section parent, Properties attributes) {
            Section section = parent.AddSection("");
            SetSectionParameters(section, attributes);
            return section;
        }
        
        private static void SetSectionParameters(Section section, Properties attributes) {
            String value;
            value = attributes[ElementTags.NUMBERDEPTH];
            if (value != null) {
                section.NumberDepth = int.Parse(value);
            }
            value = attributes[ElementTags.INDENT];
            if (value != null) {
                section.Indentation = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            value = attributes[ElementTags.INDENTATIONLEFT];
            if (value != null) {
                section.IndentationLeft = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            value = attributes[ElementTags.INDENTATIONRIGHT];
            if (value != null) {
                section.IndentationRight = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
        }

        /// <summary>
        /// Returns an Image that has been constructed taking in account
        /// the value of some attributes.
        /// </summary>
        /// <param name="attributes">Some attributes</param>
        /// <returns>an Image</returns>
        public static Image GetImage(Properties attributes) {
            String value;
            
            value = attributes[ElementTags.URL];
            if (value == null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.url.of.the.image.is.missing"));
            Image image = Image.GetInstance(value);
            
            value = attributes[ElementTags.ALIGN];
            int align = 0;
            if (value != null) {
                if (Util.EqualsIgnoreCase(ElementTags.ALIGN_LEFT, value))
                    align |= Image.ALIGN_LEFT;
                else if (Util.EqualsIgnoreCase(ElementTags.ALIGN_RIGHT, value))
                    align |= Image.ALIGN_RIGHT;
                else if (Util.EqualsIgnoreCase(ElementTags.ALIGN_MIDDLE, value))
                    align |= Image.ALIGN_MIDDLE;
            }
            if (Util.EqualsIgnoreCase("true", attributes[ElementTags.UNDERLYING]))
                align |= Image.UNDERLYING;
            if (Util.EqualsIgnoreCase("true", attributes[ElementTags.TEXTWRAP]))
                align |= Image.TEXTWRAP;
            image.Alignment = align;
            
            value = attributes[ElementTags.ALT];
            if (value != null) {
                image.Alt = value;
            }
            
            String x = attributes[ElementTags.ABSOLUTEX];
            String y = attributes[ElementTags.ABSOLUTEY];
            if ((x != null) && (y != null)) {
                image.SetAbsolutePosition(float.Parse(x, System.Globalization.NumberFormatInfo.InvariantInfo),
                        float.Parse(y, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            value = attributes[ElementTags.PLAINWIDTH];
            if (value != null) {
                image.ScaleAbsoluteWidth(float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            value = attributes[ElementTags.PLAINHEIGHT];
            if (value != null) {
                image.ScaleAbsoluteHeight(float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            value = attributes[ElementTags.ROTATION];
            if (value != null) {
                image.Rotation = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            return image;
        }

        /**
        * Creates an Annotation object based on a list of properties.
        * @param attributes
        * @return an Annotation
        */
        public static Annotation GetAnnotation(Properties attributes) {
            float llx = 0, lly = 0, urx = 0, ury = 0;
            String value;
            
            value = attributes[ElementTags.LLX];
            if (value != null) {
                llx = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            value = attributes[ElementTags.LLY];
            if (value != null) {
                lly = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            value = attributes[ElementTags.URX];
            if (value != null) {
                urx = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            value = attributes[ElementTags.URY];
            if (value != null) {
                ury = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            
            String title = attributes[ElementTags.TITLE];
            String text = attributes[ElementTags.CONTENT];
            if (title != null || text != null) {
                return new Annotation(title, text, llx, lly, urx, ury);
            }
            value = attributes[ElementTags.URL];
            if (value != null) {
                return new Annotation(llx, lly, urx, ury, value);
            }
            value = attributes[ElementTags.NAMED];
            if (value != null) {
                return new Annotation(llx, lly, urx, ury, int.Parse(value));
            }
            String file = attributes[ElementTags.FILE];
            String destination = attributes[ElementTags.DESTINATION];
            String page = (String) attributes.Remove(ElementTags.PAGE);
            if (file != null) {
                if (destination != null) {
                    return new Annotation(llx, lly, urx, ury, file, destination);
                }
                if (page != null) {
                    return new Annotation(llx, lly, urx, ury, file, int.Parse(page));
                }
            }
            if (title == null)
                title = "";
            if (text == null)
                text = "";
            return new Annotation(title, text, llx, lly, urx, ury);
        }
    }
}
