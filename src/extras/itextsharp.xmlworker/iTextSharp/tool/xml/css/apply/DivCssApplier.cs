using System;
using System.Collections.Generic;
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.html;

namespace iTextSharp.tool.xml.css.apply {

    public class DivCssApplier {
        private CssUtils utils = CssUtils.GetInstance();

        public PdfDiv apply(PdfDiv div, Tag t, IMarginMemory memory, IPageSizeContainable psc) {
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
                if (Util.EqualsIgnoreCase(align,CSS.Value.CENTER)) {
                    div.TextAlignment = Element.ALIGN_CENTER;
                } else if (Util.EqualsIgnoreCase(align, CSS.Value.RIGHT)) {
                    div.TextAlignment = Element.ALIGN_RIGHT;
                } else if (Util.EqualsIgnoreCase(align, CSS.Value.JUSTIFY)) {
                    div.TextAlignment = Element.ALIGN_JUSTIFIED;
                }
            }


            String widthValue;
            if (!t.CSS.TryGetValue(HTML.Attribute.WIDTH, out widthValue)) {
                t.Attributes.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
            }
            if (widthValue != null) {
                if (utils.IsNumericValue(widthValue) || utils.IsMetricValue(widthValue)) {
				    div.Width = utils.ParsePxInCmMmPcToPt(widthValue);
                } else if (utils.IsRelativeValue(widthValue)) {
                    if (widthValue.Contains(CSS.Value.PERCENTAGE)) {
                        div.PercentageWidth = utils.ParseRelativeValue(widthValue, 1f);
                    } else {
                        div.Width = utils.ParseRelativeValue(widthValue, fontSize);
                    }
                }
            }

            String heightValue;
            if (!t.CSS.TryGetValue(HTML.Attribute.HEIGHT, out heightValue)) {
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
                } else if (Util.EqualsIgnoreCase(key, CSS.Property.PADDING_LEFT)) {
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

                //TODO: border, background properties.
            }

            return div;
        }
    }
}