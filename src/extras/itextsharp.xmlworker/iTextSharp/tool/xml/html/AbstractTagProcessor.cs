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
using System.Threading;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.api;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using System.util;
using System.util.collections;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.util;

namespace iTextSharp.tool.xml.html {

    /**
     * Abstract ITagProcessor that allows setting the configuration object to a
     * protected member variable.<br />
     * Implements {@link ITagProcessor#startElement(Tag)} and
     * {@link ITagProcessor#endElement(Tag, List)} to calculate font sizes and add
     * new pages if needed.<br />
     * Extend from this class instead of implementing {@link ITagProcessor} to
     * benefit from auto fontsize metric conversion to pt and
     * page-break-before/after insertion. Override
     * {@link AbstractTagProcessor#start(Tag)} and
     * {@link AbstractTagProcessor#end(Tag, List)} in your extension.
     *
     * @author redlab_b
     *
     */

    public abstract class AbstractTagProcessor : ITagProcessor, CssAppliersAware
    {

        /**
         * The configuration object of the XMLWorker.
         */
        private FontSizeTranslator fontsizeTrans;
        private CssAppliers cssAppliers;

        /**
         *
         */

        public AbstractTagProcessor() {
            fontsizeTrans = FontSizeTranslator.GetInstance();
        }

        /**
         * Utility method that fetches the CSSResolver from the if any and if it uses the default key.
         *
         * @return CSSResolver
         * @throws NoCustomContextException if the context of the
         *             {@link CssResolverPipeline} could not be found.
         */
        public virtual ICSSResolver GetCSSResolver(IWorkerContext context) {
            return ((ObjectContext<ICSSResolver>)context.Get(typeof(CssResolverPipeline).FullName)).Get();
            //return (ICSSResolver) ((MapContext)ctx.Get(typeof(CssResolverPipeline).FullName))[CssResolverPipeline.CSS_RESOLVER];
        }

        /**
         * Utility method that fetches the HtmlPipelineContext used if any and if it
         * uses the default key.
         * @return a HtmlPipelineContext
         * @throws NoCustomContextException if the context of the
         *             {@link HtmlPipelineContext} could not be found.
         */
        public virtual HtmlPipelineContext GetHtmlPipelineContext(IWorkerContext ctx) {
            return (HtmlPipelineContext) ctx.Get(typeof(HtmlPipeline).FullName);
        }
        /**
         * Calculates any found font size to pt values and set it in the CSS before
         * calling {@link AbstractTagProcessor#start(Tag)}.<br />
         * Checks for
         * {@link com.itextpdf.tool.xml.css.CSS.Property#PAGE_BREAK_BEFORE}, if the
         * value is always a <code>Chunk.NEXTPAGE</code> added before the
         * implementors {@link AbstractTagProcessor#start(Tag)} method.
         *
         */
        public IList<IElement> StartElement(IWorkerContext ctx, Tag tag) {
            float fontSize = fontsizeTrans.TranslateFontSize(tag);
            if (fontSize != Font.UNDEFINED) {
                tag.CSS[CSS.Property.FONT_SIZE] = fontSize.ToString(CultureInfo.InvariantCulture) + "pt";
            }
            String pagebreak;
            tag.CSS.TryGetValue(CSS.Property.PAGE_BREAK_BEFORE, out pagebreak);
            if (null != pagebreak && Util.EqualsIgnoreCase(CSS.Value.ALWAYS, pagebreak)) {
                List<IElement> list = new List<IElement>(2);
                list.Add(Chunk.NEXTPAGE);
                list.AddRange(Start(ctx, tag));
                return list;
            }
            return Start(ctx, tag);
        }

