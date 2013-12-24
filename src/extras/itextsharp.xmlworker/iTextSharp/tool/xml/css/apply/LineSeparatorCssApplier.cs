using System;
using System.Collections.Generic;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.html;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.pipeline.html;
/**
 *
 */
namespace iTextSharp.tool.xml.css.apply {

    /**
     * @author Emiel Ackermann
     *
     */
    public class LineSeparatorCssApplier {

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.css.CssApplier#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag)
         */
        virtual public LineSeparator Apply(LineSeparator ls, Tag t, IPageSizeContainable psc) {
            float lineWidth = 1;
            IDictionary<String, String> css = t.CSS;
            if (css.ContainsKey(CSS.Property.HEIGHT)) {
                lineWidth = CssUtils.GetInstance().ParsePxInCmMmPcToPt(css[CSS.Property.HEIGHT]);
            }
            ls.LineWidth = lineWidth;
            BaseColor lineColor = BaseColor.BLACK;
            if (css.ContainsKey(CSS.Property.COLOR)) {
                lineColor  = HtmlUtilities.DecodeColor(css[CSS.Property.COLOR]);
            } else if (css.ContainsKey(CSS.Property.BACKGROUND_COLOR)) {
                lineColor = HtmlUtilities.DecodeColor(css[CSS.Property.BACKGROUND_COLOR]);
            }
            ls.LineColor = lineColor;
            float percentage = 100;
            String widthStr;
            css.TryGetValue(CSS.Property.WIDTH, out widthStr);
            if (widthStr != null) {
                if (widthStr.Contains("%")) {
                    percentage = float.Parse(widthStr.Replace("%", ""), CultureInfo.InvariantCulture);
                } else {
                    percentage = (CssUtils.GetInstance().ParsePxInCmMmPcToPt(widthStr)/psc.PageSize.Width)*100;
                }
            }
            ls.Percentage = percentage;
            ls.Offset = 9;
            return ls;
        }
    }
}
