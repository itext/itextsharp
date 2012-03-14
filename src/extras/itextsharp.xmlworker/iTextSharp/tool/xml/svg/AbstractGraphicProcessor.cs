using System;
using System.Collections.Generic;

using iTextSharp.text;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.ctx;
/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: VVB, Bruno Lowagie, et al.
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
namespace iTextSharp.tool.xml.svg {

    public class AbstractGraphicProcessor : ITagProcessor
    {

        /**
         * Utility method that fetches the CSSResolver from the if any and if it uses the default key.
         * @param context the WorkerContext
         *
         * @return CSSResolver
         * @throws NoCustomContextException if the context of the
         *             {@link CssResolverPipeline} could not be found.
         */
        //@SuppressWarnings("unchecked")
        public ICSSResolver GetCSSResolver(IWorkerContext context)
        {
            return ((ObjectContext<ICSSResolver>)context.Get(typeof(CssResolverPipeline).FullName)).Get();
        }

        /**
         * Utility method that fetches the HtmlPipelineContext used if any and if it
         * uses the default key.
         * @param context the WorkerContext
         * @return a HtmlPipelineContext
         * @throws NoCustomContextException if the context of the
         *             {@link HtmlPipelineContext} could not be found.
         */
        public SvgPipelineContext GetSvgPipelineContext(IWorkerContext context)
        {
            return ((SvgPipelineContext)context.Get(typeof(SvgPipeline).FullName));
        }
        /**
         * Calculates any found font size to pt values and set it in the CSS before
         * calling {@link AbstractTagProcessor#start(WorkerContext, Tag)}.<br />
         * Checks for
         * {@link com.itextpdf.tool.xml.css.CSS.Property#PAGE_BREAK_BEFORE}, if the
         * value is always a <code>Chunk.NEXTPAGE</code> added before the
         * implementors {@link AbstractTagProcessor#start(WorkerContext, Tag)} method.
         *
         */
        public IList<IElement> StartElement(IWorkerContext ctx, Tag tag)
        {
            return Start(ctx, tag);
        }

        /**
         * Classes extending AbstractTagProcessor should override this method for actions that should be done in
         * {@link TagProcessor#startElement(WorkerContext, Tag)}. The {@link AbstractTagProcessor#startElement(WorkerContext, Tag)} calls this method
         * after or before doing certain stuff, (see it's description).
         * @param ctx the WorkerContext
         * @param tag the tag
         *
         * @return an element to be added to current content, may be null
         */
        public virtual IList<IElement> Start(IWorkerContext ctx, Tag tag) { return new List<IElement>(0); }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.TagProcessor#content(com.itextpdf.tool.xml.Tag, java.lang.String)
         */
        public virtual IList<IElement> Content(IWorkerContext ctx, Tag tag, String content)
        {
            return new List<IElement>(0);
        }

        public void AddElementsToMemoryWithId(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
        {
            //add the list of elements to the memory, for later use (see useTag)
            try
            {
                IDictionary<String, String> attributes = tag.Attributes;
                if (attributes != null)
                {
                    //TODO think : what if the attribute doesn't exist or the content is the same as a previous one
                    String id;
                    if (attributes.TryGetValue("id", out id) && id != null)
                    {
                        SvgPipelineContext context = GetSvgPipelineContext(ctx);
                        context.AddSymbolById(id, currentContent);
                    }
                }
            }
            catch (NoCustomContextException e)
            {
                throw new RuntimeWorkerException(e);
            }
        }

        /**
         * Checks for
         * {@link com.itextpdf.tool.xml.css.CSS.Property#PAGE_BREAK_AFTER}, if the
         * value is always a <code>Chunk.NEXTPAGE</code> is added to the
         * currentContentList after calling
         * {@link AbstractTagProcessor#end(WorkerContext, Tag, List)}.
         */
        public virtual IList<IElement> EndElement(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
        {
            //convert the tags into graphical elements
            IList<IElement> list = End(ctx, tag, currentContent);

            if (IsElementWithId())
            {
                AddElementsToMemoryWithId(ctx, tag, list);
            }

            return list;
        }

        /**
         * Classes extending AbstractTagProcessor should override this method for
         * actions that should be done in {@link TagProcessor#endElement(WorkerContext, Tag, List)}.
         * The {@link AbstractTagProcessor#endElement(WorkerContext, Tag, List)} calls this method
         * after or before doing certain stuff, (see it's description).
         * @param ctx the WorkerContext
         * @param tag the tag
         * @param currentContent the content created from e.g. inner tags, inner content and not yet added to document.
         *
         * @return a List containing iText Element objects
         */
        public virtual IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
        {
            return new List<IElement>(currentContent);
        }

        /**
         * Defaults to false.
         *
         * @see com.itextpdf.tool.xml.html.TagProcessor#isStackOwner()
         */
        public virtual bool IsStackOwner()
        {
            return false;
        }

        public virtual bool IsElementWithId()
        {
            return false;
        }
    }
}