        /**
         * Classes extending AbstractTagProcessor should override this method for actions that should be done in
         * {@link ITagProcessor#startElement(Tag)}. The {@link AbstractTagProcessor#startElement(Tag)} calls this method
         * after or before doing certain stuff, (see it's description).
         *
         * @param tag the tag
         * @return an element to be added to current content, may be null
         */
        public virtual IList<IElement> Start(IWorkerContext ctx, Tag tag){ return new List<IElement>(0); }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.ITagProcessor#content(com.itextpdf.tool.xml.Tag, java.lang.String)
         */
        public virtual IList<IElement> Content(IWorkerContext ctx, Tag tag, String content) {
            return new List<IElement>(0);
        }

        /**
         * For some tags, if they have their own not inherited DIR attribute, this attribute will definitely not be applied
         * for itext layout. For the most common such tags we use this set to ignore DIR attribute, in order to avoid
         * unnecessary adjustments in XmlWorker.
         *
         * However if parent of these tags have DIR attribute, it may be applied to these tags.
         */
        private HashSet2<String> ignoreDirAttribute = new HashSet2<String>(new string[]{
            HTML.Tag.P,
            HTML.Tag.SPAN
        });

        private IList<Tag> tree;
        private String GetParentDirection() {
            String result = null;
            foreach (Tag tag in tree) {
                if (!ignoreDirAttribute.Contains(tag.Name.ToLower())) {
                    tag.Attributes.TryGetValue(HTML.Attribute.DIR, out result);

                    if (result != null) break;
                    // Nested tables need this check
                    tag.CSS.TryGetValue(CSS.Property.DIRECTION, out result);
                    if (result != null) break;
                }
            }
            return result;
        }

        protected virtual int GetRunDirection(Tag tag) {
            /* CSS should get precedence, but a dir attribute defined on the tag
               itself should take precedence over an inherited style tag
            */
            String dirValue = null;
            bool toFetchRunDirFromThisTag = tag.Name != null &&
                                            !ignoreDirAttribute.Contains(tag.Name.ToLower());
            if (toFetchRunDirFromThisTag) {
                tag.Attributes.TryGetValue(HTML.Attribute.DIR, out dirValue);
            }

            if (dirValue == null) {
                if (toFetchRunDirFromThisTag) {
                    // using CSS is actually discouraged, but still supported
                    tag.CSS.TryGetValue(CSS.Property.DIRECTION, out dirValue);
                }
                if (dirValue == null) {
                    // dir attribute is inheritable in HTML but gets trumped by CSS
                    tree = new ParentTreeUtil().GetParentTagTree(tag, tree);
                    dirValue = GetParentDirection();
                }
            }

            if (Util.EqualsIgnoreCase(CSS.Value.RTL, dirValue)) {
                return PdfWriter.RUN_DIRECTION_RTL;
            }

            if (Util.EqualsIgnoreCase(CSS.Value.LTR, dirValue)) {
                return PdfWriter.RUN_DIRECTION_LTR;
            }

            if (Util.EqualsIgnoreCase(CSS.Value.AUTO, dirValue)) {
                return PdfWriter.RUN_DIRECTION_DEFAULT;
            }

            return PdfWriter.RUN_DIRECTION_NO_BIDI;
        }

        protected virtual List<IElement> TextContent(IWorkerContext ctx, Tag tag, String content) {
		    List<Chunk> sanitizedChunks = HTMLUtils.Sanitize(content, false);
		    List<IElement> l = new List<IElement>(1);
            foreach (Chunk sanitized in sanitizedChunks) {
                try {
                    l.Add(GetCssAppliers().Apply(sanitized, tag, GetHtmlPipelineContext(ctx)));
                } catch (NoCustomContextException e) {
                    throw new RuntimeWorkerException(e);
                }
            }
		    return l;
	    }

        /**
         * Checks for
         * {@link com.itextpdf.tool.xml.css.CSS.Property#PAGE_BREAK_AFTER}, if the
         * value is always a <code>Chunk.NEXTPAGE</code> is added to the
         * currentContentList after calling
         * {@link AbstractTagProcessor#end(Tag, List)}.
         */

