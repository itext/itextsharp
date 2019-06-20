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
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.pipeline.html;
namespace iTextSharp.tool.xml.html {

    /**
     * @author Emiel Ackermann
     *
     */
    public class OrderedUnorderedList : AbstractTagProcessor {

        /**
         *
         */
        private static FontSizeTranslator fst = FontSizeTranslator.GetInstance();
        /**
         *
         */
        private static CssUtils utils = CssUtils.GetInstance();
        private static ILogger LOG = LoggerFactory.GetLogger(typeof(OrderedUnorderedList));

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.ITagProcessor#endElement(com.itextpdf.tool.xml.Tag,
         * java.util.List)
         */
        public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
            IList<IElement> listElements = PopulateList(currentContent);
            int size = listElements.Count;
            IList<IElement> returnedList = new List<IElement>();
            if (size > 0) {
                HtmlPipelineContext htmlPipelineContext = null;
                List list;
                try {
                    htmlPipelineContext = GetHtmlPipelineContext(ctx);
                    list = (List) GetCssAppliers().Apply(new List(), tag, htmlPipelineContext);
                } catch (NoCustomContextException) {
                    list = (List) GetCssAppliers().Apply(new List(), tag, null);
                }
                int i = 0;
                foreach (IElement li in listElements) {
                    if (li is ListItem) {
                        Tag child = tag.Children[i];
                        if (size == 1) {
                            child.CSS[CSS.Property.MARGIN_TOP] =
                                        CalculateTopOrBottomSpacing(true, false, tag, child, ctx).ToString(CultureInfo.InvariantCulture) + "pt";
                            float marginBottom = CalculateTopOrBottomSpacing(false, false, tag, child, ctx);
                            child.CSS[CSS.Property.MARGIN_BOTTOM] = marginBottom.ToString(CultureInfo.InvariantCulture) + "pt";
                        } else {
                            if (i == 0) {
                                child.CSS[CSS.Property.MARGIN_TOP] =
                                            CalculateTopOrBottomSpacing(true, false, tag, child, ctx).ToString(CultureInfo.InvariantCulture) + "pt";
                            }
                            if (i == size - 1) {
                                float marginBottom = CalculateTopOrBottomSpacing(false, true, tag, child, ctx);
                                child.CSS[CSS.Property.MARGIN_BOTTOM] = marginBottom.ToString(CultureInfo.InvariantCulture) + "pt";
                            }
                        }
                        try {
                            list.Add(GetCssAppliers().Apply(li, child, GetHtmlPipelineContext(ctx)));
                        } catch (NoCustomContextException e1) {
                            throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e1);
                        }
                    } else {
                        list.Add(li);
                    }
                    i++;
                }
                returnedList.Add(list);
            }
            return returnedList;
        }
        /**
         * Fills a java.util.List with all elements found in currentContent. Places elements that are not a {@link ListItem} or {@link com.itextpdf.text.List} in a new ListItem object.
         *
         * @param currentContent
         * @return java.util.List with only {@link ListItem}s or {@link com.itextpdf.text.List}s in it.
         */
        private IList<IElement> PopulateList(IList<IElement> currentContent) {
            IList<IElement> listElements = new List<IElement>();
            foreach (IElement e in currentContent) {
                if (e is ListItem || e is List) {
                    listElements.Add(e);
                } else {
                    ListItem listItem = new ListItem();
                    listItem.Add(e);
                    listElements.Add(listItem);
                }
            }
            return listElements;
        }
        /**
         * Calculates top or bottom spacing of the list. In HTML following possibilities exist:
         * <ul>
         * <li><b>padding-top of the ul/ol tag == 0.</b><br />
         * The margin-top values of the ul/ol tag and its <b>first</b> li tag are <b>compared</b>. The total spacing before is the largest margin value and the first li's padding-top.</li>
         * <li><b>padding-top of the ul/ol tag != 0.</b><br />
         * The margin-top or bottom values of the ul/ol tag and its first li tag are <b>accumulated</b>, along with padding-top values of both tags.</li>
         * <li><b>padding-bottom of the ul/ol tag == 0.</b><br />
         * The margin-bottom values of the ul/ol tag and its <b>last</b> li tag are <b>compared</b>. The total spacing after is the largest margin value and the first li's padding-bottom.</li>
         * <li><b>padding-bottom of the ul/ol tag != 0.</b><br />
         * The margin-bottom or bottom values of the ul/ol tag and its last li tag are <b>accumulated</b>, along with padding-bottom values of both tags.</li>
         * </ul>
         * @param isTop bool, if true the top spacing is calculated, if false the bottom spacing is calculated.
         * @param storeMarginBottom if true the calculated margin bottom value is stored for later comparison with the top margin value of the next tag.
         * @param tag the ul/ol tag.
         * @param child first or last li tag of this list.
         * @return float containing the spacing before or after.
         */
        private float CalculateTopOrBottomSpacing(bool isTop, bool storeMarginBottom, Tag tag, Tag child, IWorkerContext ctx) {
            float totalSpacing = 0;
            try {
                HtmlPipelineContext context = GetHtmlPipelineContext(ctx);
                String end = isTop?"-top":"-bottom";
                float ownFontSize = fst.GetFontSize(tag);
                if (ownFontSize == Font.UNDEFINED)
                    ownFontSize = 0;
                float ownMargin = 0;
                String marginValue;
                tag.CSS.TryGetValue(CSS.Property.MARGIN+end, out marginValue);
                if (marginValue==null) {
                    if (null != tag.Parent && GetHtmlPipelineContext(ctx).GetRootTags().Contains(tag.Parent.Name)) {
                        ownMargin = ownFontSize;
                    }
                } else {
                    ownMargin = utils.ParseValueToPt(marginValue,ownFontSize);
                }
                float ownPadding = 0;
                if (tag.CSS.ContainsKey(CSS.Property.PADDING+end))
                   ownPadding = utils.ParseValueToPt(tag.CSS[CSS.Property.PADDING+end],ownFontSize);
                float childFontSize = fst.GetFontSize(child);
                float childMargin = 0;
                if (child.CSS.ContainsKey(CSS.Property.MARGIN+end))
                    childMargin = utils.ParseValueToPt(child.CSS[CSS.Property.MARGIN+end],childFontSize);
                //Margin values of this tag and its first child need to be compared if paddingTop or bottom = 0.
                if (ownPadding == 0) {
                    float margin = 0;
                    if (ownMargin != 0 && childMargin != 0){
                        margin = ownMargin>=childMargin?ownMargin:childMargin;
                    } else if (ownMargin != 0) {
                        margin = ownMargin;
                    } else if (childMargin != 0) {
                        margin = childMargin;
                    }
                    if (!isTop && storeMarginBottom){
                        context.GetMemory()[HtmlPipelineContext.LAST_MARGIN_BOTTOM] = margin;
                    }
                    totalSpacing = margin;
                } else { // ownpadding != 0 and all margins and paddings need to be accumulated.
                    totalSpacing = ownMargin+ownPadding+childMargin;
                    if (!isTop && storeMarginBottom){
                        context.GetMemory()[HtmlPipelineContext.LAST_MARGIN_BOTTOM] = ownMargin;
                    }
                }
            } catch (NoCustomContextException e) {
                throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
            }
            return totalSpacing;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.ITagProcessor#isStackOwner()
         */
        public override bool IsStackOwner() {
            return true;
        }
    }
}
