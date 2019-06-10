/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
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
using System.IO;
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.net;
using iTextSharp.tool.xml.net.exc;
using iTextSharp.tool.xml.pipeline.html;
using Image = iTextSharp.text.Image;

namespace iTextSharp.tool.xml.css.apply {

    public class DivCssApplier : CssApplier<PdfDiv> {
        private CssUtils utils = CssUtils.GetInstance();
        private static ILogger LOG = LoggerFactory.GetLogger(typeof(DivCssApplier));

        public override PdfDiv Apply(PdfDiv div, Tag t, IMarginMemory memory, IPageSizeContainable psc, HtmlPipelineContext context) {
            IDictionary<String, String> css = t.CSS;
            float fontSize = FontSizeTranslator.GetInstance().TranslateFontSize(t);
            if (fontSize == Font.UNDEFINED) {
                fontSize =  FontSizeTranslator.DEFAULT_FONT_SIZE;
            }
            String align = null;
            if (t.Attributes.ContainsKey(HTML.Attribute.ALIGN)) {
                align = t.Attributes[HTML.Attribute.ALIGN];
            } else if (css.ContainsKey(CSS.Property.TEXT_ALIGN)) {
                align = css[CSS.Property.TEXT_ALIGN];
            }

            if (align != null) {
                div.TextAlignment = CSS.GetElementAlignment(align);
            }


            String widthValue;
            if (!css.TryGetValue(HTML.Attribute.WIDTH, out widthValue)) {
                t.Attributes.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
            }
            if (widthValue != null) {
                float pageWidth = psc.PageSize.Width;
                if (utils.IsNumericValue(widthValue) || utils.IsMetricValue(widthValue)) {
				    div.Width = Math.Min(pageWidth, utils.ParsePxInCmMmPcToPt(widthValue));
                } else if (utils.IsRelativeValue(widthValue)) {
                    if (widthValue.Contains(CSS.Value.PERCENTAGE)) {
                        div.PercentageWidth = utils.ParseRelativeValue(widthValue, 1f);
                    } else {
                        div.Width = Math.Min(pageWidth, utils.ParseRelativeValue(widthValue, fontSize));
                    }
                }
            }

            String heightValue;
            if (!css.TryGetValue(HTML.Attribute.HEIGHT, out heightValue)) {
                t.Attributes.TryGetValue(HTML.Attribute.HEIGHT, out heightValue);
            }
            if (heightValue != null) {
                if (utils.IsNumericValue(heightValue) || utils.IsMetricValue(heightValue)) {
                    div.Height = utils.ParsePxInCmMmPcToPt(heightValue);
                } else if (utils.IsRelativeValue(heightValue)) {
                    if (heightValue.Contains(CSS.Value.PERCENTAGE)) {
                        div.PercentageHeight = utils.ParseRelativeValue(heightValue, 1f);
                    } else {
                        div.Height = utils.ParseRelativeValue(heightValue, fontSize);
                    }
                }
            }

            float? marginTop = null;
            float? marginBottom = null;

            foreach (KeyValuePair<String, String> entry in css) {
                String key = entry.Key;
			    String value = entry.Value;
                if (Util.EqualsIgnoreCase(key, CSS.Property.LEFT)) {
                    div.Left = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.RIGHT)) {
                    if (div.Width == null || div.Left == null) {
                        div.Right = utils.ParseValueToPt(value, fontSize);
                    }
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.TOP)) {
                    div.Top = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.BOTTOM)) {
                    if (div.Height == null || div.Top == null) {
                        div.Bottom = utils.ParseValueToPt(value, fontSize);
                    }
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.BACKGROUND_COLOR)) {
				    div.BackgroundColor = HtmlUtilities.DecodeColor(value);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.BACKGROUND_IMAGE)) {
                    string url = utils.ExtractUrl(value);
                    try {
                        Image img =
                            new ImageRetrieve(context.ResourcePath, context.GetImageProvider()).RetrieveImage(url);
                        div.BackgroundImage = img;
                    }
                    catch (NoImageException e) {
                        if (LOG.IsLogging(Level.ERROR)) {
                            LOG.Error(string.Format(LocaleMessages.GetInstance().GetMessage("html.tag.img.failed"), url), e);
                        }
                    }
                }
                else if (Util.EqualsIgnoreCase(key, CSS.Property.PADDING_LEFT)) {
                    div.PaddingLeft = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.PADDING_RIGHT)) {
                    div.PaddingRight = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.PADDING_TOP)) {
                    div.PaddingTop = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.PADDING_BOTTOM)) {
                    div.PaddingBottom = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.MARGIN_TOP)) {
                    marginTop = utils.CalculateMarginTop(value, fontSize, memory);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.MARGIN_BOTTOM)) {
                    marginBottom = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.FLOAT)) {
                    if (Util.EqualsIgnoreCase(value, CSS.Value.LEFT)) {
                        div.Float = PdfDiv.FloatType.LEFT;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.RIGHT)) {
                        div.Float = PdfDiv.FloatType.RIGHT;
                    }
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.POSITION)) {
                    if (Util.EqualsIgnoreCase(value, CSS.Value.ABSOLUTE)) {
                        div.Position = PdfDiv.PositionType.ABSOLUTE;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.FIXED)) {
                        div.Position = PdfDiv.PositionType.FIXED;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.RELATIVE)) {
                        div.Position = PdfDiv.PositionType.RELATIVE;
                    }
                }
                else if (Util.EqualsIgnoreCase(key, CSS.Property.DISPLAY)) {
                    if (Util.EqualsIgnoreCase(value, CSS.Value.BLOCK)) {
                        div.Display = PdfDiv.DisplayType.BLOCK;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.INLINE)) {
                        div.Display = PdfDiv.DisplayType.INLINE;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.INLINE_BLOCK)) {
                        div.Display = PdfDiv.DisplayType.INLINE_BLOCK;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.INLINE_TABLE)) {
                        div.Display = PdfDiv.DisplayType.INLINE_TABLE;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.LIST_ITEM)) {
                        div.Display = PdfDiv.DisplayType.LIST_ITEM;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.NONE)) {
                        div.Display = PdfDiv.DisplayType.NONE;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.RUN_IN)) {
                        div.Display = PdfDiv.DisplayType.RUN_IN;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE)) {
                        div.Display = PdfDiv.DisplayType.TABLE;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_CAPTION)) {
                        div.Display = PdfDiv.DisplayType.TABLE_CAPTION;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_CELL)) {
                        div.Display = PdfDiv.DisplayType.TABLE_CELL;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_COLUMN_GROUP)) {
                        div.Display = PdfDiv.DisplayType.TABLE_COLUMN_GROUP;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_COLUMN)) {
                        div.Display = PdfDiv.DisplayType.TABLE_COLUMN;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_FOOTER_GROUP)) {
                        div.Display = PdfDiv.DisplayType.TABLE_FOOTER_GROUP;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_HEADER_GROUP)) {
                        div.Display = PdfDiv.DisplayType.TABLE_HEADER_GROUP;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_ROW)) {
                        div.Display = PdfDiv.DisplayType.TABLE_ROW;
                    } else if (Util.EqualsIgnoreCase(value, CSS.Value.TABLE_ROW_GROUP)) {
                        div.Display = PdfDiv.DisplayType.TABLE_ROW_GROUP;
                    }
                }
                else if (Util.EqualsIgnoreCase(CSS.Property.BORDER_TOP_STYLE, key))
                {
                    if (Util.EqualsIgnoreCase(CSS.Value.DOTTED, value))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.DOTTED;
                    }
                    else if (Util.EqualsIgnoreCase(CSS.Value.DASHED, value))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.DASHED;
                    }
                    else if (Util.EqualsIgnoreCase(CSS.Value.SOLID, value))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.SOLID;
                    }
                    else if (Util.EqualsIgnoreCase(CSS.Value.DOUBLE, value))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.DOUBLE;
                    }
                    else if (Util.EqualsIgnoreCase(CSS.Value.GROOVE, value))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.GROOVE;
                    }
                    else if (Util.EqualsIgnoreCase(CSS.Value.RIDGE, value))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.RIDGE;
                    }
                    else if (Util.EqualsIgnoreCase(value, CSS.Value.INSET))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.INSET;
                    }
                    else if (Util.EqualsIgnoreCase(value, CSS.Value.OUTSET))
                    {
                        div.BorderStyle = PdfDiv.BorderTopStyle.OUTSET;
                    }

                } else if (Util.EqualsIgnoreCase(key, CSS.Property.PAGE_BREAK_INSIDE)) {
                    if (Util.EqualsIgnoreCase(value, CSS.Value.AVOID)) {
                        div.KeepTogether = true;
                    }
                } 

                //TODO: border, background properties.
            }

            return div;
        }
    }
}