        public IList<IElement> EndElement(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
        {
            IList<IElement> list = new List<IElement>();
            if (currentContent.Count == 0)
            {
                list = End(ctx, tag, currentContent);
            }
            else
            {
                IList<IElement> elements = new List<IElement>();
                foreach (IElement el in currentContent)
                {
                    if (el is Chunk && ((Chunk) el).HasAttributes() &&
                        ((Chunk) el).Attributes.ContainsKey(Chunk.NEWPAGE))
                    {
                        if (elements.Count > 0)
                        {
                            IList<IElement> addedElements = End(ctx, tag, elements);
                            foreach (IElement addedElement in addedElements)
                            {
                                list.Add(addedElement);
                            }
                            elements.Clear();
                        }
                        list.Add(el);
                    }
                    else
                    {
                        elements.Add(el);
                    }
                }
                if (elements.Count > 0)
                {
                    IList<IElement> addedElements = End(ctx, tag, elements);
                    foreach (IElement addedElement in addedElements)
                    {
                        list.Add(addedElement);
                    }
                    elements.Clear();
                }
            }
            String pagebreak;
            if (tag.CSS.TryGetValue(CSS.Property.PAGE_BREAK_AFTER, out pagebreak) && Util.EqualsIgnoreCase(CSS.Value.ALWAYS, pagebreak))
            {
                list.Add(Chunk.NEXTPAGE);
            }
            return list;
        }

        /**
         * Classes extending AbstractTagProcessor should override this method for
         * actions that should be done in {@link ITagProcessor#endElement(Tag, List)}.
         * The {@link AbstractTagProcessor#endElement(Tag, List)} calls this method
         * after or before doing certain stuff, (see it's description).
         *
         * @param tag the tag
         * @param currentContent the content created from e.g. inner tags, inner content and not yet added to document.
         * @return a List containing iText IElement objects
         */
        public virtual IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
            return new List<IElement>(currentContent);
        }

        /**
         * Defaults to false.
         *
         * @see com.itextpdf.tool.xml.html.ITagProcessor#isStackOwner()
         */
        public virtual bool IsStackOwner() {
            return false;
        }

        /**
         * Adds currentContent list to a paragraph element. If addNewLines is true a
         * Paragraph object is returned, else a NoNewLineParagraph object is
         * returned.
         *
         * @param currentContent IList<IElement> of the current elements to be added.
         * @param addNewLines bool to declare which paragraph element should be
         *            returned, true if new line should be added or not.
         * @param applyCSS true if CSS should be applied on the paragraph
         * @param tag the relevant tag
         * @return a List with paragraphs
         */

        public virtual IList<IElement> CurrentContentToParagraph(IList<IElement> currentContent,
            bool addNewLines, bool applyCSS, Tag tag,
            IWorkerContext ctx) {
            try {
                int direction = GetRunDirection(tag);
                IList<IElement> list = new List<IElement>();
                if (currentContent.Count > 0) {
                    if (addNewLines) {
                        Paragraph p = CreateParagraph();
                        p.MultipliedLeading = 1.2f;
                        foreach (IElement e in currentContent) {
                            if (e is LineSeparator) {
                                try {
                                    HtmlPipelineContext htmlPipelineContext = GetHtmlPipelineContext(ctx);
                                    Chunk newLine = (Chunk)GetCssAppliers().Apply(new Chunk(Chunk.NEWLINE), tag, htmlPipelineContext);
                                    p.Add(newLine);
                                }
                                catch (NoCustomContextException exc) {
                                    throw new RuntimeWorkerException(
                                        LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), exc);
                                }
                            }
                            p.Add(e);
                        }
                        if (p.Trim()) {
                            if (applyCSS) {
                                p = (Paragraph) GetCssAppliers().Apply(p, tag, GetHtmlPipelineContext(ctx));
                            }
                            if (direction.Equals(PdfWriter.RUN_DIRECTION_RTL)) {
                                DoRtlIndentCorrections(p);
                                InvertTextAlignForParagraph(p);
                            }
                            list.Add(p);
                        }
                    }
                    else {
                        NoNewLineParagraph p = new NoNewLineParagraph(float.NaN);
                        p.MultipliedLeading = 1.2f;
                        foreach (IElement e in currentContent) {
                            UpdateParagraphFontIfNeeded(p, e);
                            p.Add(e);
                        }
                        p = (NoNewLineParagraph) GetCssAppliers().Apply(p, tag, GetHtmlPipelineContext(ctx));
                        if (direction.Equals(PdfWriter.RUN_DIRECTION_RTL)) {
                            DoRtlIndentCorrections(p);
                            InvertTextAlignForParagraph(p);
                        }
                        list.Add(p);
                    }
                }
                return list;
            }
            catch (NoCustomContextException e) {
                throw new RuntimeWorkerException(
                    LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
            }
        }

