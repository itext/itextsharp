/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.ctx;
namespace iTextSharp.tool.xml {

    /**
     * The implementation of the XMLWorker. For legacy purposes this class also
     * : {@link SimpleXMLDocHandler}
     *
     * @author redlab_b
     *
     */
    public class XMLWorker : IXMLParserListener {

        protected IPipeline rootpPipe;
        private static LocalDataStoreSlot context = Thread.AllocateDataSlot();
        protected bool parseHtml;

        /**
         * Constructs a new XMLWorker
         *
         * @param pipeline the pipeline
         * @param parseHtml true if this XMLWorker is parsing HTML, this actually
         *            just means: convert all tags to lowercase.
         */
        public XMLWorker(IPipeline pipeline, bool parseHtml) {
            this.parseHtml = parseHtml;
            rootpPipe = pipeline;
        }

        virtual public void Init()  {
            IPipeline p = rootpPipe;
            try {
                while ((p = p.Init(GetLocalWC()))!= null);
            } catch (PipelineException e) {
                throw new RuntimeWorkerException(e);
            }
        }

        public virtual void StartElement(String tag, IDictionary<String, String> attr, String ns) {
            Tag t = CreateTag(tag, attr, ns);
            WorkerContextImpl ctx = (WorkerContextImpl)GetLocalWC();
            if (null != ctx.GetCurrentTag()) {
                ctx.GetCurrentTag().AddChild(t);
            }
            ctx.SetCurrentTag(t);
            IPipeline wp = rootpPipe;
            ProcessObject po = new ProcessObject();
            try {
                while (null != (wp = wp.Open(ctx, t, po)))
                    ;
            } catch (PipelineException e) {
                throw new RuntimeWorkerException(e);
            }

        }

        /**
         * Creates a new Tag object from the given parameters.
         * @param tag the tag name
         * @param attr the attributes
         * @param ns the namespace if any
         * @return a Tag
         */
        virtual protected Tag CreateTag(String tag, IDictionary<String, String> attr, String ns) {
            if (parseHtml) {
                tag = tag.ToLowerInvariant();
            }
            Tag t = new Tag(tag, attr, ns);
            return t;
        }

        /**
         * Called when an ending tag is encountered by the {@link SimpleXMLParser}.
         * This method searches for the tags {@link ITagProcessor} in the given
         * {@link TagProcessorFactory}. If none found and acceptUknown is false a
         * {@link NoTagProcessorException} is thrown. If found the TagProcessors
         * endElement is called.<br />
         * The returned IElement by the ITagProcessor is added to the currentContent
         * stack.<br />
         * If any of the parent tags or the given tags
         * {@link ITagProcessor#isStackOwner()} is true. The returned IElement is put
         * on the respective stack.Else it element is added to the document or the
         * elementList.
         *
         */
        public virtual void EndElement(String tag, String ns) {
            String thetag = null;
            if (parseHtml) {
                thetag = tag.ToLowerInvariant();
            } else {
                thetag = tag;
            }
            IWorkerContext ctx = GetLocalWC();
            if (null != ctx.GetCurrentTag() && !thetag.Equals(ctx.GetCurrentTag().Name)) {
                throw new RuntimeWorkerException(String.Format(
                        LocaleMessages.GetInstance().GetMessage(LocaleMessages.INVALID_NESTED_TAG), thetag,
                        ctx.GetCurrentTag().Name));
            }
            IPipeline wp = rootpPipe;
            ProcessObject po = new ProcessObject();
            try {
                while (null != (wp = wp.Close(ctx, ctx.GetCurrentTag(), po)))
                    ;
            } catch (PipelineException e) {
                throw new RuntimeWorkerException(e);
            } finally {
                if (null != ctx.GetCurrentTag())
                    ctx.SetCurrentTag(ctx.GetCurrentTag().Parent);
            }
        }

        /**
         * This method passes encountered text to the pipeline via the
         * {@link Pipeline#content(WorkerContext, Tag, byte[], ProcessObject)}
         * method.
         */
        virtual public void Text(String text) {
            if (text.StartsWith("<![CDATA[") && text.EndsWith("]]>"))
            {
                if (IgnoreCdata())
                    return;
                else
                    text = text.Substring(9, text.Length - 12);
            }
            IWorkerContext ctx = GetLocalWC();
            if (null != ctx.GetCurrentTag()) {
                if (text.Length > 0) {
                    IPipeline wp = rootpPipe;
                    ProcessObject po = new ProcessObject();
                    try {
                        while (null != (wp = wp.Content(ctx, ctx.GetCurrentTag(), text, po)));
                    } catch (PipelineException e) {
                        throw new RuntimeWorkerException(e);
                    }
                }
            }
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.parser.ParserListener#unknownText(java.lang.String)
         */
        virtual public void UnknownText(String text) {
            // TODO unknown text encountered
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.parser.ParserListener#comment(java.lang.String)
         */
        virtual public void Comment(String comment) {
            // TODO xml comment encountered
        }

        virtual public void Close() {
            CloseLocalWC();
        }

        /**
         * Returns the current tag.
         * @return the current tag
         */
        virtual protected internal Tag GetCurrentTag() {
            return GetLocalWC().GetCurrentTag();
        }

        /**
         * Returns the local WorkerContext, beware: could be a newly initialized
         * one, if {@link XMLWorker#close()} has been called before.
         *
         * @return the local WorkerContext
         */
        static protected internal IWorkerContext GetLocalWC() {
            IWorkerContext ik = (IWorkerContext)Thread.GetData(context);
            if (ik == null) {
                ik = new WorkerContextImpl();
                Thread.SetData(context, ik);
            }
            return ik;
        }

        static protected internal void CloseLocalWC() {
            Thread.SetData(context, null);
        }

        virtual protected bool IgnoreCdata()
        {
            return true;
        }
    }
}
