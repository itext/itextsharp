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
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
namespace iTextSharp.tool.xml {


    /**
     * A helper class for parsing XHTML/CSS or XML flow to PDF.
     *
     * @author redlab_b
     *
     */
    public class XMLWorkerHelper {

        private static XMLWorkerHelper myself = new XMLWorkerHelper();
        private ICssFile defaultCssFile;
        private ITagProcessorFactory tpf;

        /**
         * Get a Singleton XMLWorkerHelper
         *
         * @return a singleton instance of XMLWorkerHelper
         */
        public static XMLWorkerHelper GetInstance() {
            return myself;

        }

        /**
         */
        private XMLWorkerHelper() {

        }

        /**
         * @return the default css file.
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
    	public static ICssFile GetCSS(Stream inp) {
            ICssFile cssFile = null; 
			if (null != inp) {
				CssFileProcessor cssFileProcessor = new CssFileProcessor();
                try {
                    int i = -1;
                    while (-1 != (i = inp.ReadByte())) {
                        cssFileProcessor.Process((char) i);
                    }
                    cssFile = new CSSFileWrapper(cssFileProcessor.GetCss(), true);
                } catch (IOException e) {
                    throw new RuntimeWorkerException(e);
                } finally {
                    try {
                        inp.Close();
                    } catch (IOException e) {
                        throw new RuntimeWorkerException(e);
                    }
                }
            }
            return cssFile;
        }

        virtual public ICssFile GetDefaultCSS() {
            if (defaultCssFile == null) {
                defaultCssFile = GetCSS(Assembly.GetExecutingAssembly().GetManifestResourceStream("iTextSharp.tool.xml.css.default.css"));
            }
            return defaultCssFile;
        }

        /**
         * Parses the xml data in the given reader and sends created {@link Element}
         * s to the defined ElementHandler.<br />
         * This method configures the XMLWorker and XMLParser to parse (X)HTML/CSS
         * and accept unknown tags.
         *
         * @param d the handler
         * @param in the reader
         * @throws IOException thrown when something went wrong with the IO
         */
        virtual public void ParseXHtml(IElementHandler d, TextReader inp) {
            CssFilesImpl cssFiles = new CssFilesImpl();
            cssFiles.Add(GetDefaultCSS());
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc = new HtmlPipelineContext(null);
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(GetDefaultTagProcessorFactory());
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(hpc, new ElementHandlerPipeline(d,
                    null)));
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser();
            p.AddListener(worker);
            p.Parse(inp);
        }

