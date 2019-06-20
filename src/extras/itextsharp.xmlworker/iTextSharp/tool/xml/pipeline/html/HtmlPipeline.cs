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
using System.Text;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline;
namespace iTextSharp.tool.xml.pipeline.html {

    /**
     * The HtmlPipeline transforms received tags and content to PDF Elements.<br />
     * To configure this pipeline a {@link HtmlPipelineContext}.
     * @author redlab_b
     *
     */
    public class HtmlPipeline : AbstractPipeline {

        private HtmlPipelineContext hpc;

        /**
         * @param hpc the initial {@link HtmlPipelineContext}
         * @param next the next pipe in row
         */
        public HtmlPipeline(HtmlPipelineContext hpc, IPipeline next) : base(next) {
            this.hpc = hpc;
        }

        public override IPipeline Init(IWorkerContext context) {
            HtmlPipelineContext clone = (HtmlPipelineContext)hpc.Clone();
            context.Put(GetContextKey(), clone);
            return GetNext();

        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.pipeline.IPipeline#open(com.itextpdf.tool.
         * xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Open(IWorkerContext context, Tag t, ProcessObject po) {
            HtmlPipelineContext hcc = (HtmlPipelineContext)GetLocalContext(context);
            try {
                Object lastMarginBottom = null;
                if (hcc.GetMemory().TryGetValue(HtmlPipelineContext.LAST_MARGIN_BOTTOM, out lastMarginBottom)) {
                    t.LastMarginBottom = lastMarginBottom;
                    hcc.GetMemory().Remove(HtmlPipelineContext.LAST_MARGIN_BOTTOM);
                }
                ITagProcessor tp = hcc.ResolveProcessor(t.Name, t.NameSpace);
                AddStackKeeper(t, hcc, tp);
                IList<IElement> content = tp.StartElement(context, t);
                if (content.Count > 0) {
                    if (tp.IsStackOwner()) {
                        StackKeeper peek = hcc.Peek();
                        if (peek == null)
                            throw new PipelineException(String.Format(LocaleMessages.STACK_404, t.ToString()));
                        foreach (IElement elem in content) {
                            peek.Add(elem);
                        }
                    } else {
                        foreach (IElement elem in content) {
                            hcc.CurrentContent().Add(elem);
                            if (elem.Type == Element.BODY)
                            {
                                WritableElement writableElement = new WritableElement();
                                writableElement.Add(elem);
                                po.Add(writableElement);
                                hcc.CurrentContent().Remove(elem);
                            }
                        }
                    }
                }
            } catch (NoTagProcessorException e) {
                if (!hcc.AcceptUnknown()) {
                    throw e;
                }
            }
            return GetNext();
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.pipeline.IPipeline#content(com.itextpdf.tool
         * .xml.Tag, java.lang.String, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Content(IWorkerContext context, Tag t, string text, ProcessObject po) {
            HtmlPipelineContext hcc = (HtmlPipelineContext)GetLocalContext(context);
            ITagProcessor tp;
            try {
                tp = hcc.ResolveProcessor(t.Name, t.NameSpace);
                //String ctn = null;
                //if (null != hcc.CharSet()) {
                //    ctn = hcc.CharSet().GetString(b);
                //} else {
                //    ctn = Encoding.Default.GetString(b);
                //}
                IList<IElement> elems = tp.Content(context, t, text);
                if (elems.Count > 0)
                {
                    StackKeeper peek = hcc.Peek();
                    if (peek != null)
                    {
                        foreach (IElement e in elems)
                        {
                            peek.Add(e);
                        }
                    }
                    else
                    {
                        WritableElement writableElement = new WritableElement();
                        foreach (IElement elem in elems)
                        {
                            writableElement.Add(elem);
                        }
                        po.Add(writableElement);
                    }
                }
              } catch (NoTagProcessorException e) {
                if (!hcc.AcceptUnknown()) {
                    throw e;
                }
            }
            return GetNext();
        }

        /**
         * Defaults to the fully qualified class name of the object.
         */
        public override String GetContextKey()
        {
            return typeof(HtmlPipeline).FullName;
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.pipeline.IPipeline#close(com.itextpdf.tool
         * .xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Close(IWorkerContext context, Tag t, ProcessObject po) {
            HtmlPipelineContext hcc = (HtmlPipelineContext)GetLocalContext(context);
            ITagProcessor tp;
            try {
                if (t.LastMarginBottom != null) {
                    hcc.GetMemory()[HtmlPipelineContext.LAST_MARGIN_BOTTOM] = t.LastMarginBottom;
                } else {
                    hcc.GetMemory().Remove(HtmlPipelineContext.LAST_MARGIN_BOTTOM);
                }
                tp = hcc.ResolveProcessor(t.Name, t.NameSpace);
                IList<IElement> elems = null;
                if (tp.IsStackOwner()) {
                    // remove the element from the StackKeeper Queue if end tag is
                    // found
                    StackKeeper tagStack;
                    try {
                        tagStack = hcc.Poll();
                    } catch (NoStackException e) {
                        throw new PipelineException(String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.STACK_404), t.ToString()), e);
                    }
                    elems = tp.EndElement(context, t,  tagStack.GetElements());
                } else {
                    elems = tp.EndElement(context, t, hcc.CurrentContent());
                    hcc.CurrentContent().Clear();
                }
                if (elems.Count > 0) {
                    StackKeeper stack = hcc.Peek();

                    if(stack !=null){
                        foreach (IElement elem in elems) {
                            stack.Add(elem);
                        }
                    } else {
                        WritableElement writableElement = new WritableElement();
                            po.Add(writableElement);
                            writableElement.AddAll(elems);
                    }

                }
            } catch (NoTagProcessorException e) {
                if (!hcc.AcceptUnknown()) {
                    throw e;
                }
            }
            return GetNext();
        }

        protected virtual void AddStackKeeper(Tag t, HtmlPipelineContext hcc, ITagProcessor tp)
        {
            if (tp.IsStackOwner())
                hcc.AddFirst(new StackKeeper(t));
        }
    }
}