        /**
         * Default apply CSS to false and tag to null.
         * @see AbstractTagProcessor#currentContentToParagraph(List, bool, bool, Tag)
         * @param currentContent IList<IElement> of the current elements to be added.
         * @param addNewLines bool to declare which paragraph element should be
         *            returned, true if new line should be added or not.
         * @return a List with paragraphs
         */

        public IList<IElement> CurrentContentToParagraph(IList<IElement> currentContent,
                                                                 bool addNewLines)
        {
            return this.CurrentContentToParagraph(currentContent, addNewLines, false, null, null);
        }

        virtual public void SetCssAppliers(CssAppliers cssAppliers)
        {
            this.cssAppliers = cssAppliers;
        }

        virtual public CssAppliers GetCssAppliers()
        {
            return cssAppliers;
        }

        virtual protected Paragraph CreateParagraph()
        {
            return new Paragraph(float.NaN);
        }

        protected void DoRtlIndentCorrections(IIndentable p) {
            float right = p.IndentationRight;
            p.IndentationRight = p.IndentationLeft;
            p.IndentationLeft = right;
        }

        protected void InvertTextAlignForParagraph(Paragraph p) {
            switch (p.Alignment) {
                case Element.ALIGN_UNDEFINED:
                case Element.ALIGN_CENTER:
                case Element.ALIGN_JUSTIFIED:
                case Element.ALIGN_JUSTIFIED_ALL:
                    break;
                case Element.ALIGN_RIGHT:
                    p.Alignment = Element.ALIGN_LEFT;
                    break;
                case Element.ALIGN_LEFT:
                default:
                    p.Alignment = Element.ALIGN_RIGHT;
                    break;
            }
        }

        protected void InvertTextAlignForParagraph(NoNewLineParagraph p) {
            switch (p.Alignment) {
                case Element.ALIGN_UNDEFINED:
                case Element.ALIGN_CENTER:
                case Element.ALIGN_JUSTIFIED:
                case Element.ALIGN_JUSTIFIED_ALL:
                    break;
                case Element.ALIGN_RIGHT:
                    p.Alignment = Element.ALIGN_LEFT;
                    break;
                case Element.ALIGN_LEFT:
                default:
                    p.Alignment = Element.ALIGN_RIGHT;
                    break;
            }
        }

        /**
         * In case child font is of bigger size than paragraph font, text overlapping may occur.
         * This happens because leading of the lines in paragraph is set based on paragraph font.
         */
        protected void UpdateParagraphFontIfNeeded(Phrase p, IElement child) {
            Font childFont = null;
            if (child is Chunk) {
                childFont = ((Chunk)child).Font;
            } else if (child is Phrase) {
                childFont = ((Phrase)child).Font;
            }
            float pFontSize = p.Font != null ? p.Font.Size : Font.DEFAULTSIZE;
            if (childFont != null && childFont.Size > pFontSize) {
                //Create a copy with size only.
                p.Font = new Font(Font.FontFamily.UNDEFINED, childFont.Size);
            }
        }
    }

}