        /**
         * Parses the xml data. This method configures the XMLWorker to parse
         * (X)HTML/CSS and accept unknown tags. Writes the output in the given
         * PdfWriter with the given document.
         *
         * @param writer the PdfWriter
         * @param doc the Document
         * @param inp the reader
         * @throws IOException thrown when something went wrong with the IO
         */
        virtual public void ParseXHtml(PdfWriter writer, Document doc, TextReader inp) {
            CssFilesImpl cssFiles = new CssFilesImpl();
            cssFiles.Add(GetDefaultCSS());
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc = new HtmlPipelineContext(null);
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(GetDefaultTagProcessorFactory());
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(hpc, new PdfWriterPipeline(doc,
                    writer)));
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser();
            p.AddListener(worker);
            p.Parse(inp);
        }

        virtual public void ParseXHtml(PdfWriter writer, Document doc, Stream inp, Stream inCssFile, Encoding charset,
            IFontProvider fontProvider) {
            ParseXHtml(writer, doc, inp, inCssFile, charset, fontProvider, null);
        }

        virtual public void ParseXHtml(PdfWriter writer, Document doc, Stream inp, Stream inCssFile, Encoding charset, IFontProvider fontProvider, string resourcesRootPath) {
            CssFilesImpl cssFiles = new CssFilesImpl();
            if (inCssFile != null)
                cssFiles.Add(GetCSS(inCssFile));
            else
                cssFiles.Add(GetDefaultCSS());
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc = new HtmlPipelineContext(new CssAppliersImpl(fontProvider));
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(GetDefaultTagProcessorFactory()).ResourcePath = resourcesRootPath;
            HtmlPipeline htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, writer));
            IPipeline pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser(true, worker, charset);
            if (charset != null) {
                p.Parse(inp, charset);
            } else {
                p.Parse(inp);
            }
        }

        virtual public void ParseXHtml(PdfWriter writer, Document doc, Stream inp, Stream inCssFile) {
            ParseXHtml(writer, doc, inp, inCssFile, null, new XMLWorkerFontProvider());
        }

        virtual public void ParseXHtml(PdfWriter writer, Document doc, Stream inp, Stream inCssFile, Encoding charset) {
            ParseXHtml(writer, doc, inp, inCssFile, charset, new XMLWorkerFontProvider());
        }

        virtual public void ParseXHtml(PdfWriter writer, Document doc, Stream inp, Encoding charset) {
            ParseXHtml(writer, doc, inp,
                Assembly.GetExecutingAssembly().GetManifestResourceStream("iTextSharp.tool.xml.css.default.css"),
                charset);
        }

        virtual public void ParseXHtml(PdfWriter writer, Document doc, Stream inp, Stream inCssFile, IFontProvider fontProvider) {
            ParseXHtml(writer, doc, inp, inCssFile, null, fontProvider);
        }

        /**
         * @param d the ElementHandler
         * @param inp the Stream
         * @throws IOException if something went seriously wrong with IO.
         */
        virtual public void ParseXHtml(IElementHandler d, Stream inp, Encoding charset) {
            CssFilesImpl cssFiles = new CssFilesImpl();
            cssFiles.Add(GetDefaultCSS());
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc = new HtmlPipelineContext(null);
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(GetDefaultTagProcessorFactory());
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(hpc, new ElementHandlerPipeline(d,
                    null)));
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser(true, worker, charset);
            if (charset != null) {
                p.Parse(inp, charset);
            } else {
                p.Parse(inp);
            }
        }

        /**
         * Get a CSSResolver implementation.
         *
         * @param addDefaultCss true if the defaultCss should already be added.
         * @return the default CSSResolver
         *
         */
        virtual public ICSSResolver GetDefaultCssResolver(bool addDefaultCss) {
            ICSSResolver resolver = new StyleAttrCSSResolver();
            if (addDefaultCss) {
                resolver.AddCss(GetDefaultCSS());
            }
            return resolver;
        }

        /**
         * Retrieves the default factory for processing HTML tags from
         * {@link Tags#getHtmlTagProcessorFactory()}. On subsequent calls the same
         * {@link TagProcessorFactory} is returned every time. <br />
         * @return a
         *         <code>DefaultTagProcessorFactory<code> that maps HTML tags to {@link TagProcessor}s
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        virtual protected internal ITagProcessorFactory GetDefaultTagProcessorFactory() {
            if (null == tpf) {
                tpf = Tags.GetHtmlTagProcessorFactory();
            }
            return tpf;
        }

        /**
         * Parses an HTML string and a string containing CSS into a list of Element objects.
         * The FontProvider will be obtained from iText's FontFactory object.
         * 
         * @param   html    a String containing an XHTML snippet
         * @param   css     a String containing CSS
         * @return  an ElementList instance
         */
        public static ElementList ParseToElementList(String html, String css) {
            // CSS
            ICSSResolver cssResolver = new StyleAttrCSSResolver();
            if (css != null) {
                ICssFile cssFile = XMLWorkerHelper.GetCSS(new MemoryStream(Encoding.Default.GetBytes(css)));
                cssResolver.AddCss(cssFile);
            }

            // HTML
            CssAppliers cssAppliers = new CssAppliersImpl(FontFactory.FontImp);
            HtmlPipelineContext htmlContext = new HtmlPipelineContext(cssAppliers);
            htmlContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            htmlContext.AutoBookmark(false);

            // Pipelines
            ElementList elements = new ElementList();
            ElementHandlerPipeline end = new ElementHandlerPipeline(elements, null);
            HtmlPipeline htmlPipeline = new HtmlPipeline(htmlContext, end);
            CssResolverPipeline cssPipeline = new CssResolverPipeline(cssResolver, htmlPipeline);

            // XML Worker
            XMLWorker worker = new XMLWorker(cssPipeline, true);
            XMLParser p = new XMLParser(worker);
            p.Parse(new MemoryStream(Encoding.Default.GetBytes(html)));

            return elements;
        }
    }
}
