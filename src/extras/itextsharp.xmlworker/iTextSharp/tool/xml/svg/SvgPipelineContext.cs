using System;
using System.Collections.Generic;
using System.Text;

using iTextSharp.text;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;

/*
 * $Id: $
 *
 /// * This file is part of the iText (R) project.
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

    public class SvgPipelineContext : ICustomContext, ICloneable
    {
        /**
         *  Key for the memory, used to store bookmark nodes
         */
        public static String BOOKMARK_TREE = "header.autobookmark.RootNode";
        /**
         * Key for the memory, used in Html TagProcessing
         */
        public static String LAST_MARGIN_BOTTOM = "lastMarginBottom";
        private LinkedList<StackKeeper> queue;
        private bool acceptUnknown = true;
        private ITagProcessorFactory tagFactory;
        private IList<IElement> ctn = new List<IElement>();
        private IImageProvider imageProvider;
        private Rectangle pageSize = PageSize.A4;
        private Encoding charset;
        private IList<String> roottags = new List<String>(new String[] { "body", "div" });
        private ILinkProvider linkprovider;
        private bool autoBookmark = true;
        private bool definition = false;

        public bool IsDefinition()
        {
            return definition;
        }
        public void SetDefinition(bool definition)
        {
            this.definition = definition;
        }

        private IDictionary<String, IList<IElement>> symbols;

        /**
         * Construct a new CvgPipelineContext object
         */
        public SvgPipelineContext()
        {
            this.queue = new LinkedList<StackKeeper>();
            this.symbols = new Dictionary<String, IList<IElement>>();
        }
        /**
         * @param tag the tag to find a TagProcessor for
         * @param nameSpace the namespace.
         * @return a TagProcessor
         */
        protected internal ITagProcessor ResolveProcessor(String tag, String nameSpace)
        {
            ITagProcessor tp = tagFactory.GetProcessor(tag, nameSpace);
            return tp;
        }

        /**
         * Add a {@link StackKeeper} to the top of the stack list.
         * @param stackKeeper the {@link StackKeeper}
         */
        protected internal void AddFirst(StackKeeper stackKeeper)
        {
            this.queue.AddFirst(stackKeeper);
        }

        /**
         * Retrieves, but does not remove, the head (first element) of this list.
         * @return a StackKeeper
         * @throws NoStackException if there are no elements on the stack
         */
        protected internal StackKeeper Peek()
        {
            if (queue.Count == 0)
                throw new NoStackException();
            return this.queue.First.Value;

        }

        /**
         * @return the current content of elements.
         */
        protected internal IList<IElement> CurrentContent()
        {
            return ctn;
        }

        /**
         * @return if this pipelines tag processing accept unknown tags: true. False otherwise
         */
        public bool AcceptUnknown()
        {
            return this.acceptUnknown;
        }

        /**
         * @return returns true if the stack is empty
         */
        protected bool IsEmpty()
        {
            return queue.Count > 0;
        }

        /**
         * Retrieves and removes the top of the stack.
         * @return a StackKeeper
         * @throws NoStackException if there are no elements on the stack
         */
        protected internal StackKeeper Poll()
        {
            try
            {
                StackKeeper first = this.queue.First.Value;
                this.queue.RemoveFirst();
                return first;
            } catch (InvalidOperationException e) {
                throw new NoStackException();
            }
        }
        /**
         * @return true if auto-bookmarks should be enabled. False otherwise.
         */
        public bool AutoBookmark()
        {
            return autoBookmark;
        }

        public IList<IElement> GetSymbolById(String id)
        {
            return symbols[id];
        }

        public void AddSymbolById(String id, IList<IElement> elements)
        {
            symbols.Add(id, elements);
        }

        /**
         * @return the image provider.
         *
         */
        public IImageProvider GetImageProvider()
        {
            if (null == this.imageProvider)
            {
                throw new NoImageProviderException();
            }
            return this.imageProvider;

        }

        /**
         * Set a {@link Charset} to use.
         * @param cSet the charset.
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext CharSet(Encoding cSet)
        {
            this.charset = cSet;
            return this;
        }
        /**
         * @return the {@link Charset} to use, or null if none configured.
         */
        public Encoding CharSet()
        {
            return charset;
        }
        /**
         * Returns a {@link Rectangle}
         * @return the pagesize.
         */
        public Rectangle GetPageSize()
        {
            return this.pageSize;
        }

        /**
         * @return a list of tags to be taken as root-tags. This matters for
         *         margins. By default the root-tags are &lt;body&gt; and
         *         &lt;div&gt;
         */
        public IList<String> GetRootTags()
        {
            return roottags;
        }

        /**
         * Returns the LinkProvider, used to prepend e.g. http://www.example.org/ to
         * found &lt;a&gt; tags that have no absolute url.
         *
         * @return the LinkProvider if any.
         */
        public ILinkProvider GetLinkProvider()
        {
            return linkprovider;
        }
        /**
         * If no pageSize is set, the default value A4 is used.
         * @param pageSize the pageSize to set
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext SetPageSize(Rectangle pageSize)
        {
            this.pageSize = pageSize;
            return this;
        }

        /**
         * Create a clone of this HtmlPipelineContext, the clone only contains the
         * initial values, not the internal values. Beware, the state of the current
         * Context is not copied to the clone. Only the configurational important
         * stuff like the LinkProvider (same object), ImageProvider (new
         * {@link AbstractImageProvider} with same ImageRootPath) ,
         * TagProcessorFactory (same object), acceptUnknown (primitive), charset
         * (Charset.forName to get a new charset), autobookmark (primitive) are
         * copied.
         */
        private class CloneImageProvider : AbstractImageProvider
        {
            private string rootPath;

            public CloneImageProvider(string rootPath)
            {
                this.rootPath = rootPath;
            }

            public override String GetImageRootPath()
            {
                return rootPath;
            }
        }

        public Object Clone()
        {
            SvgPipelineContext newCtx = new SvgPipelineContext();
            if (this.imageProvider != null)
            {
                newCtx.SetImageProvider(new CloneImageProvider(imageProvider.GetImageRootPath()));
            }
            if (null != this.charset)
            {
                newCtx.CharSet(Encoding.GetEncoding(this.charset.EncodingName));
            }
            newCtx.SetPageSize(new Rectangle(this.pageSize)).SetLinkProvider(this.linkprovider)
                    .SetRootTags(new List<String>(this.roottags)).AutoBookmark(this.autoBookmark)
                    .SetTagFactory(this.tagFactory).SetAcceptUnknown(this.acceptUnknown);
            return newCtx;
        }


        /**
         * Set to true to allow the HtmlPipeline to accept tags it does not find in
         * the given {@link TagProcessorFactory}
         *
         * @param acceptUnknown true or false
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext SetAcceptUnknown(bool acceptUnknown)
        {
            this.acceptUnknown = acceptUnknown;
            return this;
        }
        /**
         * Set the {@link TagProcessorFactory} to be used. For HTML use {@link Tags#getHtmlTagProcessorFactory()}
         * @param tagFactory the {@link TagProcessorFactory} that should be used
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext SetTagFactory(ITagProcessorFactory tagFactory)
        {
            this.tagFactory = tagFactory;
            return this;
        }

        /**
         * Set to true to enable the automatic creation of bookmarks on &lt;h1&gt;
         * to &lt;h6&gt; tags. Works in conjunction with {@link Header}.
         *
         * @param autoBookmark true or false
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext AutoBookmark(bool autoBookmark)
        {
            this.autoBookmark = autoBookmark;
            return this;
        }

        /**
         * Set the root-tags, this matters for margins. By default these are set to
         * &lt;body&gt; and &lt;div&gt;.
         *
         * @param roottags the root tags
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext SetRootTags(IList<String> roottags)
        {
            this.roottags = roottags;
            return this;
        }

        /**
         * An ImageProvider can be provided and works in conjunction with
         * {@link Image} and {@link ListStyleTypeCssApplier} for List Images.
         *
         * @param imageProvider the {@link ImageProvider} to use.
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext SetImageProvider(IImageProvider imageProvider)
        {
            this.imageProvider = imageProvider;
            return this;
        }

        /**
         * Set the LinkProvider to use if any.
         *
         * @param linkprovider the LinkProvider (@see
         *            {@link HtmlPipelineContext#getLinkProvider()}
         * @return this <code>HtmlPipelineContext</code>
         */
        public SvgPipelineContext SetLinkProvider(ILinkProvider linkprovider)
        {
            this.linkprovider = linkprovider;
            return this;
        }
    }
}
